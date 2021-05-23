using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PVR.Tour
{
    /// <summary>
    /// A utility class that locates elements in the hierarchy, potentially based on their TourPlatform hierarchy ID.
    /// </summary>
    public class ObjectFinder : MonoBehaviour
    {
        void Awake()
        {
            gameObject.name = "[ObjectFinder]";
        }    
        
        public T Find<T>(bool requireActive = false) where T : Component
        {
            var objects = requireActive ? FindObjectsOfType(typeof(T)) : Resources.FindObjectsOfTypeAll(typeof(T));

            return objects.Cast<T>().FirstOrDefault(obj => !(obj is null));
        }
        
        public T FindChild<T>(GameObject parent) where T : Component
        {
            var target = parent.GetComponentInChildren<T>();

            return target;
        }


        public List<T> FindInterfaces<T>(  )
        {
            List<T> interfaces = new List<T>();
            GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach( var rootGameObject in rootGameObjects )
            {
                T[] childrenInterfaces = rootGameObject.GetComponentsInChildren<T>();
                foreach( var childInterface in childrenInterfaces )
                {
                    interfaces.Add(childInterface);
                }
            }
            return interfaces;
        }

        public T FindObjectByID<T>(string id) where T : Component
        {
            var objects = FindObjectsOfTypeAll(typeof(T));
            
            foreach (T foundObject in objects)
            {
                var hID = foundObject.gameObject.name.Split(' ')[0];
                if (hID == id)
                {
                    return foundObject.gameObject.GetComponent<T>();
                }
            }

            return null;
        }
        
    }
}
