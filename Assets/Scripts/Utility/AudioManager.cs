using System;
using UnityEngine;

namespace PVR.Utilities
{
    /// <summary>
    /// Handles playing audio clips and controls app volume.
    /// </summary>
    public class AudioManager
    {
        public AudioPlayer audioPlayerPrefab;

        public float Volume { get; private set; }

        public event Action<float> OnVolumeChanged;

        private bool debugging = false;
        
        public AudioManager(AudioPlayer audioPlayerPrefab, float volume)
        {
            this.audioPlayerPrefab = audioPlayerPrefab;
            Volume = volume;
        }

        public AudioPlayer PlayAudio(AudioClip audioClip)
        {
            return PlayAudio(audioClip, Vector3.zero, Volume);
        }
        
        public AudioPlayer PlayAudio(AudioClip audioClip, Vector3 location, float volume, float pitch = 1f, bool loop = false)
        {
            if (audioClip == null) return null;

            AudioPlayer audioPlayer = GameObject.Instantiate(audioPlayerPrefab).GetComponent<AudioPlayer>();          
            audioPlayer.Initialize(this);
            
            audioPlayer.PlayAudio(audioClip, location, volume, pitch, loop);

            return audioPlayer;
        }

        public void SetVolume(float value)
        {
            Volume = value;
            OnVolumeChanged?.Invoke(value);
        }
    }
}