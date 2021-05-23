using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PVR.Utilities
{
    public class AutoRotate : MonoBehaviour
    {

        public Vector3 rotation = new Vector3();
        
        // Update is called once per frame
        void Update()
        {
            transform.Rotate(rotation * Time.deltaTime);
        }
    }
}