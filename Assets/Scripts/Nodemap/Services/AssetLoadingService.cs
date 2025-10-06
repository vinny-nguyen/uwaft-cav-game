using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Nodemap.Services
{
    /// <summary>
    /// Centralized asset loading service that manages Addressables lifecycle properly.
    /// Eliminates the dangerous patterns of nullable handles and improper cleanup.
    /// </summary>
    public class AssetLoadingService : IDisposable
    {
        private readonly Dictionary<string, LoadedAsset<Sprite>> _loadedSprites = new();
        private readonly Dictionary<string, AsyncOperationHandle<Sprite>> _loadingHandles = new();

        public void LoadSpriteAsync(string address, Action<Sprite> onLoaded, Action<string> onError = null)
        {
            // Return cached sprite immediately if available
            if (_loadedSprites.TryGetValue(address, out var cached) && cached.IsValid)
            {
                onLoaded?.Invoke(cached.Asset);
                return;
            }

            // Cancel existing load if in progress
            if (_loadingHandles.TryGetValue(address, out var existingHandle))
            {
                if (existingHandle.IsValid())
                {
                    Addressables.Release(existingHandle);
                }
                _loadingHandles.Remove(address);
            }

            // Start new load
            var handle = Addressables.LoadAssetAsync<Sprite>(address);
            _loadingHandles[address] = handle;

            handle.Completed += (AsyncOperationHandle<Sprite> completedHandle) =>
            {
                // Remove from loading handles
                _loadingHandles.Remove(address);

                if (completedHandle.Status == AsyncOperationStatus.Succeeded && completedHandle.Result != null)
                {
                    // Cache the loaded sprite
                    _loadedSprites[address] = new LoadedAsset<Sprite>(completedHandle.Result, completedHandle);
                    onLoaded?.Invoke(completedHandle.Result);
                }
                else
                {
                    // Clean up failed handle
                    if (completedHandle.IsValid())
                    {
                        Addressables.Release(completedHandle);
                    }
                    
                    onError?.Invoke($"Failed to load sprite at address: {address}");
                }
            };
        }

        public Sprite GetCachedSprite(string address)
        {
            return _loadedSprites.TryGetValue(address, out var cached) && cached.IsValid ? cached.Asset : null;
        }

        public void UnloadSprite(string address)
        {
            if (_loadedSprites.TryGetValue(address, out var cached))
            {
                cached.Dispose();
                _loadedSprites.Remove(address);
            }
        }

        public void UnloadAll()
        {
            // Cancel all pending loads
            foreach (var handle in _loadingHandles.Values)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
            _loadingHandles.Clear();

            // Release all cached assets
            foreach (var loaded in _loadedSprites.Values)
            {
                loaded.Dispose();
            }
            _loadedSprites.Clear();
        }

        public void Dispose()
        {
            UnloadAll();
        }

        /// <summary>
        /// Helper class to safely manage loaded assets with their handles
        /// </summary>
        private class LoadedAsset<T> : IDisposable where T : UnityEngine.Object
        {
            public T Asset { get; private set; }
            public bool IsValid => Asset != null && _handle.IsValid();

            private readonly AsyncOperationHandle<T> _handle;

            public LoadedAsset(T asset, AsyncOperationHandle<T> handle)
            {
                Asset = asset;
                _handle = handle;
            }

            public void Dispose()
            {
                if (_handle.IsValid())
                {
                    Addressables.Release(_handle);
                }
                Asset = null;
            }
        }
    }

    /// <summary>
    /// Static service locator for easy access to asset loading.
    /// In a larger project, you'd use proper DI container.
    /// </summary>
    public static class AssetLoader
    {
        private static AssetLoadingService _instance;
        
        public static AssetLoadingService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AssetLoadingService();
                }
                return _instance;
            }
        }

        public static void Cleanup()
        {
            _instance?.Dispose();
            _instance = null;
        }

        // Convenience methods
        public static void LoadSprite(string address, Action<Sprite> onLoaded, Action<string> onError = null)
        {
            Instance.LoadSpriteAsync(address, onLoaded, onError);
        }

        public static Sprite GetCachedSprite(string address)
        {
            return Instance.GetCachedSprite(address);
        }
    }

    /// <summary>
    /// Utility for creating fallback sprites when assets fail to load
    /// </summary>
    public static class FallbackSpriteFactory
    {
        public static Sprite CreateColoredSprite(Color color, int size = 64)
        {
            Texture2D texture = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];
            
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;
                
            texture.SetPixels(pixels);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f);
        }

        public static Sprite CreateNodeFallback(NodeState state)
        {
            Color color = state switch
            {
                NodeState.Active => Color.yellow,
                NodeState.Completed => Color.green,
                _ => Color.gray
            };
            
            return CreateColoredSprite(color);
        }
    }
}