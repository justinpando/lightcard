using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

namespace PVR.Utilities
{

    public class FirstPersonCamera : MonoBehaviour
    {
        Vector3 Angles;
        public float sensitivityX;
        public float sensitivityY;

        public bool drag = false;

        void Awake() {
            
        }
        void Update()
        {
            float rotationY = Input.GetAxis("Mouse Y") * sensitivityX;
            float rotationX = Input.GetAxis("Mouse X") * sensitivityY;

            if (drag)
            {
                if (!Input.GetMouseButton(0)) return;
                rotationY = -rotationY;
                rotationX = -rotationX;
            }
            if (rotationY > 0)
                Angles = new Vector3(Mathf.MoveTowards(Angles.x, -80, rotationY), Angles.y + rotationX, 0);
            else
                Angles = new Vector3(Mathf.MoveTowards(Angles.x, 80, -rotationY), Angles.y + rotationX, 0);
            transform.localEulerAngles = Angles;
        }
    }
}
