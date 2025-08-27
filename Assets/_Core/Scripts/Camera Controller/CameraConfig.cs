using UnityEngine;



/// <summary>
/// A ScriptableObject to hold all configurable camera settings.
/// This allows for easy management and reuse of camera configurations
/// across different scenes or objects.
/// </summary>
[CreateAssetMenu(fileName = "New Camera Config", menuName = "SWB/Camera/Camera Config", order = 1)]
public class CameraConfig : ScriptableObject
{
    [Header("Movement & Rotation")]
    public float rotationSensitivity = 2f;
    public float zoomSensitivity = 5f;

    [Header("Dampening")]
    public float rotationSmoothTime = 0.3f;
    public float zoomSmoothTime = 0.3f;
    public float lookAtSmoothTime = 0.5f;

    [Header("Arm Length & Pitch")]
    public float defaultArmLength = 10f;
    public float minZoomDistance = 5f;
    public float maxZoomDistance = 25f;
    public float minPitchAngle = 30f;
    public float maxPitchAngle = 80f;

    [Header("Organic Floating")]
    public float floatingFrequency = 0.5f;
    public float floatingAmplitude = 0.1f;

    [Header("Dynamic Zoom")]
    public float zoomInOffset = 5f;
    public float zoomReturnTime = 2.0f;
    public float zoomOutResetThreshold = 5.0f;

    [Header("Gizmos")]
    public Color gizmoColor = Color.yellow;
    public float gizmoSphereRadius = 0.5f;
}
