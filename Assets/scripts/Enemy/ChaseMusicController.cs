using UnityEngine;

public class ChaseMusicController : MonoBehaviour
{
    [Header("Chase Music Configuration")]
    [Tooltip("AudioSource that will play the chase music")]
    public AudioSource musicAudioSource;
    
    [Tooltip("The chase music clip to play")]
    public AudioClip chaseMusicClip;
    
    [Tooltip("Should the music loop?")]
    public bool loopMusic = true;
    
    [Tooltip("Volume of the chase music (0-1)")]
    [Range(0f, 1f)]
    public float musicVolume = 1f;
    
    [Header("Debug Info")]
    [SerializeField] private bool hasPlayedOnce = false;
    [SerializeField] private bool isCurrentlyPlaying = false;
    
    void Start()
    {
        
        if (musicAudioSource == null)
        {
            musicAudioSource = GetComponent<AudioSource>();
            if (musicAudioSource == null)
            {

            }
        }
        
        ConfigureAudioSource();
    }
    
    void ConfigureAudioSource()
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.loop = loopMusic;
            musicAudioSource.volume = musicVolume;
            musicAudioSource.spatialBlend = 0f; 
            musicAudioSource.playOnAwake = false;
        }
    }
    
    void Update()
    {
        if (musicAudioSource != null)
        {
            isCurrentlyPlaying = musicAudioSource.isPlaying;
        }
    }
    
    
    
    
    public void PlayChaseMusic()
    {
        if (hasPlayedOnce)
        {

            return;
        }
        
        if (musicAudioSource == null)
        {

            return;
        }
        
        if (chaseMusicClip == null)
        {

            return;
        }
        
        if (musicAudioSource.isPlaying)
        {

            return;
        }
        
        musicAudioSource.clip = chaseMusicClip;
        musicAudioSource.Play();
        hasPlayedOnce = true;
        

    }
    
    
    
    
    public void ResetChaseMusic()
    {
        hasPlayedOnce = false;
        if (musicAudioSource != null && musicAudioSource.isPlaying)
        {
            musicAudioSource.Stop();
        }

    }
    
    
    
    
    public void StopChaseMusic()
    {
        if (musicAudioSource != null && musicAudioSource.isPlaying)
        {
            musicAudioSource.Stop();

        }
    }
    
    
    
    
    public bool HasPlayedOnce() => hasPlayedOnce;
    
    
    
    
    public bool IsPlaying() => isCurrentlyPlaying;
    
    [ContextMenu("Test Play Chase Music")]
    public void TestPlayChaseMusic()
    {
        PlayChaseMusic();
    }
    
    [ContextMenu("Test Reset Chase Music")]
    public void TestResetChaseMusic()
    {
        ResetChaseMusic();
    }
    
    [ContextMenu("Test Stop Chase Music")]
    public void TestStopChaseMusic()
    {
        StopChaseMusic();
    }
    
    void OnValidate()
    {
        
        if (Application.isPlaying && musicAudioSource != null)
        {
            musicAudioSource.loop = loopMusic;
            musicAudioSource.volume = musicVolume;
            musicAudioSource.spatialBlend = 0f;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        #if UNITY_EDITOR
        if (Application.isEditor)
        {
            string status = hasPlayedOnce ? "Played" : "Ready";
            string playing = isCurrentlyPlaying ? "▶ Playing" : "⏸ Stopped";
            UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, $"Chase Music: {status}\n{playing}");
        }
        #endif
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(ChaseMusicController))]
public class ChaseMusicControllerEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        ChaseMusicController controller = (ChaseMusicController)target;
        
        GUILayout.Space(10);
        GUILayout.Label("Testing Tools", UnityEditor.EditorStyles.boldLabel);
        
        if (GUILayout.Button("Test Play Chase Music"))
        {
            controller.TestPlayChaseMusic();
        }
        
        if (GUILayout.Button("Test Stop Chase Music"))
        {
            controller.TestStopChaseMusic();
        }
        
        if (GUILayout.Button("Test Reset Chase Music"))
        {
            controller.TestResetChaseMusic();
        }
        
        GUILayout.Space(10);
        GUILayout.Label($"Status: {(controller.HasPlayedOnce() ? "Played Once" : "Ready to Play")}", UnityEditor.EditorStyles.miniLabel);
        GUILayout.Label($"Currently: {(controller.IsPlaying() ? "▶ Playing" : "⏸ Stopped")}", UnityEditor.EditorStyles.miniLabel);
    }
}
#endif
