using UnityEngine;
using System.Collections;
using UnityEditor;

namespace PVR.Utilities
{
    [CreateAssetMenu(menuName = "My Assets/AudioData")]
    public class AudioCollection : ScriptableObject
    {
        public float volume = 1f;

        public AudioClip showMenu;
        public AudioClip hideMenu;
        public AudioClip removeItem;
        public AudioClip addItem;
        
    }

}