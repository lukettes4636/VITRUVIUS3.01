using UnityEngine;
using System.Collections.Generic;






public class ElectricDamage : MonoBehaviour
{
    [Header("Damage Configuration")]
    [Tooltip("Cantidad de dano aplicado por 'tick' (golpe).")]
    public float damageAmount = 20f;

    [Tooltip("Tasa de aplicacion de dano (tiempo en segundos entre cada tick de dano).")]
    public float damageRate = 2f;

    [Header("Audio")]
    [Tooltip("Sonido de trampa electrica (se reproduce constantemente)")]
    public AudioClip electricTrapSound;
    [Tooltip("Sonido de dano electrico al jugador")]
    public AudioClip electricDamageSound;
    [Tooltip("Volumen del sonido de trampa")]
    public float trapSoundVolume = 0.5f;

    
    private Dictionary<PlayerHealth, float> playerNextDamageTime = new Dictionary<PlayerHealth, float>();
    private AudioSource trapAudioSource;

    void Start()
    {
        
        if (electricTrapSound != null)
        {
            trapAudioSource = gameObject.AddComponent<AudioSource>();
            trapAudioSource.clip = electricTrapSound;
            trapAudioSource.loop = true;
            trapAudioSource.volume = trapSoundVolume;
            trapAudioSource.playOnAwake = true;
            trapAudioSource.Play();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            
            playerNextDamageTime[playerHealth] = Time.time;
            
            if (electricDamageSound != null)
            {
                
                AudioManager.Instance.PlaySFX(electricDamageSound, transform.position, 1f, 0.8f);
            }
        }
    }

    
    private void OnTriggerExit(Collider other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null && playerNextDamageTime.ContainsKey(playerHealth))
        {
            
            playerNextDamageTime.Remove(playerHealth);

        }
    }

    private void OnTriggerStay(Collider other)
    {
        
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        
        if (playerHealth != null)
        {
            
            float currentTime = Time.time;
            
            
            if (!playerNextDamageTime.ContainsKey(playerHealth))
            {
                playerNextDamageTime[playerHealth] = 0f;
            }
            
            
            if (currentTime >= playerNextDamageTime[playerHealth])
            {
                playerHealth.SetLastDamageSource("ElectricDamage");
                playerHealth.TakeDamage((int)damageAmount);

                
                playerNextDamageTime[playerHealth] = currentTime + damageRate;
            }
        }
    }

    
    
    
    public void SimulatePlayerEnter(PlayerHealth playerHealth)
    {
        if (playerHealth == null) return;
        

        
        
        playerNextDamageTime[playerHealth] = Time.time;
        
        
        if (electricDamageSound != null)
        {
            AudioManager.Instance.PlaySFX(electricDamageSound, transform.position, 1f, 0.8f);
        }
        
        playerHealth.SetLastDamageSource("ElectricDamage");
        playerHealth.TakeDamage((int)damageAmount);
        playerNextDamageTime[playerHealth] = Time.time + damageRate;
    }

    
    
    
    public void SimulateContinuousDamage(PlayerHealth playerHealth)
    {
        if (playerHealth == null) return;
        
        
        float currentTime = Time.time;
        
        
        if (!playerNextDamageTime.ContainsKey(playerHealth))
        {
            playerNextDamageTime[playerHealth] = 0f;
        }
        
        
        if (currentTime < playerNextDamageTime[playerHealth]) return;
        

        
        playerHealth.SetLastDamageSource("ElectricDamage");
        playerHealth.TakeDamage((int)damageAmount);
        
        
        playerNextDamageTime[playerHealth] = currentTime + damageRate;
    }
}
