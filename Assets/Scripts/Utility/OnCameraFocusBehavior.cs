using UnityEngine;
using UnityEngine.Events;

public class OnCameraFocusBehavior : MonoBehaviour
{
    public UnityEvent OnGainFocus;
    public UnityEvent OnLoseFocus;
    
    public float focusThreshold;
    private bool focused = false;
    [SerializeField]
    private float focusValue;
    public float FocusValue => focusValue;

    public Transform cam;
    
    void Start()
    {
        if(cam == null) cam = Camera.main.transform;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (cam == null) return;
        
        focusValue = GetFocusValue();

        if (!focused)
        {
            if (focusValue > focusThreshold)
            {
                focused = true;
                OnGainFocus?.Invoke();
            }
        }
        else
        {
            if (FocusValue < focusThreshold)
            {
                focused = false;
                OnLoseFocus?.Invoke();
            }
        }
        
    }

    float GetFocusValue()
    {
        Vector3 vToObject = transform.position - cam.position;

        float zIsFocused = Vector3.Dot(vToObject.normalized, cam.forward);

        return zIsFocused;
    }
}
