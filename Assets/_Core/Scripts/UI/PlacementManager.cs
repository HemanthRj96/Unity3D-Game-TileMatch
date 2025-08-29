using UnityEngine;
using System;


public class PlacementManager : MonoBehaviour
{
    // Fields
    [Header("Dependencies")]
    [Tooltip("Reference to the GridManager to handle entity spawning.")]
    [SerializeField] private GridManager _gridManager;
    [Tooltip("Reference to the GridCellSelector to manage selection state.")]
    [SerializeField] private GridCellSelector _gridCellSelector;
    [SerializeField] private InventoryUI _inventoryUI;

    // Internal state variables to track the currently selected SKU and cell.
    private SKUAssetContainer _selectedSKU;
    private Vector3Int _selectedGridIndex;

    // Lifecycle methods
    private void Awake()
    {
        _inventoryUI.PopulateInventoryUI();
    }

    private void OnEnable()
    {
        // Subscribe to events from the UI and the GridCellSelector.
        InventoryUI.OnSKUSelected += OnSKUSelected;
        GridCellSelector.OnCellSelected += OnCellSelected;
    }

    private void OnDisable()
    {
        // Unsubscribe from events to prevent memory leaks.
        InventoryUI.OnSKUSelected -= OnSKUSelected;
        GridCellSelector.OnCellSelected -= OnCellSelected;
    }

    // Event handler methods

    /// <summary>
    /// Event handler for when a new SKU is selected from the Inventory UI.
    /// </summary>
    /// <param name="asset">The SKU asset that was selected.</param>
    private void OnSKUSelected(SKUAssetContainer asset)
    {
        _selectedSKU = asset;
        _gridCellSelector.SetSelectorState(true);
        _gridCellSelector.DeselectCurrentCell();
        _inventoryUI.TogglePanel(false);
        tryPlaceSKU();
    }

    /// <summary>
    /// Event handler for when a grid cell is selected by the user.
    /// </summary>
    /// <param name="cellIndex">The index of the selected cell.</param>
    private void OnCellSelected(Vector3Int cellIndex)
    {
        _selectedGridIndex = cellIndex;
        if (tryDeleteSKU())
        {
            _gridCellSelector.DeselectCurrentCell();
        }
        else
        {
            _gridCellSelector.SetSelectorState(false);
            _inventoryUI.TogglePanel(true);
        }
    }

    // Public methods

    private bool tryDeleteSKU()
    {
        var entity = _gridManager.GetEntityAt(_selectedGridIndex);

        if (entity != null)
        {
            _gridManager.DespawnEntityAt(_selectedGridIndex);
            return true;
        }
        return false;
    }


    /// <summary>
    /// Attempts to place the currently selected SKU at the currently selected grid index.
    /// </summary>
    private void tryPlaceSKU()
    {
        var entity = _gridManager.GetEntityAt(_selectedGridIndex);

        _gridManager.SpawnEntityAt(_selectedSKU.skuID, _selectedGridIndex);
    }
}
