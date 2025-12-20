using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEditor.Events;
using System;

public class MainMenuSetupTool : EditorWindow
{
    public static void ShowWindow()
    {
        GetWindow<MainMenuSetupTool>("Setup Main Menu");
    }

    private void OnGUI()
    {
        GUILayout.Label("Main Menu 3D Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Setup 3D Background System"))
        {
            SetupSystem();
        }

        GUILayout.Space(6);
        if (GUILayout.Button("Fix Main Menu UI Events (Broken PPtr)"))
        {
            FixBrokenUIEvents();
        }

        GUILayout.Space(6);
        if (GUILayout.Button("Remove Missing Scripts From Scene"))
        {
            RemoveMissingScriptsInScene();
        }

        GUILayout.Space(6);
        if (GUILayout.Button("Improve Graphics (SSAO + Fog)"))
        {
            ImproveGraphics();
        }

        GUILayout.Space(10);
        GUILayout.Label("Instructions:", EditorStyles.boldLabel);
        GUILayout.Label("1. Open 'Main Menu' scene.");
        GUILayout.Label("2. Click 'Setup 3D Background System'.");
        GUILayout.Label("3. Verify the 'Background Camera' has the movement script.");
        GUILayout.Label("4. Verify the 'UI Camera' is in the Stack of Background Camera.");
    }

    private static void SetupSystem()
    {
        // 1. Setup Layers
        // We assume "UI" layer exists (index 5).
        // We might want a "Background" layer, but "Default" is fine for now if UI is strictly on UI layer.

        // 2. Setup Background Camera (Base)
        GameObject bgCamObj = GameObject.Find("Background Camera");
        if (bgCamObj == null)
        {
            bgCamObj = new GameObject("Background Camera");
            bgCamObj.tag = "MainCamera";
        }
        
        Camera bgCam = bgCamObj.GetComponent<Camera>();
        if (bgCam == null) bgCam = bgCamObj.AddComponent<Camera>();

        var bgCamData = bgCamObj.GetComponent<UniversalAdditionalCameraData>();
        if (bgCamData == null) bgCamData = bgCamObj.AddComponent<UniversalAdditionalCameraData>();

        bgCamData.renderType = CameraRenderType.Base;
        bgCam.cullingMask = ~(1 << LayerMask.NameToLayer("UI")); // Everything except UI
        bgCam.depth = -1;
        
        // Ensure AudioListener exists on Background Camera
        AudioListener listener = bgCamObj.GetComponent<AudioListener>();
        if (listener == null) listener = bgCamObj.AddComponent<AudioListener>();

        AudioSource bgAudio = bgCamObj.GetComponent<AudioSource>();
        if (bgAudio == null) bgAudio = bgCamObj.AddComponent<AudioSource>();
        bgAudio.playOnAwake = true;
        bgAudio.loop = true;
        bgAudio.spatialBlend = 0f;
        
        AudioClip musicClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Scenes/Start/SoundsLoadingMenu/music1.mp3");
        if (musicClip == null)
        {
            musicClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Scenes/Start/SoundsLoadingMenu/music2.mp3");
        }
        if (musicClip == null)
        {
            musicClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Scenes/Start/SoundsLoadingMenu/music3.mp3");
        }
        if (musicClip != null)
        {
            bgAudio.clip = musicClip;
        }
        
        AudioConfig audioCfg = AssetDatabase.LoadAssetAtPath<AudioConfig>("Assets/Shader/Renders/AudioConfig.asset");
        if (audioCfg != null && audioCfg.musicMixerGroup != null)
        {
            bgAudio.outputAudioMixerGroup = audioCfg.musicMixerGroup;
        }

        // Setup Fog and Background Color
        Color fogColor = Color.black;
        
        bgCam.clearFlags = CameraClearFlags.SolidColor;
        bgCam.backgroundColor = fogColor;
        
        // Setup Ambient Light for dark scene
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.05f, 0.05f, 0.05f); // Very dim ambient to allow spotlights to pop

        // Add Movement Script
        MenuCameraMovement movement = bgCamObj.GetComponent<MenuCameraMovement>();
        if (movement == null)
        {
            movement = bgCamObj.AddComponent<MenuCameraMovement>();
        }
        // Adjusted values: Slower but still noticeable (Half of previous speed)
        movement.movementSpeed = 0.25f; 
        movement.movementRadius = 0.2f; 
        movement.enableRotation = true;
        movement.rotationSpeed = 0.2f; 
        movement.maxRotationAngle = 0.5f;

        // 3. Setup UI Camera (Overlay)
        GameObject uiCamObj = GameObject.Find("UI Camera");
        if (uiCamObj == null)
        {
            uiCamObj = new GameObject("UI Camera");
        }

        Camera uiCam = uiCamObj.GetComponent<Camera>();
        if (uiCam == null) uiCam = uiCamObj.AddComponent<Camera>();

        var uiCamData = uiCamObj.GetComponent<UniversalAdditionalCameraData>();
        if (uiCamData == null) uiCamData = uiCamObj.AddComponent<UniversalAdditionalCameraData>();

        uiCamData.renderType = CameraRenderType.Overlay;
        uiCam.cullingMask = 1 << LayerMask.NameToLayer("UI"); // Only UI
        uiCam.clearFlags = CameraClearFlags.Depth; // Ignored for Overlay, but good practice

        // 4. Stack UI Camera
        if (!bgCamData.cameraStack.Contains(uiCam))
        {
            bgCamData.cameraStack.Add(uiCam);
        }

        // 5. Setup Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = uiCam;
            canvas.planeDistance = 1;
            
            // Disable Background Image
            // Try to find an image that looks like a background
            Image[] images = canvas.GetComponentsInChildren<Image>();
            foreach (var img in images)
            {
                if (img.name.ToLower().Contains("background") || img.name.ToLower().Contains("bg"))
                {
                    // Check if it's the full screen one. 
                    // Often the first one is the background.
                    // Let's just log it for the user or disable it if it's obviously a background.
                    if (img.transform.parent == canvas.transform && img.rectTransform.anchorMin == Vector2.zero && img.rectTransform.anchorMax == Vector2.one)
                    {
                        Undo.RecordObject(img.gameObject, "Disable Background Image");
                        img.gameObject.SetActive(false);
                        Debug.Log($"Disabled potential background image: {img.name}");
                    }
                }
            }
        }
        else
        {
            Debug.LogError("No Canvas found!");
        }

        // 6. Setup Fog
        // User requested "Fog" from DaVinciPB. We couldn't find it there, but found Fog_ParcialPREFAB in VFX.
        // We will use this one and ensure it's visible against the black background.
        GameObject fogPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VFX/Fog_ParcialPREFAB.prefab");
        if (fogPrefab == null)
        {
             // Try alternate path
             fogPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/GroundFog.prefab");
        }

        if (fogPrefab != null)
        {
            GameObject fogInstance = (GameObject)PrefabUtility.InstantiatePrefab(fogPrefab);
            fogInstance.name = "Volumetric Fog VFX";
            
            // Position near the building (Building is at Z=10, Y=2)
            // We place fog at the base of the building
            fogInstance.transform.position = new Vector3(0, -2f, 9f); 
            
            // Scale to match building width roughly (Building width is 10)
            // Keep it very low (Y=0.2) and limit depth
            fogInstance.transform.localScale = new Vector3(0.5f, 0.2f, 0.5f); 
            
            // Standard Fog Settings for backup/blending
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.0008f; // Much more subtle global fog
            // Fog color should be very subtle and atmospheric
            RenderSettings.fogColor = new Color(0.15f, 0.15f, 0.18f); 
        }
        else
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.0008f; 
            RenderSettings.fogColor = new Color(0.15f, 0.15f, 0.18f);
            Debug.LogWarning("Fog Prefab not found. Using Standard Fog.");
        }

        // 7. Cleanup Original Main Camera
        GameObject originalCam = GameObject.Find("Main Camera");
        if (originalCam != null && originalCam != bgCamObj && originalCam != uiCamObj)
        {
            originalCam.SetActive(false);
        }

        // 8. Create Demo Content
        CreateDemoContent();
        
        // 9. Setup Street Light Flicker (Horror)
        SetupStreetLightFlicker();

        // 10. Setup Transparent UI
        SetupTransparentUI();

        // 11. Setup Instinto Decal
        SetupInstintoDecal();

        // 12. Setup Post Processing (Film Grain & Chromatic Aberration)
        SetupPostProcessingInternal();

        // 13. Setup Audio
        SetupAudio();

        Debug.Log("Main Menu 3D Setup Complete!");
    }

    private static void SetupAudio()
    {
        // 1. Ensure AudioListener (already done in Camera setup, but double check)
        GameObject bgCamObj = GameObject.Find("Background Camera");
        if (bgCamObj != null)
        {
            if (bgCamObj.GetComponent<AudioListener>() == null) bgCamObj.AddComponent<AudioListener>();
        }

        // 2. Ensure SoundManager exists in scene for direct testing
        GameObject sm = GameObject.Find("SoundManager");
        if (sm == null)
        {
             // Try to find the prefab
             string[] guids = AssetDatabase.FindAssets("SoundManager t:Prefab");
             if (guids.Length > 0)
             {
                 // Filter for the one in Assets/Scenes/Start/ if possible, or just take the first one
                 string bestPath = "";
                 foreach (string g in guids)
                 {
                     string p = AssetDatabase.GUIDToAssetPath(g);
                     if (p.Contains("Start")) 
                     {
                         bestPath = p;
                         break;
                     }
                 }
                 if (string.IsNullOrEmpty(bestPath)) bestPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                 
                 GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(bestPath);
                 if (prefab != null)
                 {
                     GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                     instance.name = "SoundManager";
                     Debug.Log("Instantiated SoundManager prefab.");
                 }
             }
             else
             {
                 Debug.LogWarning("SoundManager prefab not found.");
             }
        }

        // 3. Ensure Menu Music Object exists
        GameObject musicObj = GameObject.Find("Menu Music");
        if (musicObj == null)
        {
            musicObj = new GameObject("Menu Music");
            
            // Try to add BackgroundMusicPlayer if it exists
            System.Type musicType = System.Type.GetType("BackgroundMusicPlayer, Assembly-CSharp");
            if (musicType != null)
            {
                Component musicPlayer = musicObj.AddComponent(musicType);
                
                // Try to find the music clip
                string musicPath = "Assets/Scenes/Start/SoundsLoadingMenu/music1.mp3";
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(musicPath);
                
                if (clip != null)
                {
                    SerializedObject so = new SerializedObject(musicPlayer);
                    so.FindProperty("musicClip").objectReferenceValue = clip;
                    so.FindProperty("playOnStart").boolValue = true;
                    so.FindProperty("loop").boolValue = true;
                    so.FindProperty("fadeDuration").floatValue = 2f;
                    so.ApplyModifiedProperties();
                    Debug.Log("Configured Menu Music.");
                }
            }
            else
            {
                // Fallback: Simple AudioSource
                AudioSource source = musicObj.AddComponent<AudioSource>();
                string musicPath = "Assets/Scenes/Start/SoundsLoadingMenu/music1.mp3";
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(musicPath);
                if (clip != null)
                {
                    source.clip = clip;
                    source.loop = true;
                    source.playOnAwake = true;
                    source.volume = 0.5f;
                    if (!source.isPlaying) source.Play();
                }
            }
        }
    }

    private static void FixBrokenUIEvents()
    {
        // Clear persistent listeners on all Buttons and PressKeyEvent to avoid broken PPtr targets
        var buttons = GameObject.FindObjectsOfType<UnityEngine.UI.Button>(true);
        foreach (var btn in buttons)
        {
            var click = btn.onClick;
            int count = click.GetPersistentEventCount();
            for (int i = count - 1; i >= 0; i--)
            {
                UnityEditor.Events.UnityEventTools.RemovePersistentListener(click, i);
            }
            btn.onClick.RemoveAllListeners();
            EditorUtility.SetDirty(btn);
        }

        var pressKeyEvents = GameObject.FindObjectsOfType<Michsky.UI.Dark.PressKeyEvent>(true);
        foreach (var pke in pressKeyEvents)
        {
            var ev = pke.pressAction;
            int count = ev.GetPersistentEventCount();
            for (int i = count - 1; i >= 0; i--)
            {
                UnityEditor.Events.UnityEventTools.RemovePersistentListener(ev, i);
            }
            EditorUtility.SetDirty(pke);
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("Cleared persistent UI event listeners to fix broken PPtr references. Runtime wiring will handle clicks (MinimalMenuController).");
    }

    private static void RemoveMissingScriptsInScene()
    {
        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        int totalRemoved = 0;
        foreach (var root in roots)
        {
            totalRemoved += UnityEditor.GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);
        }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log($"Removed {totalRemoved} missing scripts from scene.");
    }

    private static void ImproveGraphics()
    {
        SetupPostProcessingInternal();
        RenderSettings.fogDensity = 0.0015f;
    }

    private static void EnsureAmbientOcclusion(VolumeProfile profile)
    {
        if (profile == null) return;
        var aoType = Type.GetType("UnityEngine.Rendering.Universal.AmbientOcclusion, Unity.RenderPipelines.Universal.Runtime");
        if (aoType == null) return;

        VolumeComponent existing = null;
        foreach (var c in profile.components)
        {
            if (c != null && c.GetType() == aoType)
            {
                existing = c;
                break;
            }
        }

        if (existing == null)
        {
            var aoComp = ScriptableObject.CreateInstance(aoType) as VolumeComponent;
            if (aoComp != null)
            {
                aoComp.active = true;
                profile.components.Add(aoComp);
                EditorUtility.SetDirty(profile);
                existing = aoComp;
            }
        }

        if (existing != null)
        {
            var so = new SerializedObject(existing);
            var intensity = so.FindProperty("intensity.m_Value");
            if (intensity != null) intensity.floatValue = 0.35f;
            var direct = so.FindProperty("directLightingStrength.m_Value");
            if (direct != null) direct.floatValue = 0.1f;
            so.ApplyModifiedProperties();
        }
    }

    private static void SetupPostProcessingInternal()
    {
        GameObject volumeObj = GameObject.Find("Global Volume");
        if (volumeObj == null)
        {
            volumeObj = new GameObject("Global Volume");
        }
        
        Volume volume = volumeObj.GetComponent<Volume>();
        if (volume == null) volume = volumeObj.AddComponent<Volume>();
        
        volume.isGlobal = true;
        
        // Ensure profile exists
        if (volume.profile == null)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Settings"))
            {
                AssetDatabase.CreateFolder("Assets", "Settings");
            }
            
            string profilePath = "Assets/Settings/MainMenuProfile.asset";
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, profilePath);
                AssetDatabase.SaveAssets();
            }
            volume.profile = profile;
        }
        
        // 1. Tonemapping (ACES) - Essential for cinematic look
        if (!volume.profile.TryGet(out Tonemapping tonemapping))
        {
            tonemapping = volume.profile.Add<Tonemapping>(true);
        }
        tonemapping.active = true;
        tonemapping.mode.Override(TonemappingMode.ACES);

        // 2. Bloom - Glow for lights
        if (!volume.profile.TryGet(out Bloom bloom))
        {
            bloom = volume.profile.Add<Bloom>(true);
        }
        bloom.active = true;
        bloom.threshold.Override(0.9f);
        bloom.intensity.Override(1.0f); // Reduced from 1.5f
        bloom.scatter.Override(0.4f); // Reduced from 0.5f

        // 3. Vignette - Horror atmosphere
        if (!volume.profile.TryGet(out Vignette vignette))
        {
            vignette = volume.profile.Add<Vignette>(true);
        }
        vignette.active = true;
        vignette.intensity.Override(0.35f); // Slightly reduced
        vignette.smoothness.Override(0.4f);

        // 4. Color Adjustments - Contrast & Saturation
        if (!volume.profile.TryGet(out ColorAdjustments colorAdj))
        {
            colorAdj = volume.profile.Add<ColorAdjustments>(true);
        }
        colorAdj.active = true;
        colorAdj.postExposure.Override(0.1f); 
        colorAdj.contrast.Override(12f); // Slightly reduced
        colorAdj.saturation.Override(8f); // More vibrant

        // 5. Film Grain (Very Subtle)
        if (!volume.profile.TryGet(out FilmGrain filmGrain))
        {
            filmGrain = volume.profile.Add<FilmGrain>(true);
        }
        filmGrain.active = true;
        filmGrain.type.Override(FilmGrainLookup.Thin1); 
        filmGrain.intensity.Override(0.1f); // Reduced for cleaner look
        
        // 6. Chromatic Aberration (Minimal)
        if (!volume.profile.TryGet(out ChromaticAberration chromAb))
        {
            chromAb = volume.profile.Add<ChromaticAberration>(true);
        }
        chromAb.active = true;
        chromAb.intensity.Override(0.05f); // Almost invisible, just a hint of realism

        EnsureAmbientOcclusion(volume.profile);
    }

    private static void SetupInstintoDecal()
    {
        // 1. Find or Create Building
        GameObject building = GameObject.Find("Building");
        if (building == null) building = GameObject.Find("Edificio");
        
        if (building == null)
        {
             // Create a dummy wall if no building exists
             building = GameObject.CreatePrimitive(PrimitiveType.Cube);
             building.name = "Building";
             building.transform.position = new Vector3(0, 2, 10); // Behind the light
             building.transform.localScale = new Vector3(10, 10, 1);
             // Make it dark
             var rend = building.GetComponent<Renderer>();
             if (rend != null) rend.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(0.2f, 0.2f, 0.2f) };
        }

        // Paint Davinci Doors Black
        string[] doors = { "DavinciPuerta", "DavinciPuerta 1", "DavinciPuerta 2", "DavinciPuerta 3" };
        Material blackMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        blackMat.color = Color.black;
        // Low smoothness for matte finish
        blackMat.SetFloat("_Smoothness", 0.0f);
        
        // Save material to avoid leaks or create it once
        string blackMatPath = "Assets/Materials/BlackMatte.mat";
        if (!AssetDatabase.IsValidFolder("Assets/Materials")) AssetDatabase.CreateFolder("Assets", "Materials");
        
        Material existingBlack = AssetDatabase.LoadAssetAtPath<Material>(blackMatPath);
        if (existingBlack == null)
        {
            AssetDatabase.CreateAsset(blackMat, blackMatPath);
            existingBlack = blackMat;
        }

        foreach (string doorName in doors)
        {
            GameObject door = GameObject.Find(doorName);
            if (door != null)
            {
                Renderer r = door.GetComponent<Renderer>();
                if (r != null)
                {
                    Undo.RecordObject(r, "Paint Door Black");
                    r.material = existingBlack;
                }
            }
        }

        // 2. Load and OPTIMIZE Texture
        string texPath = "Assets/instinto.jpg";
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        if (tex == null)
        {
            texPath = "Assets/instinto.png";
            tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        }
        
        if (tex != null)
        {
            // Force High Quality Import Settings
            TextureImporter importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
            if (importer != null)
            {
                bool changed = false;
                if (importer.maxTextureSize < 4096) { importer.maxTextureSize = 4096; changed = true; }
                if (importer.textureCompression != TextureImporterCompression.Uncompressed) { importer.textureCompression = TextureImporterCompression.Uncompressed; changed = true; } // Max quality
                if (importer.filterMode != FilterMode.Trilinear) { importer.filterMode = FilterMode.Trilinear; changed = true; }
                if (importer.anisoLevel != 16) { importer.anisoLevel = 16; changed = true; }
                if (importer.mipmapEnabled) { importer.mipmapEnabled = false; changed = true; } // Disable MipMaps for sharpness
                
                if (changed)
                {
                    importer.SaveAndReimport();
                    Debug.Log($"Optimized texture quality for: {texPath}");
                }
            }
        }
        else
        {
            Debug.LogError("Could not find 'Assets/instinto.jpg' or 'instinto.png'.");
            return;
        }

        // 3. Create Poster (Quad)
        GameObject posterObj = GameObject.Find("Instinto Poster");
        if (posterObj == null)
        {
            posterObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            posterObj.name = "Instinto Poster";
        }
        
        // Remove collider if any
        Collider col = posterObj.GetComponent<Collider>();
        if (col != null) DestroyImmediate(col);

        // Position slightly in front of the building (Building Z=10)
        posterObj.transform.position = new Vector3(0, 3, 9.95f); 
        posterObj.transform.rotation = Quaternion.Euler(0, 180, 0); // Face -Z (towards camera)

        // Calculate Aspect Ratio to prevent stretching
        float aspect = (float)tex.width / tex.height;
        float maxSize = 3.5f; // Max size in Unity units
        float width = maxSize;
        float height = maxSize;

        if (aspect >= 1)
        {
            // Landscape or Square
            height = width / aspect;
        }
        else
        {
            // Portrait
            width = height * aspect;
        }

        posterObj.transform.localScale = new Vector3(width, height, 1);

        // Create Material for Poster
        string posterMatPath = "Assets/Materials/InstintoPoster.mat";
        Material posterMat = AssetDatabase.LoadAssetAtPath<Material>(posterMatPath);
        
        if (posterMat == null)
        {
            // Use Unlit for visibility or Lit for lighting interaction
            Shader posterShader = Shader.Find("Universal Render Pipeline/Lit");
            if (posterShader == null) posterShader = Shader.Find("Standard"); // Fallback
            
            posterMat = new Material(posterShader);
            posterMat.SetTexture("_BaseMap", tex);
            posterMat.SetTexture("_MainTex", tex);
            posterMat.color = Color.white;
            
            // Matte paper finish
            posterMat.SetFloat("_Smoothness", 0.1f); 
            posterMat.SetFloat("_Metallic", 0.0f);
            
            AssetDatabase.CreateAsset(posterMat, posterMatPath);
        }
        else
        {
            // Update texture just in case
            posterMat.SetTexture("_BaseMap", tex);
            posterMat.SetTexture("_MainTex", tex);
            posterMat.SetFloat("_Smoothness", 0.1f); // Ensure it's matte
        }
        
        Renderer posterRend = posterObj.GetComponent<Renderer>();
        if (posterRend != null) posterRend.material = posterMat;
        
        // Remove old Decal Projector if it exists to avoid confusion
        GameObject oldDecal = GameObject.Find("Instinto Decal");
        if (oldDecal != null) DestroyImmediate(oldDecal);
    }

    private static void SetupStreetLightFlicker()
    {
        // Find Street Light (1) or Street Light
        GameObject streetLight = GameObject.Find("Street Light (1)");
        if (streetLight == null) streetLight = GameObject.Find("Street Light");
        
        if (streetLight != null)
        {
             // Add EffectFlicker
             System.Type flickerType = System.Type.GetType("VLB.EffectFlicker, Assembly-CSharp");
             if (flickerType != null)
             {
                 Component flicker = streetLight.GetComponent(flickerType);
                 if (flicker == null) flicker = streetLight.AddComponent(flickerType);
                 
                 SerializedObject so = new SerializedObject(flicker);
                 
                 // Horror settings
                  // frequency: Very High for intense strobe effect
                  so.FindProperty("frequency").floatValue = 80f; // Increased from 45f
                  so.FindProperty("performPauses").boolValue = true;
                  so.FindProperty("restoreIntensityOnPause").boolValue = true; // Go dark on pause
                 
                 // Flickering Duration (short bursts)
                 SerializedProperty flickerDuration = so.FindProperty("flickeringDuration");
                 flickerDuration.FindPropertyRelative("m_MinValue").floatValue = 0.1f;
                 flickerDuration.FindPropertyRelative("m_MaxValue").floatValue = 0.4f; // Slightly faster bursts

                 // Pause Duration (shorter intervals for faster pace)
                 SerializedProperty pauseDuration = so.FindProperty("pauseDuration");
                 pauseDuration.FindPropertyRelative("m_MinValue").floatValue = 0.05f;
                 pauseDuration.FindPropertyRelative("m_MaxValue").floatValue = 0.5f; // Reduced from 1.5f for less downtime

                 // Intensity Amplitude (dimming significantly)
                 SerializedProperty intensityAmplitude = so.FindProperty("intensityAmplitude");
                 intensityAmplitude.FindPropertyRelative("m_MinValue").floatValue = -2f; 
                 intensityAmplitude.FindPropertyRelative("m_MaxValue").floatValue = 0f;
                 
                 so.ApplyModifiedProperties();
                 Debug.Log("Configured Street Light Flicker for Horror.");
             }
             else
             {
                 Debug.LogWarning("VLB.EffectFlicker type not found.");
             }
        }
    }

    private static void SetupTransparentUI()
    {
        // Find Main Panels
        GameObject mainPanels = GameObject.Find("Main Panels");
        if (mainPanels != null)
        {
            // Find Home Panel
            Transform home = mainPanels.transform.Find("Home");
            if (home != null)
            {
                // Ensure Home is active
                home.gameObject.SetActive(true);

                // Make Home background transparent if it has an image
                Image homeImg = home.GetComponent<Image>();
                if (homeImg != null)
                {
                    // If the image is just a background color, make it transparent
                    // But if it's the container for buttons, we might want to keep it?
                    // The user said "fondo debe ser transparente".
                    // Let's assume the buttons are children and have their own graphics.
                    Undo.RecordObject(homeImg, "Make Home Transparent");
                    Color c = homeImg.color;
                    c.a = 0f;
                    homeImg.color = c;
                    homeImg.raycastTarget = false; // Allow clicks to pass through
                }
            }

            // Check for background images on Main Panels itself
            Image mainPanelsImg = mainPanels.GetComponent<Image>();
            if (mainPanelsImg != null)
            {
                Undo.RecordObject(mainPanelsImg, "Make Main Panels Transparent");
                Color c = mainPanelsImg.color;
                c.a = 0f;
                mainPanelsImg.color = c;
                mainPanelsImg.raycastTarget = false; // Allow clicks to pass through
            }
        }

        // Look for common background objects in Canvas and disable them
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            foreach (Transform child in canvas.transform)
            {
                // If it's not Main Panels and looks like a background, disable it
                if (child.gameObject != mainPanels && 
                    (child.name.ToLower().Contains("background") || child.name.ToLower().Contains("bg") || child.name.ToLower().Contains("image")))
                {
                     // Check if it covers the screen
                     RectTransform rt = child.GetComponent<RectTransform>();
                     if (rt != null && rt.anchorMin == Vector2.zero && rt.anchorMax == Vector2.one)
                     {
                         Undo.RecordObject(child.gameObject, "Disable Background Object");
                         child.gameObject.SetActive(false);
                     }
                }
            }
        }
    }

    private static void CreateDemoContent()
    {
        GameObject root = GameObject.Find("Background Environment");
        if (root == null) root = new GameObject("Background Environment");
        
        // Check if empty
        if (root.transform.childCount == 0)
        {
            // Create a floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.transform.SetParent(root.transform);
            floor.transform.position = new Vector3(0, -2, 10);
            floor.transform.localScale = new Vector3(10, 1, 10);
            
            // Create Street Light
            GameObject streetLight = new GameObject("Street Light");
            streetLight.transform.SetParent(root.transform);
            streetLight.transform.position = new Vector3(0, 5, 5); // Above and slightly in front
            streetLight.transform.rotation = Quaternion.Euler(90, 0, 0); // Pointing down

            // Add Unity Light
            Light lightComp = streetLight.AddComponent<Light>();
            lightComp.type = LightType.Spot;
            lightComp.range = 20f;
            lightComp.spotAngle = 45f;
            lightComp.intensity = 2f;
            lightComp.color = new Color(1f, 0.95f, 0.8f); // Warm street light

            // Add VLB
            // We use Reflection or check if assembly exists to avoid compilation errors if VLB is missing
            // But since we are in the project, we can assume VLB namespace is available if we add using.
            // However, this is an Editor script. Let's try to add the component if the type exists.
            System.Type vlbType = System.Type.GetType("VLB.VolumetricLightBeamSD, Assembly-CSharp");
            if (vlbType != null)
            {
                var vlb = streetLight.AddComponent(vlbType);
                // We can't access properties easily without casting, but default settings should work.
                // Or we can use SerializedObject to set properties.
                SerializedObject so = new SerializedObject(vlb);
                so.FindProperty("colorMode").enumValueIndex = 0; // Flat
                so.FindProperty("color").colorValue = new Color(1f, 0.95f, 0.8f, 0.5f);
                so.FindProperty("fallOffEnd").floatValue = 10f;
                so.FindProperty("spotAngleFromLight").boolValue = true;
                so.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogWarning("VLB.VolumetricLightBeamSD type not found. VLB might not be installed or assembly name differs.");
            }
        }
    }
}
