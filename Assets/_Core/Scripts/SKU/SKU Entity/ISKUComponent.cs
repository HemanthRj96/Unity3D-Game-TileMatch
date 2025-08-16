using UnityEngine;

public interface ISKUComponent { }

[System.Serializable]
public class BrandComponent : ISKUComponent
{
    public BrandType brand;
}

[System.Serializable]
public class CategoryComponent : ISKUComponent
{
    public CategoryType category;
}


[System.Serializable]
public class IdentificationComponent : ISKUComponent
{
    public string skuID;
    public string displayName;

    public IdentificationComponent(string skuID, string displayName)
    {
        this.skuID = skuID;
        this.displayName = displayName;
    }
}

[System.Serializable]
public class PackagingInfoComponent : ISKUComponent
{
    public PackagingSize size;
    public ContainerType containerType;
    public bool isSingleServe;
}

[System.Serializable]
public class TemperatureInfoComponent : ISKUComponent
{
    public TemperatureType type;
    public bool isColdCompatible;
}

[System.Serializable]
public class ZoneComponent : ISKUComponent
{
    public ZoneType zoneType;
}

[System.Serializable]
public class EntityTypeComponent : ISKUComponent
{
    public EntityType entityType;
}

[System.Serializable]
public class GenericMatchComponent : ISKUComponent
{
    public int value;
    public Color color;

    public GenericMatchComponent(int value, Color color)
    {
        this.value = value;
        this.color = color;
    }
}

[System.Serializable]
public class UnityMetadataComponent : ISKUComponent
{
    // Fields

    public GameObject prefab;
    public Sprite iconSprite;
    public MeshRenderer meshRenderer;

    private float _idleTime;
    private Quaternion _initialRotation;


    // Constructors

    public UnityMetadataComponent(GameObject go, Sprite icon)
    {
        prefab = go;
        iconSprite = icon;
        _initialRotation = prefab.transform.rotation;
    }


    // Public Methods

    public void DisableComponent() => prefab.SetActive(false);

    public void EnableComponent() => prefab.SetActive(true);

    public void ApplyVisuals(Color color, Sprite sprite)
    {
        if (meshRenderer != null)
            meshRenderer.material.color = color;
    }

    public void AnimateIdle()
    {
        _idleTime += Time.deltaTime;
        float rotationAngle = Mathf.Sin(_idleTime * 2f) * 10f;
        prefab.transform.rotation = _initialRotation * Quaternion.Euler(0, rotationAngle, 0);
    }
}