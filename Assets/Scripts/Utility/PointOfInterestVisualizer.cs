using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PVR.Utilities
{
    public class PointOfInterestVisualizer : MonoBehaviour
    {

        public Transform origin;
        public Transform target;

        public LineDrawer lineDrawer;
        public PositionOverTransformBehavior imagePositioner;

        // Use this for initialization
        void Start()
        {
            imagePositioner.target = target;

            lineDrawer.originTransform = origin;
            lineDrawer.targetTransform = target;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
