using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDragger : MonoBehaviour
{
    [Space(16f)] 
    public bool useInertia;
    public float inertia = 2;
    public float inertialDeadZone = 0.5f;
    public float snappiness = 30f;
    private Vector2 runningInertialSpeed;
    private Vector2 inertiaSpeed;
    private Vector2 lastFrameInput;
    private float degreesPerPixel;
    
    private Camera cam;
    private GameObject rootCamera;
    private GameObject yawCamera;
    public float swipeScalar = 1;
    public void Start() {
        cam = GetComponent<Camera>();
        degreesPerPixel = cam.fieldOfView/((float) Screen.height);
        BuildOrUpdateMonoRoot();
    }
    public void LateUpdate()
    {
        HandleMonoInputs();
    }    
    private void BuildOrUpdateMonoRoot() {
        if(rootCamera==null)
            rootCamera = new GameObject("CameraRoot");
        rootCamera.transform.position = cam.transform.position;
        rootCamera.transform.rotation = Quaternion.identity; // monoCamera.transform.rotation;
        rootCamera.transform.parent = cam.transform.parent;
        //cam.transform.parent = rootCamera.transform;
        
        
        if(yawCamera==null)
            yawCamera = new GameObject("CameraRootY");
        yawCamera.transform.position = cam.transform.position;
        yawCamera.transform.rotation = Quaternion.identity; // monoCamera.transform.rotation;
        yawCamera.transform.parent = rootCamera.transform;
    }

    private bool isUpdating;
    void HandleMonoInputs() {
        if (Input.GetMouseButtonDown(0))
            lastFrameInput = Input.mousePosition;
        if (Input.GetMouseButtonUp(0)) {
            inertiaSpeed = runningInertialSpeed;
            if (inertiaSpeed.magnitude > 20)
                inertiaSpeed = inertiaSpeed.normalized * 20;
            runningInertialSpeed=Vector2.zero;
        }
        if (Input.GetMouseButton(0)) {
            Vector2 delta = ((Vector2)Input.mousePosition) - lastFrameInput;
            lastFrameInput = Input.mousePosition;
            Vector2 scaledDeltaX = delta * -degreesPerPixel;
            //float dotDownX = Vector3.Dot(Vector3.down, -cam.transform.up);
            //float dotRightX = Vector3.Dot(Vector3.down, cam.transform.right);
            //scaledDeltaX.x *= dotDownX;
            //scaledDeltaX.y *= dotRightX;
            
            Vector2 scaledDeltaY = delta * -degreesPerPixel;
            //float dotDownY = Vector3.Dot(Vector3.down, cam.transform.up);
            //float dotRightY = Vector3.Dot(Vector3.down, cam.transform.right);
            //scaledDeltaY.y *= dotDownY;
            //scaledDeltaY.x *= dotRightY;
            
            rootCamera.transform.Rotate(0,(scaledDeltaX.x)*swipeScalar,0);
            yawCamera.transform.Rotate((-scaledDeltaY.y)*swipeScalar,0,0,Space.Self);
            
            CalculateInertiaTally(new Vector2(
                scaledDeltaX.x,
                -scaledDeltaY.y)*swipeScalar);
            isUpdating = true;
        } else if(useInertia) {
            ApplyInertia();
            isUpdating = false;
        }

        cam.transform.rotation = Quaternion.Lerp(
            Quaternion.LookRotation(cam.transform.forward, Vector3.up), 
            Quaternion.LookRotation(yawCamera.transform.forward, Vector3.up),
            snappiness * Time.deltaTime);
        ClampPitch(yawCamera.gameObject,70);
    }    
    private void ClampPitch(GameObject go, float theshold) {
        Vector3 rot = go.transform.localEulerAngles;
        if (rot.x > 180) rot.x -= 360;
        if (rot.x > theshold)
            rot.x = theshold;
        else if (rot.x < -theshold)
            rot.x = -theshold;
        go.transform.localEulerAngles = rot;
    }
    private void CalculateInertiaTally(Vector2 delta){
        runningInertialSpeed = Vector2.Lerp(runningInertialSpeed, delta, Time.deltaTime * 5);
    }        
    private void ApplyInertia(){
        float frameDampen = Time.deltaTime * inertia;
        inertiaSpeed = Vector2.Lerp(inertiaSpeed, Vector2.zero, frameDampen);
        if (inertiaSpeed.magnitude > inertialDeadZone)
        {
            rootCamera.transform.Rotate(0,inertiaSpeed.x,0);
            yawCamera.transform.Rotate(inertiaSpeed.y,0,0,Space.Self);
        }
    }
    private void OnGUI() {
        if (false) {
            GUIUtility.ScaleAroundPivot(Vector2.one * 10, Vector2.one);
            GUI.Button(new Rect(0, 0, 80, 40), $"snappiness\n{snappiness}");
            if (GUI.Button(new Rect(0, 42, 40, 40), $"-")) snappiness -= 0.1f;
            if (GUI.Button(new Rect(40, 42, 40, 40), $"+")) snappiness += 0.1f;
        }
    }
}
