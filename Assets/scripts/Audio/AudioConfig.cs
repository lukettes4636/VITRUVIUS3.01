using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AudioConfig", menuName = "Audio/Audio Configuration", order = 1)]
public class AudioConfig : ScriptableObject
{
    [Header("AudioMixer Configuration")]
    public AudioMixerGroup masterMixerGroup;
    public AudioMixerGroup musicMixerGroup;
    public AudioMixerGroup sfxMixerGroup;
    public AudioMixerGroup ambientMixerGroup;
    public AudioMixerGroup voiceMixerGroup;

    [Header("Footsteps")]
    public AudioClip[] player1Footsteps;
    public AudioClip[] player2Footsteps;

    [Header("Common Sound Effects")]
    public AudioClip[] pickUpSounds;
    public AudioClip[] doorOpenSounds;
    public AudioClip[] doorCloseSounds;
    public AudioClip[] doorLockedSounds;
    public AudioClip[] doorUnlockSounds;
    public AudioClip[] interactionSounds;
    public AudioClip[] puzzleSounds;
    public AudioClip[] flashlightSounds;

    [Header("Music")]
    public AudioClip[] explorationMusic;

    [Header("Volume Configuration (Default Values)")]
    [Range(0f, 1f)]
    public float defaultMasterVolume = 1f;
    [Range(0f, 1f)]
    public float defaultMusicVolume = 1f;
    [Range(0f, 1f)]
    public float defaultSfxVolume = 1f;
}