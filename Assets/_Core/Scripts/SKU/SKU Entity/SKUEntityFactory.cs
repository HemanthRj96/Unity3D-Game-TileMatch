using UnityEngine;


/// <summary>
/// Responsible for the creation of SKU creation after providing with parent transform and SKU ID, a monobehaviour that 
/// instantiates prefabs and generates SKUEntity ready to be added to the grid
/// </summary>
public class SKUEntityFactory : MonoBehaviour
{
    // Fields

    [Tooltip("The ScriptableObject repository containing all SKU asset data.")]
    [SerializeField]
    private SKUAssetRepository assetRepository;


    // Public methods

    public SKUEntity CreateSKUEntityGameObject(string skuId, Transform parent)
    {
        if (assetRepository == null)
        {
            Debug.LogError("SKUEntityFactory: assetRepository is not assigned. Cannot create SKU.");
            return null;
        }

        SKUAssetContainer assetContainer = assetRepository.GetAsset(skuId);
        if (assetContainer == null)
            return null;

        GameObject newGameObject;

        if (assetContainer.prefab != null)
        {
            newGameObject = Instantiate(assetContainer.prefab, parent);
        }
        else
        {
            newGameObject = new GameObject(assetContainer.displayName);
            newGameObject.transform.parent = parent;
        }

        SKUEntity skuEntity = new SKUEntity();

        // Add the components from the asset container to the new entity.
        skuEntity.AddComponent(new IdentificationComponent(assetContainer.skuID, assetContainer.displayName));

        if (assetContainer.brandComponent != null)
            skuEntity.AddComponent(assetContainer.brandComponent);
        if (assetContainer.categoryComponent != null)
            skuEntity.AddComponent(assetContainer.categoryComponent);
        if (assetContainer.packagingInfoComponent != null)
            skuEntity.AddComponent(assetContainer.packagingInfoComponent);
        if (assetContainer.temperatureInfoComponent != null)
            skuEntity.AddComponent(assetContainer.temperatureInfoComponent);
        if (assetContainer.zoneComponent != null)
            skuEntity.AddComponent(assetContainer.zoneComponent);
        if (assetContainer.entityTypeComponent != null)
            skuEntity.AddComponent(assetContainer.entityTypeComponent);
        if (assetContainer.genericMatchComponent != null)
            skuEntity.AddComponent(assetContainer.genericMatchComponent);

        skuEntity.AddComponent(new UnityMetadataComponent(newGameObject, assetContainer.icon));
        return skuEntity;
    }
}