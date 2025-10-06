using UnityEngine;

namespace Nodemap.Core
{
    /// <summary>
    /// Base class that provides clean config access without boilerplate.
    /// Eliminates the repeated GetConfigValue pattern across all classes.
    /// </summary>
    public abstract class ConfigurableComponent : MonoBehaviour
    {
        [SerializeField] protected MapConfig mapConfig;

        protected virtual void Awake()
        {
            InitializeConfig();
        }

        private void InitializeConfig()
        {
            if (mapConfig == null)
            {
                mapConfig = MapConfig.Instance;
            }
        }

        /// <summary>
        /// Safe config access with fallback. Eliminates null checking boilerplate.
        /// </summary>
        protected T GetConfig<T>(System.Func<MapConfig, T> accessor, T fallback = default)
        {
            return mapConfig != null ? accessor(mapConfig) : fallback;
        }

        /// <summary>
        /// Ensures config is available for classes that need it in Start or later.
        /// </summary>
        protected void EnsureConfigInitialized()
        {
            if (mapConfig == null)
            {
                InitializeConfig();
            }
        }
    }

    /// <summary>
    /// Static utility for config access in non-MonoBehaviour classes.
    /// Provides consistent config access patterns across the codebase.
    /// </summary>
    public static class ConfigHelper
    {
        private static MapConfig _cachedConfig;

        public static MapConfig Config
        {
            get
            {
                if (_cachedConfig == null)
                {
                    _cachedConfig = MapConfig.Instance;
                }
                return _cachedConfig;
            }
        }

        public static T GetValue<T>(System.Func<MapConfig, T> accessor, T fallback = default)
        {
            return Config != null ? accessor(Config) : fallback;
        }

        /// <summary>
        /// Clears cached config. Call this if config changes at runtime.
        /// </summary>
        public static void RefreshConfig()
        {
            _cachedConfig = null;
        }
    }
}