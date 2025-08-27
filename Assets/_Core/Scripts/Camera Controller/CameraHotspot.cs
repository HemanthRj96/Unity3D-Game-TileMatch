// CameraHotspot.cs - A self-contained component for interactive hotspots
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// This script makes a 3D object an interactive "hotspot."
/// When clicked, it tells the camera to enter a "focus mode."
/// It also handles its own UI display based on its proximity to the camera.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CameraHotspot : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("The camera controller to interact with.")]
    public GodViewCameraController godViewCamera;
    [Tooltip("The UI prefab for this hotspot.")]
    public GameObject uiPrefab;
    [Tooltip("The canvas to parent the UI prefabs to. If null, will try to find one.")]
    public Transform uiCanvas;

    [Header("UI & Interaction")]
    [Tooltip("The world-space distance where UI begins to scale up.")]
    public Vector3 uiOffest;
    public float scaleUpDistance = 5f;

    // Internal state
    private GameObject _uiInstance;

    void Start()
    {
        // Check for dependencies
        if (godViewCamera == null)
        {
            Debug.LogError("GodViewCameraController not found. Please assign it or add it to the scene.");
            enabled = false;
            return;
        }

        if (uiCanvas == null)
        {
            Debug.LogError("UI Canvas not found. Please assign it or add it to the scene.");
            enabled = false;
            return;
        }

        // Instantiate the UI prefab and parent it to the world space canvas
        if (uiPrefab != null)
        {
            _uiInstance = Instantiate(uiPrefab, uiCanvas);
        }
    }

    void Update()
    {
        // Update the position and scale of the UI instance
        if (_uiInstance != null)
        {
            // Position UI to match the hotspot's world position
            _uiInstance.transform.position = transform.position + uiOffest;

            // Make the UI always face the camera (billboard effect)
            _uiInstance.transform.LookAt(_uiInstance.transform.position + godViewCamera.mainCamera.transform.rotation * Vector3.forward,
                                         godViewCamera.mainCamera.transform.rotation * Vector3.up);

            // Dynamically scale UI as the camera gets closer
            float distance = Vector3.Distance(transform.position, godViewCamera.mainCamera.transform.position);
            float scale = Mathf.Clamp(distance / scaleUpDistance, 0.1f, 1f);
            _uiInstance.transform.localScale = Vector3.one * scale;
        }
    }

    /// <summary>
    /// Handles the mouse click event on this object.
    /// </summary>
    private void OnMouseDown()
    {
        // Don't do anything if we are over a UI element.
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // Tell the camera controller to enter focus mode on this object
        godViewCamera.EnterFocusMode(this.transform);
    }
}
