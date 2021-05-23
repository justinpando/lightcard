using UnityEngine;

namespace PVR.Utilities
{

    [RequireComponent(typeof(AudioSource))]
    public class AudioPlayer : MonoBehaviour
    {

        public AudioSource audioSource;
        private AudioManager audioManager;

        private void Start()
        {
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
        }

        public void Initialize(AudioManager audioManager)
        {
            this.audioManager = audioManager;
            audioManager.OnVolumeChanged += HandleVolumeChanged;
        }
        
        public void PlayAudio(AudioClip audioClip, Vector3 location, float volume, float pitch = 1f, bool loop = false)
        {
            audioSource.transform.position = location;

            audioSource.pitch = pitch;
            audioSource.volume = volume;
            audioSource.clip = audioClip;
            audioSource.loop = loop;
            audioSource.Play();
        }

        void HandleVolumeChanged(float value)
        {
            audioSource.volume = value;
        }
        
    }
}
