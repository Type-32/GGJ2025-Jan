using System;
using System.Collections.Generic;
using ProtoLib.Library.Mono.Utility;
using UnityEngine;
using UnityEngine.Audio;

namespace Bubble.Audio
{
    [Serializable]
    public class NamedAudioClip
    {
        public string name;
        public AudioClip clip;
    }
    public class AudioManager : MonoBehaviour
    {
        // Singleton instance for easy access
        public static AudioManager Instance;

        // Dictionary to store audio clips by name
        public List<NamedAudioClip> audioClips = new();

        // Reference to the Audio Mixer
        public AudioMixer audioMixer;

        // Mixer group for SFX (assign in the Inspector)
        public AudioMixerGroup sfxMixerGroup;

        // Mixer group for Music (assign in the Inspector)
        public AudioMixerGroup musicMixerGroup;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private bool TryGetClip(string name, out AudioClip clip)
        {
            clip = audioClips.Find(namedClip => namedClip.name == name)?.clip;
            return clip != null;
        }

        // Overload 1: Play audio by name with default settings (SFX mixer group)
        public void PlayAudio(string name, float volume = 1f, float pitch = 1f)
        {
            PlayAudio(name, volume, pitch, 0f, sfxMixerGroup); // Default panning (center), SFX mixer group
        }

        // Overload 2: Play audio by name with volume, pitch, stereo panning, and mixer group
        public void PlayAudio(string name, float volume, float pitch, float pan, AudioMixerGroup mixerGroup = null)
        {
            if (TryGetClip(name, out AudioClip clip))
            {
                // Create a temporary GameObject to hold the AudioSource
                GameObject tempAudioObject = new GameObject("TempAudio");
                AudioSource audioSource = tempAudioObject.AddComponent<AudioSource>();

                // Configure the AudioSource
                audioSource.clip = clip;
                audioSource.volume = volume;
                audioSource.pitch = pitch;
                audioSource.panStereo = pan; // Stereo panning (-1 = left, 1 = right)

                // Assign the AudioSource to the specified mixer group
                if (mixerGroup != null)
                {
                    audioSource.outputAudioMixerGroup = mixerGroup;
                }

                // Play the clip
                audioSource.Play();

                // Destroy the object after the clip finishes playing
                Destroy(tempAudioObject, clip.length);
            }
            else
            {
                Debug.LogError($"Audio clip with name '{name}' not found in the dictionary.");
            }
        }

        // Overload 3: Play audio by name with a delay and mixer group
        public void PlayAudio(string name, float volume, float pitch, float pan, float delay,
            AudioMixerGroup mixerGroup = null)
        {
            if (TryGetClip(name, out AudioClip clip))
            {
                // Create a temporary GameObject to hold the AudioSource
                GameObject tempAudioObject = new GameObject("TempAudio");
                AudioSource audioSource = tempAudioObject.AddComponent<AudioSource>();

                // Configure the AudioSource
                audioSource.clip = clip;
                audioSource.volume = volume;
                audioSource.pitch = pitch;
                audioSource.panStereo = pan;

                // Assign the AudioSource to the specified mixer group
                if (mixerGroup != null)
                {
                    audioSource.outputAudioMixerGroup = mixerGroup;
                }

                // Play the clip with a delay
                audioSource.PlayDelayed(delay);

                // Destroy the object after the clip finishes playing
                Destroy(tempAudioObject, clip.length + delay);
            }
            else
            {
                Debug.LogError($"Audio clip with name '{name}' not found in the dictionary.");
            }
        }
    }
}