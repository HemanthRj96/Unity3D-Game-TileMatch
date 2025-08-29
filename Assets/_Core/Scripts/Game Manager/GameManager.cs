using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// This class acts as the central manager for the game logic,
/// orchestrating interactions between the UI and core grid components.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GridCellSelector _gridCellSelector;
    [SerializeField] private GridFillHelper _gridFillHelper;
    [SerializeField] private GridManager _gridManager;

    [Header("UI References")]
    [SerializeField] private GameObject _inventoryPanel;
    [SerializeField] private Transform _skuListContainer;
    [SerializeField] private Button _resetButton;
    [SerializeField] private Button _submitButton;

    [Header("SKU Assets & UI Prefabs")]
    [SerializeField] private SKUAssetRepository _skuRepository;
    [SerializeField] private GameObject _skuListItemPrefab; // Prefab for a single SKU button or UI element

    // Store the index of the currently selected grid cell.
    private Vector3Int _selectedCellIndex;
    private bool _isCellSelected = false;

    // A list to hold our pooled UI buttons for reuse.
    private List<GameObject> _pooledButtons = new List<GameObject>();

    /// <summary>
    /// This method is called when a SKU is selected from the UI inventory.
    /// It attempts to spawn the selected SKU at the currently selected grid cell.
    /// </summary>
    /// <param name="skuId">The ID of the SKU to place.</param>
    public void PlaceSelectedSKU(string skuId)
    {
        if (_isCellSelected)
        {
            // First, try to despawn any existing item to ensure the cell is clear.
            _gridManager.DespawnEntityAt(_selectedCellIndex);
            // Then, spawn the new item.
            _gridManager.SpawnEntityAt(skuId, _selectedCellIndex);
        }
        else
        {
            Debug.LogWarning("No cell is currently selected. Cannot place SKU.");
        }
        // Call the method to handle deselection and hide the UI.
        OnCellDeselected();
    }

    /// <summary>
    /// Handles the click event for the Reset button.
    /// This method clears and refills the grid by calling the GridFillHelper.
    /// </summary>
    public void OnResetButtonClicked()
    {
        Debug.Log("Reset button clicked. Restacking the cooler.");
        // The GridFillHelper's method already handles clearing the grid before refilling it.
        _gridFillHelper.FillEquipmentGrid();
        _gridCellSelector.DeselectCurrentCell(); // Deselect any active cell
    }

    /// <summary>
    /// Handles the click event for the Submit button.
    /// This is a placeholder for custom filter logic. You can implement
    /// your filtering and analysis code here.
    /// </summary>
    public void OnSubmitButtonClicked()
    {
        Debug.Log("Submit button clicked. Executing custom filter logic.");

        // Example: Get all front-facing entities to start your custom logic.
        List<SKUEntity> frontEntities = _gridManager.GetFrontFacingEntities();

        // Implement your custom filter logic here. For example:
        // var filteredItems = frontEntities.Where(e => e.someCondition);
        // Do something with filteredItems...
    }

    private void Awake()
    {
        // Ensure all dependencies are assigned.
        if (_gridCellSelector == null) Debug.LogError("GridCellSelector not assigned.");
        if (_gridFillHelper == null) Debug.LogError("GridFillHelper not assigned.");
        if (_gridManager == null) Debug.LogError("GridManager not assigned.");
        if (_skuRepository == null) Debug.LogError("SKUAssetRepository not assigned.");

        // Ensure UI references are assigned.
        if (_inventoryPanel == null) Debug.LogError("Inventory Panel UI not assigned.");
        if (_skuListContainer == null) Debug.LogError("SKU List Container not assigned.");
        if (_skuListItemPrefab == null) Debug.LogError("SKU List Item Prefab not assigned.");

        // Initially hide the inventory panel.
        _inventoryPanel.SetActive(false);
    }

    private void Start()
    {
        // Fill the grid at the start of the scene.
        _gridFillHelper.FillEquipmentGrid();

        // Pre-populate the object pool. We'll create enough buttons for all SKUs.
        PrepopulateButtonPool();
    }

    private void OnEnable()
    {
        // Subscribe to events from the GridCellSelector.
        GridCellSelector.OnCellSelected += OnCellSelected;
        GridCellSelector.OnCellDeselected += OnCellDeselected;

        // Set up button listeners.
        if (_resetButton != null)
        {
            _resetButton.onClick.AddListener(OnResetButtonClicked);
        }
        if (_submitButton != null)
        {
            _submitButton.onClick.AddListener(OnSubmitButtonClicked);
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from events to prevent memory leaks.
        GridCellSelector.OnCellSelected -= OnCellSelected;
        GridCellSelector.OnCellDeselected -= OnCellDeselected;

        // Remove button listeners.
        if (_resetButton != null)
        {
            _resetButton.onClick.RemoveListener(OnResetButtonClicked);
        }
        if (_submitButton != null)
        {
            _submitButton.onClick.RemoveListener(OnSubmitButtonClicked);
        }
    }

    /// <summary>
    /// Creates and hides a full set of buttons to be reused later.
    /// </summary>
    private void PrepopulateButtonPool()
    {
        var allSkus = _skuRepository.GetAllSKUAssets();
        foreach (var sku in allSkus)
        {
            GameObject listItem = Instantiate(_skuListItemPrefab, _skuListContainer);
            listItem.SetActive(false); // Hide it immediately.
            _pooledButtons.Add(listItem);

            // Set up the button's click event once.
            Button button = listItem.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => PlaceSelectedSKU(sku.skuID));
            }
        }
    }

    /// <summary>
    /// Populates the inventory UI by reusing buttons from the object pool.
    /// </summary>
    private void PopulateInventoryUI()
    {
        // Reset all buttons in the pool to be inactive.
        foreach (var button in _pooledButtons)
        {
            button.SetActive(false);
        }

        // Get all available SKUs from the repository.
        var allSkus = _skuRepository.GetAllSKUAssets();

        // Now, activate and set the content for the buttons we need.
        for (int i = 0; i < allSkus.Count && i < _pooledButtons.Count; i++)
        {
            var sku = allSkus[i];
            var listItem = _pooledButtons[i];

            // Set the text of the button to the SKU's display name.
            var buttonText = listItem.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = sku.skuID;
            }

            // Activate the button.
            listItem.SetActive(true);
        }
    }

    /// <summary>
    /// Event handler for when a grid cell is selected.
    /// </summary>
    private void OnCellSelected(Vector3Int cellIndex)
    {
        // Check if the cell already has an item.
        if (_gridManager.Grid.GetCellContent(cellIndex) != null)
        {
            Debug.Log($"Cell at index: {cellIndex} has an item. Despawning it.");
            _gridManager.DespawnEntityAt(cellIndex);
            // We now use the new parameter to deselect the tile without causing a cascading event.
            _gridCellSelector.DeselectCurrentCell(); // Do not invoke OnCellDeselected
            return;
        }

        Debug.Log($"Cell selected at index: {cellIndex}");
        _selectedCellIndex = cellIndex;
        _isCellSelected = true;
        _inventoryPanel.SetActive(true); // Show the UI inventory.
        _gridCellSelector.SetSelectorState(false); // Disable the grid selector.
        PopulateInventoryUI(); // Dynamically populate the list.
    }

    /// <summary>
    /// Event handler for when a grid cell is deselected.
    /// </summary>
    private void OnCellDeselected()
    {
        Debug.Log("Cell deselected.");
        _isCellSelected = false;
        _inventoryPanel.SetActive(false); // Hide the UI inventory.
        _gridCellSelector.SetSelectorState(true); // Re-enable the grid selector.
    }
}
