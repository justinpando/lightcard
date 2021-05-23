using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PVR.Utilities
{
    public class PositionOverTransformBehavior : MonoBehaviour
    {

        public Transform target;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

            Vector3 screenPosition = Camera.main.WorldToScreenPoint(target.position);

            transform.position = screenPosition;
        }
    }
}
