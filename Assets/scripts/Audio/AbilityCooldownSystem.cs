using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AbilityCooldownSystem : MonoBehaviour
{
    [System.Serializable]
    public class AbilitySound
    {
        public string abilityName;
        public AudioClip cooldownStartSound;
        public AudioClip cooldownEndSound;
        public AudioClip abilityReadySound;
    }

    [Header("Cooldown Configuration")]
    [SerializeField] private List<AbilitySound> abilitySounds = new List<AbilitySound>();
    [SerializeField] private AudioSource cooldownAudioSource;

    private Dictionary<string, AbilitySound> abilitySoundDict = new Dictionary<string, AbilitySound>();
    private Dictionary<string, float> cooldownTimers = new Dictionary<string, float>();

    void Awake()
    {
        if (cooldownAudioSource == null)
            cooldownAudioSource = gameObject.AddComponent<AudioSource>();

        
        foreach (var abilitySound in abilitySounds)
        {
            abilitySoundDict[abilitySound.abilityName] = abilitySound;
        }
    }

    public void StartCooldown(string abilityName, float cooldownDuration)
    {
        if (abilitySoundDict.ContainsKey(abilityName))
        {
            var sound = abilitySoundDict[abilityName];
            
            
            if (sound.cooldownStartSound != null)
                cooldownAudioSource.PlayOneShot(sound.cooldownStartSound);

            
            StartCoroutine(CooldownCompleteSound(abilityName, cooldownDuration));
        }

        cooldownTimers[abilityName] = Time.time + cooldownDuration;
    }

    private IEnumerator CooldownCompleteSound(string abilityName, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (abilitySoundDict.ContainsKey(abilityName))
        {
            var sound = abilitySoundDict[abilityName];
            if (sound.cooldownEndSound != null)
                cooldownAudioSource.PlayOneShot(sound.cooldownEndSound);
        }
    }

    public void PlayAbilityReadySound(string abilityName)
    {
        if (abilitySoundDict.ContainsKey(abilityName))
        {
            var sound = abilitySoundDict[abilityName];
            if (sound.abilityReadySound != null)
                cooldownAudioSource.PlayOneShot(sound.abilityReadySound);
        }
    }

    public bool IsAbilityReady(string abilityName)
    {
        return !cooldownTimers.ContainsKey(abilityName) || Time.time >= cooldownTimers[abilityName];
    }
}