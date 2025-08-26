using UnityEngine;

/// <summary>
/// A component to store references to Unity-specific objects, like the GameObject and its Renderer.
/// This prevents the core SKUEntity from needing to know about Unity's implementation.
/// </summary>
[System.Serializable]
public class UnityMetadataComponent : ISKUComponent
{
    // Fields

    public GameObject skuGameObject;
    public Sprite iconSprite;
    public MeshRenderer meshRenderer;

    private float _idleTime;
    private Quaternion _initialRotation;

    // Constructors

    public UnityMetadataComponent(GameObject go, Sprite icon)
    {
        skuGameObject = go;
        iconSprite = icon;
        // Correctly get and assign the MeshRenderer on creation.
        meshRenderer = go.GetComponentInChildren<MeshRenderer>();
        _initialRotation = skuGameObject.transform.rotation;
    }


    // Public Methods

    public void DisableComponent() => skuGameObject.SetActive(false);

    public void EnableComponent() => skuGameObject.SetActive(true);

    public void ApplyVisuals(Material material)
    {
        if (meshRenderer != null)
            meshRenderer.material = material;
    }

    public void ResetAnimation()
    {
        skuGameObject.transform.rotation = _initialRotation;
    }

    public void AnimateIdle()
    {
        _idleTime += Time.deltaTime;
        float rotationAngle = Mathf.Sin(_idleTime * 2f) * 10f;
        skuGameObject.transform.rotation = _initialRotation * Quaternion.Euler(rotationAngle, rotationAngle, rotationAngle);
    }
}
