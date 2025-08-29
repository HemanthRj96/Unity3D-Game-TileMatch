// CameraHotspot.cs - A self-contained component for interactive hotspots
using UnityEngine;
using UnityEngine.EventSystems;


public class CameraHotspot : MonoBehaviour
{
    Camera mainCamera;

    [Tooltip("The desired size of the UI in pixels on a 1920x1080 screen.")]
    public float pixelSize = 50f;


    private float initialScale;

    void Awake()
    {
        // Find the main camera if one isn't assigned
        mainCamera = Camera.main;
        initialScale = transform.localScale.x;
    }

    void Update()
    {
        // Make the UI face the camera
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                         mainCamera.transform.rotation * Vector3.up);

        // Calculate the distance to the camera
        float distance = Vector3.Distance(transform.position, mainCamera.transform.position);

        // Calculate the new scale based on distance and camera field of view
        float scaleFactor = distance * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) * (2.0f / mainCamera.pixelHeight);

        // Apply the new scale to maintain constant perceived size
        transform.localScale = Vector3.one * (initialScale * pixelSize) * scaleFactor;
    }
}
