using UnityEngine;





public class NPCAnimationBridge : MonoBehaviour
{
    private NPCBehaviorManager behaviorManager;
    private NPCHealth npcHealth;

    void Awake()
    {
        
        behaviorManager = GetComponentInParent<NPCBehaviorManager>();
        npcHealth = GetComponentInParent<NPCHealth>();
    }

    
    public void PlayFootstepSound()
    {
        if (behaviorManager != null)
        {
            behaviorManager.PlayFootstepSound();
        }
    }
}