using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

public class ScaleWithCameraFocus : MonoBehaviour
{
    [FormerlySerializedAs("growDuration")] public float duration;
    
    public Transform cam;
    
    private float startScale;
    public float maxScale = 1.4f;
    
    public bool debugging;

    public float focusThreshold = 0f;
    public float zFocus;
    
    public enum FocusType {Linear, Cubic}

    public FocusType focusType = FocusType.Linear;
    
    private void Start()
    {
        startScale = transform.localScale.x;
        cam = Camera.main.transform;
    }
    
    public void Update()
    {
        Vector3 vToTarget = transform.position - cam.position;
        zFocus = Vector3.Dot(cam.forward, vToTarget.normalized);

        switch (focusType)
        {
            case FocusType.Linear:
                zFocus = Mathf.Max(0f, zFocus);
                break;
            case FocusType.Cubic:
                zFocus = Mathf.Max(0f, zFocus * zFocus * zFocus);
                break;
        }
        
        //Then scale it in a range from threshold through 1
        zFocus = Remap(zFocus, focusThreshold, 1f, 0f, 1f);
        
        //Scale it!
        float growthAmount = maxScale - startScale;
        float desiredScale = Mathf.Clamp(startScale + growthAmount * zFocus, 1f, maxScale);
        transform.localScale = Vector3.one * desiredScale;
             
        if (debugging) Debug.Log($"DesiredScale: {desiredScale}".Timestamped());
    }
    
    float Remap (float from, float fromMin, float fromMax, float toMin,  float toMax)
    {
        var fromAbs  =  from - fromMin;
        var fromMaxAbs = fromMax - fromMin;      
       
        var normal = fromAbs / fromMaxAbs;
 
        var toMaxAbs = toMax - toMin;
        var toAbs = toMaxAbs * normal;
 
        var to = toAbs + toMin;
       
        return to;
    }
}