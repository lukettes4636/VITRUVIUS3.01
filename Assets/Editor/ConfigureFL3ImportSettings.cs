using UnityEngine;
using UnityEditor;

public class ConfigureFL3ImportSettings : AssetPostprocessor
{
    [InitializeOnLoadMethod]
    public static void FixFL3()
    {
        string path = "Assets/FL3.png";
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        
        if (importer != null)
        {
            // Set Texture Type to Sprite (UI and 2D)
            importer.textureType = TextureImporterType.Sprite;
            
            // Disable Compression
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            
            // Ensure no resizing (NPOT Scale: None)
            importer.npotScale = TextureImporterNPOTScale.None;
            
            // Disable Mipmaps (crisper for UI)
            importer.mipmapEnabled = false;
            
            // High quality settings
            importer.filterMode = FilterMode.Bilinear; // or Point for pixel art, but Bilinear usually good for high res logos
            
            // Apply changes
            importer.SaveAndReimport();
            
            Debug.Log("Fixed FL3 Import Settings: Uncompressed, Original Size, No Mipmaps.");
        }
        else
        {
            Debug.LogError("Could not find FL3 at " + path);
        }
    }
}
