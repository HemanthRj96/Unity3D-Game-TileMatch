using UnityEngine;


public class PlacementManager : MonoBehaviour
{
    // Fields

    [SerializeField] private GridManager _gridManager;
    [SerializeField] private GridCellSelector _cellSelector;
    [SerializeField] private SKUEntityFactory _skuFactory;
    [SerializeField] private InventoryUI _inventoryUI;

    private Vector3Int? _selectedGridIndex;


    // Lifecycle methods

    private void OnEnable()
    {
        //_cellSelector.OnCellSelectionEvent += HandleGridCellSelected;
        InventoryUI.OnSKUSelected += HandleSKUSelected;
    }

    private void OnDisable()
    {
        //_cellSelector.OnCellSelectionEvent -= HandleGridCellSelected;
        InventoryUI.OnSKUSelected -= HandleSKUSelected;
    }

    private void HandleGridCellSelected(Vector3Int gridIndex)
    {
        _selectedGridIndex = gridIndex;
        _inventoryUI.TogglePanel(true);
        _inventoryUI.PopulateInventoryUI();
    }

    private void HandleSKUSelected(SKUAssetContainer assetContainer)
    {
        if (!_selectedGridIndex.HasValue)
        {
            Debug.LogError("No grid cell selected before trying to place an SKU.");
            return;
        }

        // Create the SKUEntity and its GameObject
        SKUEntity newSkuEntity = _skuFactory.CreateSKUEntityGameObject(assetContainer.skuID, null);
        GameObject skuGameObject = newSkuEntity.GetComponent<UnityMetadataComponent>().skuGameObject;
        
        // This is the SKU gameObject
        skuGameObject.transform.position = _gridManager.Grid.GetCellCenter(_selectedGridIndex.Value) -
                                          (Vector3.up * (_gridManager.Grid.CellDimension.y / 2));

        // Load the SKUEntity into the grid
        //_gridManager.LoadEntityToGridAt(newSkuEntity, _selectedGridIndex.Value);
        _selectedGridIndex = null; // Reset for next placement
        _inventoryUI.TogglePanel(false);
    }
}