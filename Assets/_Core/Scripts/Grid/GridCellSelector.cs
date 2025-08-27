using System;
using UnityEngine;



[RequireComponent(typeof(GridManager))]
public class GridCellSelector : MonoBehaviour
{
    // Events to allow other components to react without direct coupling.
    public static event Action<Vector3Int> OnCellSelected;
    public static event Action OnCellDeselected;

    // Fields

    [SerializeField] private bool _isEnabled = false;
    [SerializeField] private GameObject _gridTilePrefab;
    [SerializeField] private Material _highlightMaterial;
    [SerializeField] private Material _defaultMaterial;
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
            DeselectCurrentCell();
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
        if (_gridTilePrefab.GetComponent<Collider>() == null)
        {
            _gridTilePrefab.AddComponent<BoxCollider>();
        }
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
    /// </summary>
    private void generateGridTiles()
    {
        var grid = _gridManager.Grid;
        // The offset for the tiles is half the cell depth.
        Vector3 tileZOffset = Vector3.forward * (grid.CellDimension.z / 2);
        // The scale is slightly smaller than the cell to make grid lines visible.
        Vector3 scaleValue = grid.CellDimension * 0.98f;
        // Tiles should be thin, so we set the Z scale to 1.
        scaleValue.z = 1;

        for (int x = 0; x < grid.GridDimension.x; ++x)
        {
            for (int y = 0; y < grid.GridDimension.y; ++y)
            {
                var cell = _gridManager.Grid.GetCell(x, y, 0);

                var tile = Instantiate(_gridTilePrefab, grid.GetCellCenter(cell.Index) - tileZOffset, Quaternion.identity, transform);
                tile.name = $"GridTile_{cell.Index.x}_{cell.Index.y}_{cell.Index.z}";

                tile.transform.localScale = scaleValue;

                // Add GridTile component and set its index.
                GridTile gridTileComponent = tile.AddComponent<GridTile>();
                gridTileComponent.Index = cell.Index;

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
                    if (_hoveredTileGameObject != _selectedTileGameObject)
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
