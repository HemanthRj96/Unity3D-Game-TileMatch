using UnityEngine;
using System;
using System.Linq;
using System.Reflection;



/// <summary>
/// A factory class responsible for the creation and initialization of SKU entities.
/// This MonoBehaviour instantiates prefabs and generates a complete SKUEntity ready to be added to the grid.
/// </summary>
public class SKUEntityFactory : MonoBehaviour
{
    // Fields

    [Tooltip("The ScriptableObject repository containing all SKU asset data.")]
    [SerializeField]
    private SKUAssetRepository _assetRepository;


    // Public methods

    /// <summary>
    /// Creates a new SKUEntity and its corresponding GameObject.
    /// </summary>
    /// <param name="skuId">The unique ID of the SKU to create.</param>
    /// <param name="parent">The transform to parent the new GameObject to.</param>
    /// <returns>A fully initialized SKUEntity object.</returns>
    public SKUEntity CreateSKUEntityGameObject(string skuId, Transform parent)
    {
        if (_assetRepository == null)
        {
            Debug.LogError("SKUEntityFactory: _assetRepository is not assigned. Cannot create SKU.");
            return null;
        }

        SKUAssetContainer assetContainer = _assetRepository.GetAsset(skuId);
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
        initializeComponentsFromAsset(skuEntity, assetContainer);
        skuEntity.AddComponent(new UnityMetadataComponent(newGameObject, assetContainer.icon));
        return skuEntity;
    }


    // Private methods

    /// <summary>
    /// Populates the SKUEntity with all components from the SKUAssetContainer using reflection.
    /// This is more scalable than a long list of manual if-checks.
    /// </summary>
    private void initializeComponentsFromAsset(SKUEntity skuEntity, SKUAssetContainer assetContainer)
    {
        // Get all public fields from the SKUAssetContainer.
        FieldInfo[] fields = typeof(SKUAssetContainer).GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            // Check if the field is an ISKUComponent.
            if (typeof(ISKUComponent).IsAssignableFrom(field.FieldType))
            {
                // Get the component value.
                ISKUComponent component = field.GetValue(assetContainer) as ISKUComponent;
                // Add the component if it exists.
                if (component != null)
                {
                    skuEntity.AddComponent(component);
                }
            }
        }
        // Always add the IdentificationComponent since it's not a field in the container.
        skuEntity.AddComponent(new IdentificationComponent(assetContainer.skuID, assetContainer.displayName));
    }
}
