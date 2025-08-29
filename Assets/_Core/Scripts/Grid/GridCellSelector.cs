using System;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// This script handles the selection and deselection of grid cells
/// through mouse input. It generates visual tiles that the player can
/// hover over and click.
/// </summary>
[RequireComponent(typeof(GridManager))]
public class GridCellSelector : MonoBehaviour
{
    // Events to allow other components to react without direct coupling.
    public static event Action<Vector3Int> OnCellSelected;
    public static event Action OnCellDeselected;

    // Fields
    [Header("Behavior")]
    [Tooltip("Enables or disables the selector's ability to respond to mouse input.")]
    [SerializeField] private bool _isEnabled = false;

    [Header("Visuals")]
    [Tooltip("The prefab used to create a visual representation of each grid cell.")]
    [SerializeField] private GameObject _gridTilePrefab;
    [Tooltip("The material to use when the mouse is hovering over a tile.")]
    [SerializeField] private Material _highlightMaterial;
    [Tooltip("The default material for unselected, unhovered tiles.")]
    [SerializeField] private Material _defaultMaterial;
    [Tooltip("The material to use for the currently selected tile.")]
    [SerializeField] private Material _selectedMaterial;

    private GridManager _gridManager;
    private GameObject _hoveredTileGameObject;
    private GameObject _selectedTileGameObject;

    // Public methods

    /// <summary>
    /// Sets the active state of the selector.
    /// </summary>
    public void SetSelectorState(bool state)
    {
        _isEnabled = state;
        if (!state)
        {
            clearHoverVisuals();
        }
    }

    /// <summary>
    /// Deselects the currently selected cell, resetting its visual state.
    /// This method can be called by other scripts to force deselection.
    /// </summary>
    public void DeselectCurrentCell()
    {
        if (_selectedTileGameObject != null)
        {
            // Reset the material of the previously selected tile.
            if (_selectedTileGameObject.TryGetComponent<Renderer>(out Renderer rend))
            {
                rend.material = _defaultMaterial;
            }
            _selectedTileGameObject = null;
            OnCellDeselected?.Invoke();
        }
    }

    // Lifecycle methods

    private void Start()
    {
        _gridManager = GetComponent<GridManager>();
        generateGridTiles();
    }

    private void Update()
    {
        if (!_isEnabled) return;

        handleHover();

        if (Input.GetMouseButtonDown(0))
        {
            handleSelection();
        }
    }

    // Private methods

    /// <summary>
    /// Generates the visual grid tiles for raycasting.
    /// The tiles' colliders are sized to perfectly match the grid cells.
    /// </summary>
    private void generateGridTiles()
    {
        var grid = _gridManager.Grid;

        // Ensure the grid tile prefab has a collider.
        if (_gridTilePrefab.GetComponent<Collider>() == null)
        {
            _gridTilePrefab.AddComponent<BoxCollider>();
        }

        for (int x = 0; x < grid.GridDimension.x; ++x)
        {
            for (int y = 0; y < grid.GridDimension.y; ++y)
            {
                // We are only generating tiles for the front layer (z=0).
                var cell = _gridManager.Grid.GetCell(x, y, 0);
                Vector3Int cellIndex = cell.Index;

                // Get the world position of the cell's center.
                Vector3 cellCenter = grid.GetCellCenter(cellIndex);

                // Instantiate the tile at the correct position.
                var tile = Instantiate(_gridTilePrefab, cellCenter, Quaternion.identity, transform);
                tile.name = $"GridTile_{cellIndex.x}_{cellIndex.y}_{cellIndex.z}";

                // Set the scale and collider size to match the grid's cell dimensions.
                // This ensures the raycast collider is a perfect fit for the grid cell.
                tile.transform.localScale = grid.CellDimension;
                if (tile.TryGetComponent<BoxCollider>(out BoxCollider boxCollider))
                {
                    boxCollider.size = Vector3.one; // Set the box collider size to 1, as the scale will handle the dimensions.
                    boxCollider.center = Vector3.zero; // Center the collider on the object's transform.
                }

                // Add GridTile component and set its index.
                GridTile gridTileComponent = tile.GetComponent<GridTile>();
                if (gridTileComponent == null)
                {
                    gridTileComponent = tile.AddComponent<GridTile>();
                }
                gridTileComponent.Index = cellIndex;

                // Store the default material.
                if (tile.TryGetComponent<Renderer>(out Renderer rend))
                {
                    rend.material = _defaultMaterial;
                }
            }
        }
    }

    /// <summary>
    /// Handles the visual feedback for hovering over grid tiles.
    /// </summary>
    private void handleHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        GridTile newHoveredTile = null;

        if (Physics.Raycast(ray, out hit))
        {
            // Check if the raycast hits a GridTile.
            if (hit.collider.TryGetComponent<GridTile>(out newHoveredTile))
            {
                // If we've moved to a new tile, update the visuals.
                if (_hoveredTileGameObject != newHoveredTile.gameObject)
                {
                    // Clear the previous hover effect if it's not the selected tile.
                    clearHoverVisuals();
                    // Apply a new hover effect.
                    if (newHoveredTile.gameObject != _selectedTileGameObject)
                    {
                        if (newHoveredTile.TryGetComponent<Renderer>(out Renderer rend))
                        {
                            rend.material = _highlightMaterial;
                        }
                    }
                    _hoveredTileGameObject = newHoveredTile.gameObject;
                }
            }
        }
        else
        {
            // Clear hover effect if the mouse is not over any tile.
            clearHoverVisuals();
            _hoveredTileGameObject = null;
        }
    }

    /// <summary>
    /// Resets the material of the previously hovered tile.
    /// </summary>
    private void clearHoverVisuals()
    {
        if (_hoveredTileGameObject != null && _hoveredTileGameObject != _selectedTileGameObject)
        {
            if (_hoveredTileGameObject.TryGetComponent<Renderer>(out Renderer rend))
            {
                rend.material = _defaultMaterial;
            }
        }
    }

    /// <summary>
    /// Handles the selection of a grid cell.
    /// </summary>
    private void handleSelection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.TryGetComponent<GridTile>(out GridTile selectedTile))
            {
                // If a new tile is selected, deselect the old one first.
                if (_selectedTileGameObject != selectedTile.gameObject)
                {
                    DeselectCurrentCell();

                    // Apply the new selection.
                    _selectedTileGameObject = selectedTile.gameObject;
                    if (_selectedTileGameObject.TryGetComponent<Renderer>(out Renderer rend))
                    {
                        rend.material = _selectedMaterial;
                    }

                    // Fire the event, passing the grid's index.
                    OnCellSelected?.Invoke(selectedTile.Index);
                }
                else
                {
                    // If the same tile is clicked again, deselect it.
                    DeselectCurrentCell();
                }
            }
        }
        else
        {
            // If the user clicks off the grid, deselect the current cell.
            DeselectCurrentCell();
        }
    }
}
