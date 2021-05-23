using UnityEngine;
using System.Collections;

public class LineDrawer : MonoBehaviour
{
    //reference to LineRenderer component
    public LineRenderer lineRenderer;

    public Transform originTransform;
    public Transform targetTransform;

    bool debugging = false;

    void Update()
    {
        Vector3 adjustedPos = originTransform.position;
        adjustedPos.z = 0.5f;
        Vector3 screenPos = Camera.main.ScreenToWorldPoint(adjustedPos);

        if (debugging)
        {
            Debug.Log("Panel Pos: " + transform.position + " || Screen Pos: " + screenPos);
        }

        Vector3 screenPosDestination = Camera.main.WorldToScreenPoint(targetTransform.position);
        screenPosDestination.z = 0.5f;
        screenPosDestination = Camera.main.ScreenToWorldPoint(screenPosDestination);

        // Set the start point and end point of the line renderer
        lineRenderer.SetPosition(0, screenPos);
        lineRenderer.SetPosition(1, screenPosDestination);   
    }

    private void OnEnable()
    {
        lineRenderer.enabled = true;
    }

    private void OnDisable()
    {
        lineRenderer.enabled = false;
    }

}