using UnityEngine;
using System.Collections;

public class DestroyOnAudioPlayedBehavior : MonoBehaviour
{

    public AudioSource audioSource;

    float initialCheckDelay = 1.0f;

    float startTime;

    // Use this for initialization
    void Start()
    {
        startTime = Time.time;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (startTime + initialCheckDelay < Time.time)
        {
            DestroyIfNotPlayingAudio();
        }
    }

    void DestroyIfNotPlayingAudio()
    {
        if (audioSource == null || !audioSource.isPlaying) Destroy(this.gameObject);
    }
}