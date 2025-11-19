using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Adds context menu options to quickly open Slide Generator with pre-filled settings
/// based on selected folders or assets.
/// </summary>
public static class SlideGeneratorContextMenu
{
    /// <summary>
    /// Context menu for folders: Right-click a folder to generate slides from it
    /// </summary>
    [MenuItem("Assets/Generate Slides from This Folder", false, 1000)]
    private static void GenerateSlidesFromFolder()
    {
        // Get selected folder
        string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        
        if (string.IsNullOrEmpty(selectedPath) || !Directory.Exists(selectedPath))
        {
            EditorUtility.DisplayDialog("Error",
                "Please select a folder containing slide images.",
                "OK");
            return;
        }
        
        // Try to auto-detect topic name from path
        string folderName = Path.GetFileName(selectedPath);
        string parentFolder = Path.GetFileName(Path.GetDirectoryName(selectedPath));
        
        // Check if this looks like a Slides folder
        if (parentFolder == "Slides" && selectedPath.Contains("Resources"))
        {
            // This is likely Resources/Slides/[Topic]
            AutoConfigureAndOpenGenerator(selectedPath, folderName);
        }
        else
        {
            // Just open the generator with the folder pre-filled
            var window = EditorWindow.GetWindow<SlideGeneratorEditor>("Slide Generator");
            
            // Use reflection to set the image folder
            var imageFolder = window.GetType().GetField("imageFolder", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (imageFolder != null)
            {
                imageFolder.SetValue(window, selectedPath);
            }
            
            window.Show();
        }
    }
    
    [MenuItem("Assets/Generate Slides from This Folder", true)]
    private static bool ValidateGenerateSlidesFromFolder()
    {
        // Only show menu if a folder is selected
        if (Selection.activeObject == null)
            return false;
        
        string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        return Directory.Exists(selectedPath);
    }
    
    /// <summary>
    /// Auto-configure the generator based on folder structure conventions
    /// </summary>
    private static void AutoConfigureAndOpenGenerator(string imageFolderPath, string topicName)
    {
        // Derive paths from convention
        string prefabOutputPath = $"Assets/Prefabs/Slides/{topicName}";
        string slideDeckPath = $"Assets/Data/Slides/{topicName}Slides.asset";
        string slidePrefix = $"{topicName}Slide";
        
        // Load or create necessary assets
        GameObject template = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Slides/SlideTemplate.prefab");
        SlideDeck slideDeck = AssetDatabase.LoadAssetAtPath<SlideDeck>(slideDeckPath);
        
        // Check if paths exist
        bool hasTemplate = template != null;
        bool hasSlideDeck = slideDeck != null;
        bool prefabFolderExists = Directory.Exists(prefabOutputPath);
        
        // Show confirmation dialog with auto-detected settings
        string message = $"Auto-detected settings:\n\n" +
            $"Topic: {topicName}\n" +
            $"Image Folder: {imageFolderPath}\n" +
            $"Prefab Output: {prefabOutputPath} {(prefabFolderExists ? "✓" : "(will create)")}\n" +
            $"SlideDeck: {slideDeckPath} {(hasSlideDeck ? "✓" : "✗ NOT FOUND")}\n" +
            $"Prefix: {slidePrefix}\n" +
            $"Template: {(hasTemplate ? "✓" : "✗ NOT FOUND")}\n\n";
        
        if (!hasTemplate || !hasSlideDeck)
        {
            message += "⚠ Missing required assets!\n\n";
            
            if (!hasTemplate)
                message += "• SlideTemplate.prefab not found\n";
            if (!hasSlideDeck)
                message += $"• {topicName}Slides.asset not found\n";
            
            message += "\nPlease create missing assets first or configure manually.";
            
            EditorUtility.DisplayDialog("Cannot Auto-Configure", message, "OK");
            
            // Open generator anyway so user can configure manually
            EditorWindow.GetWindow<SlideGeneratorEditor>("Slide Generator").Show();
            return;
        }
        
        bool proceed = EditorUtility.DisplayDialog(
            "Generate Slides",
            message + "Open Slide Generator with these settings?",
            "Yes",
            "Cancel"
        );
        
        if (!proceed)
            return;
        
        // Create prefab folder if it doesn't exist
        if (!prefabFolderExists)
        {
            Directory.CreateDirectory(prefabOutputPath);
            AssetDatabase.Refresh();
        }
        
        // Open and configure the window
        var window = EditorWindow.GetWindow<SlideGeneratorEditor>("Slide Generator");
        
        // Use reflection to set private fields
        var type = window.GetType();
        SetField(type, window, "slideTemplate", template);
        SetField(type, window, "imageFolder", imageFolderPath);
        SetField(type, window, "prefabOutputFolder", prefabOutputPath);
        SetField(type, window, "targetSlideDeck", slideDeck);
        SetField(type, window, "slidePrefix", slidePrefix);
        
        window.Show();
        window.Repaint();
        
        Debug.Log($"Slide Generator opened for topic: {topicName}");
    }
    
    private static void SetField(System.Type type, object instance, string fieldName, object value)
    {
        var field = type.GetField(fieldName, 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            field.SetValue(instance, value);
        }
    }
    
    /// <summary>
    /// Quick menu to create a new SlideDeck asset
    /// </summary>
    [MenuItem("Assets/Create/Content/Slide Deck (Quick)", priority = 81)]
    private static void CreateSlideDeckQuick()
    {
        // Get selected folder or use Assets
        string path = "Assets";
        if (Selection.activeObject != null)
        {
            path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!Directory.Exists(path))
            {
                path = Path.GetDirectoryName(path);
            }
        }
        
        // Create the asset
        SlideDeck deck = ScriptableObject.CreateInstance<SlideDeck>();
        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{path}/NewSlideDeck.asset");
        
        AssetDatabase.CreateAsset(deck, assetPath);
        AssetDatabase.SaveAssets();
        
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = deck;
        
        Debug.Log($"Created new SlideDeck at: {assetPath}");
    }
}
