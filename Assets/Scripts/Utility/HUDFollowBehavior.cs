using UnityEngine;

namespace PVR.Utilities
{
    public class HUDFollowBehavior : MonoBehaviour
    {
        public Transform cameraTransform;
        public float distance = 5f;
        public Vector3 offset;
        public float followDelay = 0.25f;
        public float tolerance = 0.25f; // How far the camera can move before this follows 
        public bool useMainCamera = false;

        private Vector3 lastTargetPos;
        private Vector3 currentTargetPos;

        public bool keepWorldUp = false;

        public bool yAxisOnly = true;
        
        private void Start()
        {
            if (useMainCamera)
            {
                if (Camera.main != null) Initialize(Camera.main.transform);
            }
            
            if (cameraTransform) transform.position = GetTargetPosition();
        }

        private void OnEnable()
        {
            //Initialize to correct position at start
            if (cameraTransform) transform.position = GetTargetPosition();
        }
        
        public void Initialize(Transform cameraTransform)
        {
            this.cameraTransform = cameraTransform;
            transform.position = GetTargetPosition();
        }

        void Update()
        {
            if (cameraTransform == null) return;

            HandlePosition();
            HandleOrientation();
        }

        private void HandlePosition()
        {
            float i = Interpolate.EaseOutQuad(0f, 1f, Time.deltaTime, followDelay);

            Vector3 target = GetTargetPosition();
            
            Vector3 vToTarget = target - transform.position;

            Vector3 pos = transform.position + vToTarget * i;

            Vector3 vFromCamToPos = pos - cameraTransform.position;

            transform.position = cameraTransform.position + vFromCamToPos.normalized * distance;
        }

        private void HandleOrientation()
        {
            Vector3 vFromCamera = transform.position - cameraTransform.position;
            Vector3 forward = vFromCamera.normalized;
            if (keepWorldUp) forward.y = 0f;
            transform.forward = forward;
        }
        
        Vector3 GetTargetPosition()
        {
            Vector3 targetPos = cameraTransform.position + offset;

            Vector3 fwd = cameraTransform.forward;
            if (yAxisOnly)
            {
                fwd.y = 0f;
                fwd.Normalize();
            }
            
            targetPos += new Vector3(cameraTransform.forward.x, 0f, cameraTransform.forward.z) * distance;

            if (Vector3.Distance(targetPos, currentTargetPos) > tolerance)
            {
                currentTargetPos = targetPos;
            }
            
            return currentTargetPos;
            
        }
    }
}
