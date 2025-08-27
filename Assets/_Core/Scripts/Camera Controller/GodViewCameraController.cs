using UnityEngine;



/// <summary>
/// Implements a "God View" or "Overhead Map" camera controller with a pivot-arm system.
/// This version includes a specific "Focus Mode" that locks rotation and allows
/// for persistent manual zoom on a target.
/// </summary>
public class GodViewCameraController : MonoBehaviour
{
    // Defines the camera's operational mode
    public enum CameraMode
    {
        Free, // Standard orbit, pan, and zoom
        Focus // Zoomed in on a target, rotation locked
    }

    [Header("Camera Configuration")]
    public CameraConfig cameraConfig;
    public Camera mainCamera;
    [Tooltip("The pivot point the camera will orbit around. If null, uses a world-space position.")]
    public Transform anchorTransform;
    [Tooltip("The point the camera will always look at. If null, the camera looks at the anchor.")]
    public Transform lookAtTransform;

    // Internal state variables
    private Vector3 _currentRotation;
    private Vector3 _rotationVelocity;
    private float _currentArmLength;
    private float _zoomVelocity;
    private Vector3 _currentAnchorPosition;
    private Vector3 _currentLookAtPosition;
    private Vector3 _floatingOffset;
    private Vector3 _floatingVelocity;
    private float _targetZoom;
    private float _targetZoomVelocity;

    private CameraMode _currentMode = CameraMode.Free;

    /// <summary>
    /// Gets the current arm length of the camera from the pivot.
    /// Used by other scripts to check the zoom level.
    /// </summary>
    public float CurrentArmLength
    {
        get { return _currentArmLength; }
    }

    /// <summary>
    /// Initializes the camera's state.
    /// </summary>
    void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("GodViewCameraController requires a main camera in the scene.");
                enabled = false;
                return;
            }
        }

        // Initialize the camera's starting position and rotation
        _currentArmLength = cameraConfig.defaultArmLength;
        _currentRotation = new Vector3(cameraConfig.minPitchAngle, 0f, 0f);
        _currentAnchorPosition = anchorTransform != null ? anchorTransform.position : Vector3.zero;
        _currentLookAtPosition = lookAtTransform != null ? lookAtTransform.position : _currentAnchorPosition;
        transform.position = _currentAnchorPosition;
        transform.eulerAngles = _currentRotation;
        mainCamera.transform.position = transform.position - transform.forward * _currentArmLength;
        mainCamera.transform.LookAt(transform);
        _targetZoom = _currentArmLength;
    }

    /// <summary>
    /// Handles user input.
    /// </summary>
    void Update()
    {
        HandleInput();
    }

    /// <summary>
    /// Applies the camera's new position and rotation.
    /// </summary>
    void LateUpdate()
    {
        ApplyDampening();
        ApplyOrganicFloating();
    }

    /// <summary>
    /// Handles all user input for camera control based on the current mode.
    /// </summary>
    private void HandleInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        switch (_currentMode)
        {
            case CameraMode.Free:
                HandleFreeModeInput(scroll);
                break;
            case CameraMode.Focus:
                HandleFocusModeInput(scroll);
                break;
        }
    }

    /// <summary>
    /// Handles input when the camera is in Free Mode.
    /// </summary>
    /// <param name="scroll">The scroll wheel input value.</param>
    private void HandleFreeModeInput(float scroll)
    {
        // Mouse drag for rotation (yaw and pitch)
        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X") * cameraConfig.rotationSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * cameraConfig.rotationSensitivity;
            _currentRotation.x -= mouseY;
            _currentRotation.y += mouseX;
            _currentRotation.x = Mathf.Clamp(_currentRotation.x, cameraConfig.minPitchAngle, cameraConfig.maxPitchAngle);
        }

        // Mouse scroll wheel for zooming
        if (Mathf.Abs(scroll) > 0)
        {
            _targetZoom -= scroll * cameraConfig.zoomSensitivity;
            _targetZoom = Mathf.Clamp(_targetZoom, cameraConfig.minZoomDistance, cameraConfig.maxZoomDistance);
        }
    }

    /// <summary>
    /// Handles input when the camera is in Focus Mode.
    /// </summary>
    /// <param name="scroll">The scroll wheel input value.</param>
    private void HandleFocusModeInput(float scroll)
    {
        // Only allow zooming. Rotation is locked.
        if (Mathf.Abs(scroll) > 0)
        {
            _targetZoom -= scroll * cameraConfig.zoomSensitivity;
            _targetZoom = Mathf.Clamp(_targetZoom, cameraConfig.minZoomDistance, cameraConfig.maxZoomDistance);
        }

        // Check if we've zoomed out enough to exit focus mode
        float resetThreshold = cameraConfig.minZoomDistance * (1 + cameraConfig.zoomOutResetThreshold);
        if (_currentArmLength > resetThreshold)
        {
            ExitFocusMode();
        }
    }

    /// <summary>
    /// Smoothly applies all movements and rotations.
    /// </summary>
    private void ApplyDampening()
    {
        // Smoothly pan the pivot to its target position
        _currentAnchorPosition = anchorTransform != null ? anchorTransform.position : Vector3.zero;
        transform.position = _currentAnchorPosition;

        // Smoothly rotate the pivot to the target rotation
        Quaternion targetQuaternion = Quaternion.Euler(_currentRotation);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetQuaternion, Time.deltaTime / cameraConfig.rotationSmoothTime);

        // Smoothly adjust the camera's arm length
        _currentArmLength = Mathf.SmoothDamp(_currentArmLength, _targetZoom, ref _zoomVelocity, cameraConfig.zoomSmoothTime);

        // Apply the dampened position
        Vector3 targetLocalPos = new Vector3(0, 0, -_currentArmLength);
        mainCamera.transform.localPosition = Vector3.SmoothDamp(mainCamera.transform.localPosition, targetLocalPos, ref _rotationVelocity, cameraConfig.zoomSmoothTime);

        // Dampen the look-at behavior for smooth transitions
        Vector3 targetLookAt = lookAtTransform != null ? lookAtTransform.position : transform.position;
        _currentLookAtPosition = Vector3.Lerp(_currentLookAtPosition, targetLookAt, Time.deltaTime / cameraConfig.lookAtSmoothTime);
        mainCamera.transform.LookAt(_currentLookAtPosition);

        // Ensure camera never clips below the ground plane (y=0)
        Vector3 cameraPos = mainCamera.transform.position;
        if (cameraPos.y < 0)
        {
            cameraPos.y = 0;
            mainCamera.transform.position = cameraPos;
        }
    }

    /// <summary>
    /// Adds a subtle, smooth floating movement to the camera's final position.
    /// </summary>
    private void ApplyOrganicFloating()
    {
        float time = Time.time * cameraConfig.floatingFrequency;
        Vector3 newOffset = new Vector3(
            Mathf.PerlinNoise(time, 0) * 2 - 1,
            Mathf.PerlinNoise(0, time) * 2 - 1,
            0);

        _floatingOffset = Vector3.SmoothDamp(_floatingOffset, newOffset * cameraConfig.floatingAmplitude, ref _floatingVelocity, 1.0f);
        mainCamera.transform.position += _floatingOffset;
    }

    /// <summary>
    /// Public method to set the camera's look-at target and enter focus mode.
    /// </summary>
    public void EnterFocusMode(Transform target)
    {
        lookAtTransform = target;
        // NOTE: We no longer set _currentLookAtPosition here to allow for smooth transitions.
        _currentMode = CameraMode.Focus;
        _targetZoom = cameraConfig.minZoomDistance; // Snap to minimum zoom when entering focus mode
    }

    /// <summary>
    /// Public method to exit focus mode and return to the default state.
    /// </summary>
    public void ExitFocusMode()
    {
        lookAtTransform = null; // Look at the default anchor
        _currentMode = CameraMode.Free;
        _targetZoom = cameraConfig.defaultArmLength; // Return to default zoom
    }

    /// <summary>
    /// Draws gizmos in the Scene view for easy setup and visualization.
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = cameraConfig.gizmoColor;
        Gizmos.DrawSphere(transform.position, cameraConfig.gizmoSphereRadius);
        if (mainCamera != null) Gizmos.DrawLine(transform.position, mainCamera.transform.position);
        Gizmos.color = Color.green;
        if (mainCamera != null) Gizmos.DrawSphere(mainCamera.transform.position, cameraConfig.gizmoSphereRadius);
        Gizmos.color = cameraConfig.gizmoColor;
        DrawWireCircle(transform.position, transform.up, _currentArmLength, 32);
    }

    /// <summary>
    /// Helper function to draw a wire circle.
    /// </summary>
    private void DrawWireCircle(Vector3 center, Vector3 normal, float radius, int segments)
    {
        Quaternion rotation = Quaternion.LookRotation(normal);
        Vector3 startPoint = rotation * Vector3.forward * radius;
        for (int i = 0; i <= segments; i++)
        {
            Vector3 endPoint = rotation * Quaternion.Euler(0, (float)i / segments * 360f, 0) * Vector3.forward * radius;
            Gizmos.DrawLine(center + startPoint, center + endPoint);
            startPoint = endPoint;
        }
    }
}
