using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Nodemap.UI;

/// <summary>
/// Editor tool to automatically generate slide prefabs from images and populate SlideDeck assets.
/// This eliminates the manual process of creating prefab variants for each slide image.
/// </summary>
public class SlideGeneratorEditor : EditorWindow
{
    [Header("Required References")]
    [SerializeField] private GameObject slideTemplate;
    [SerializeField] private string imageFolder = "Assets/Resources/Slides/";
    [SerializeField] private string prefabOutputFolder = "Assets/Prefabs/Slides/";
    [SerializeField] private SlideDeck targetSlideDeck;
    
    [Header("Options")]
    [SerializeField] private string slidePrefix = "Slide";
    [SerializeField] private bool clearExistingSlides = false;
    [SerializeField] private bool overwriteExistingPrefabs = false;
    
    private Vector2 scrollPosition;

    [MenuItem("Tools/Slide Generator")]
    public static void ShowWindow()
    {
        GetWindow<SlideGeneratorEditor>("Slide Generator");
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.LabelField("Slide Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "This tool automatically generates slide prefabs from images and populates SlideDeck assets.\n\n" +
            "How it works:\n" +
            "1. Scans the image folder for Slide#.png files\n" +
            "2. Creates prefab variants from the slide template\n" +
            "3. Assigns sprites to each prefab\n" +
            "4. Populates the SlideDeck with generated slides in order\n\n" +
            "You can then manually add custom slides or minigames between slides.",
            MessageType.Info);
        
        EditorGUILayout.Space();
        
        // Template reference
        slideTemplate = EditorGUILayout.ObjectField(
            "Slide Template", 
            slideTemplate, 
            typeof(GameObject), 
            false
        ) as GameObject;
        
        // Image folder
        EditorGUILayout.BeginHorizontal();
        imageFolder = EditorGUILayout.TextField("Image Folder", imageFolder);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Image Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                imageFolder = FileUtil.GetProjectRelativePath(path);
            }
        }
        EditorGUILayout.EndHorizontal();
        
        // Prefab output folder
        EditorGUILayout.BeginHorizontal();
        prefabOutputFolder = EditorGUILayout.TextField("Prefab Output Folder", prefabOutputFolder);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets/Prefabs", "");
            if (!string.IsNullOrEmpty(path))
            {
                prefabOutputFolder = FileUtil.GetProjectRelativePath(path);
            }
        }
        EditorGUILayout.EndHorizontal();
        
        // Target SlideDeck
        targetSlideDeck = EditorGUILayout.ObjectField(
            "Target SlideDeck", 
            targetSlideDeck, 
            typeof(SlideDeck), 
            false
        ) as SlideDeck;
        
        EditorGUILayout.Space();
        
        // Options
        EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
        slidePrefix = EditorGUILayout.TextField("Slide Prefix", slidePrefix);
        clearExistingSlides = EditorGUILayout.Toggle("Clear Existing Slides", clearExistingSlides);
        overwriteExistingPrefabs = EditorGUILayout.Toggle("Overwrite Existing Prefabs", overwriteExistingPrefabs);
        
        EditorGUILayout.Space();
        
        // Preview
        if (slideTemplate != null && Directory.Exists(imageFolder))
        {
            var images = GetSortedSlideImages(imageFolder);
            EditorGUILayout.LabelField($"Found {images.Count} slide images", EditorStyles.miniLabel);
            
            if (images.Count > 0)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Preview:", EditorStyles.boldLabel);
                foreach (var img in images.Take(5))
                {
                    EditorGUILayout.LabelField($"  • {img.name} → {slidePrefix}{img.slideNumber}", EditorStyles.miniLabel);
                }
                if (images.Count > 5)
                {
                    EditorGUILayout.LabelField($"  ... and {images.Count - 5} more", EditorStyles.miniLabel);
                }
                EditorGUILayout.EndVertical();
            }
        }
        
        EditorGUILayout.Space();
        
        // Generate button
        GUI.enabled = slideTemplate != null && targetSlideDeck != null && 
                      Directory.Exists(imageFolder) && !string.IsNullOrEmpty(prefabOutputFolder);
        
        if (GUILayout.Button("Generate Slides", GUILayout.Height(30)))
        {
            GenerateSlides();
        }
        
        GUI.enabled = true;
        
        EditorGUILayout.EndScrollView();
    }

    private void GenerateSlides()
    {
        if (!EditorUtility.DisplayDialog(
            "Generate Slides",
            $"This will generate slide prefabs and update the SlideDeck.\n\n" +
            $"Clear existing slides: {clearExistingSlides}\n" +
            $"Overwrite existing prefabs: {overwriteExistingPrefabs}\n\n" +
            "Continue?",
            "Yes",
            "Cancel"))
        {
            return;
        }

        // Ensure output folder exists
        if (!Directory.Exists(prefabOutputFolder))
        {
            Directory.CreateDirectory(prefabOutputFolder);
            AssetDatabase.Refresh();
        }

        var images = GetSortedSlideImages(imageFolder);
        if (images.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "No slide images found in the specified folder.", "OK");
            return;
        }

        // Clear existing slides if requested
        if (clearExistingSlides)
        {
            targetSlideDeck.slides.Clear();
        }

        int successCount = 0;
        int skippedCount = 0;

        EditorUtility.DisplayProgressBar("Generating Slides", "Processing...", 0f);

        try
        {
            for (int i = 0; i < images.Count; i++)
            {
                var slideImage = images[i];
                float progress = (float)i / images.Count;
                EditorUtility.DisplayProgressBar("Generating Slides", $"Processing {slideImage.name}...", progress);

                string slideName = $"{slidePrefix}{slideImage.slideNumber}";
                string prefabPath = Path.Combine(prefabOutputFolder, $"{slideName}.prefab");

                // Check if prefab exists
                GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                
                if (existingPrefab != null && !overwriteExistingPrefabs)
                {
                    Debug.Log($"Skipped {slideName} - prefab already exists");
                    skippedCount++;
                    
                    // Still add to slide deck if not already present
                    if (!targetSlideDeck.slides.Any(s => s.key == slideName))
                    {
                        targetSlideDeck.slides.Add(new SlideRef
                        {
                            key = slideName,
                            slidePrefab = existingPrefab
                        });
                    }
                    continue;
                }

                // Create or update prefab
                GameObject slidePrefab = CreateSlidePrefab(slideName, slideImage.sprite);
                
                if (slidePrefab != null)
                {
                    // Save as prefab
                    GameObject savedPrefab;
                    if (existingPrefab != null)
                    {
                        // Replace existing
                        savedPrefab = PrefabUtility.SaveAsPrefabAsset(slidePrefab, prefabPath);
                    }
                    else
                    {
                        // Create new
                        savedPrefab = PrefabUtility.SaveAsPrefabAsset(slidePrefab, prefabPath);
                    }
                    
                    DestroyImmediate(slidePrefab);

                    // Add to slide deck (or update existing)
                    var existingSlide = targetSlideDeck.slides.FirstOrDefault(s => s.key == slideName);
                    if (existingSlide != null)
                    {
                        existingSlide.slidePrefab = savedPrefab;
                    }
                    else
                    {
                        targetSlideDeck.slides.Add(new SlideRef
                        {
                            key = slideName,
                            slidePrefab = savedPrefab
                        });
                    }

                    successCount++;
                }
            }

            // Sort slides by slide number
            targetSlideDeck.slides = targetSlideDeck.slides
                .OrderBy(s => ExtractSlideNumber(s.key))
                .ToList();

            // Save changes
            EditorUtility.SetDirty(targetSlideDeck);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Success",
                $"Generated {successCount} slides\n" +
                $"Skipped {skippedCount} existing slides\n" +
                $"Total in deck: {targetSlideDeck.slides.Count}",
                "OK"
            );
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private GameObject CreateSlidePrefab(string name, Sprite sprite)
    {
        // Instantiate from template
        GameObject slideObject = PrefabUtility.InstantiatePrefab(slideTemplate) as GameObject;
        if (slideObject == null)
        {
            Debug.LogError("Failed to instantiate slide template");
            return null;
        }

        slideObject.name = name;

        // Update SlideBase key
        SlideBase slideBase = slideObject.GetComponent<SlideBase>();
        if (slideBase != null)
        {
            SerializedObject so = new SerializedObject(slideBase);
            so.FindProperty("key").stringValue = name;
            so.ApplyModifiedProperties();
        }

        // Find and update the Image component
        Image[] images = slideObject.GetComponentsInChildren<Image>(true);
        Image slideImage = images.FirstOrDefault(img => img.gameObject.name == "Slide");
        
        if (slideImage != null)
        {
            slideImage.sprite = sprite;
        }
        else
        {
            Debug.LogWarning($"Could not find Image component named 'Slide' in {name}");
        }

        return slideObject;
    }

    private List<SlideImageInfo> GetSortedSlideImages(string folderPath)
    {
        var slideImages = new List<SlideImageInfo>();

        if (!Directory.Exists(folderPath))
        {
            Debug.LogWarning($"Folder does not exist: {folderPath}");
            return slideImages;
        }

        // Find all Slide#.png files
        var files = Directory.GetFiles(folderPath, "Slide*.png", SearchOption.TopDirectoryOnly);
        Debug.Log($"Found {files.Length} .png files in {folderPath}");

        foreach (var file in files)
        {
            string filename = Path.GetFileNameWithoutExtension(file);
            int slideNumber = ExtractSlideNumber(filename);

            if (slideNumber > 0)
            {
                // Convert absolute path to relative Unity path
                string relativePath = file.Replace("\\", "/");
                if (relativePath.Contains("Assets"))
                {
                    int assetsIndex = relativePath.IndexOf("Assets");
                    relativePath = relativePath.Substring(assetsIndex);
                }
                
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(relativePath);

                if (sprite != null)
                {
                    slideImages.Add(new SlideImageInfo
                    {
                        name = filename,
                        slideNumber = slideNumber,
                        sprite = sprite,
                        path = relativePath
                    });
                }
                else
                {
                    Debug.LogWarning($"Failed to load sprite at: {relativePath}");
                }
            }
        }

        // Sort by slide number
        slideImages.Sort((a, b) => a.slideNumber.CompareTo(b.slideNumber));

        return slideImages;
    }

    private int ExtractSlideNumber(string text)
    {
        if (string.IsNullOrEmpty(text))
            return -1;

        Match match = Regex.Match(text, @"(\d+)");
        if (match.Success && int.TryParse(match.Groups[1].Value, out int number))
        {
            return number;
        }

        return -1;
    }

    private class SlideImageInfo
    {
        public string name;
        public int slideNumber;
        public Sprite sprite;
        public string path;
    }
}
