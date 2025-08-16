using System;
using UnityEngine;


/// <summary>
/// This is the core interaction system that can be used to select a content inside the grid, the functionality has to be customized
/// for better usability
/// </summary>
[RequireComponent(typeof(GridManager))]
public class GridCellSelector : MonoBehaviour
{
    // Fields
    
    [SerializeField] private GridSelectionMode _selectionMode = GridSelectionMode.OnButtonDown;
    [SerializeField] private bool _isEnabled = false;
    [SerializeField] private GameObject _gridTilePrefab;
    [SerializeField] private Material _highlightMaterial;
    [SerializeField] private Material _defaultMaterial;
    [SerializeField] private Material _selectedMaterial;

    private GridManager _interactionGrid;
    private Cell<SKUEntity> _previousCellSelection;
    private Cell<SKUEntity> _currentCellSelection;
    private GameObject _highlightedTile;
    private GameObject _selectedTile;
    private bool _selectedOnDown = false; // New boolean to track the selection state


    // Properties
    
    public event Action OnCellSelectionEvent = delegate { };


    // Public Methods
    
    public void SetSelectorState(bool state) => _isEnabled = state;
    public Cell<SKUEntity> GetCurrentCellSelection() => _currentCellSelection;
    public Cell<SKUEntity> GetPreviousCellSelection() => _previousCellSelection;


    // Private Methods
    
    private bool trySelectCell(out Cell<SKUEntity> selectedCell)
    {
        selectedCell = null;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GridTile gridTile = hit.collider.GetComponent<GridTile>();
            if (gridTile != null)
            {
                selectedCell = _interactionGrid.Grid.GetCell(gridTile.Index);
                return selectedCell != null;
            }
        }
        return false;
    }

    private void onCellSelection(Cell<SKUEntity> cell)
    {
        if (cell == null) return;
        _previousCellSelection = _currentCellSelection;
        _currentCellSelection = cell;
        OnCellSelectionEvent?.Invoke();
    }

    private void handleHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject hitObject = hit.collider.gameObject;
            if (hitObject != _highlightedTile)
            {
                // Reset previous highlight
                if (_highlightedTile != null && _highlightedTile != _selectedTile)
                {
                    Renderer rend = _highlightedTile.GetComponent<Renderer>();
                    if (rend != null) rend.material = _defaultMaterial;
                }

                // Apply new highlight only if it's not the selected tile
                if (hitObject != _selectedTile)
                {
                    Renderer hitRenderer = hitObject.GetComponent<Renderer>();
                    if (hitRenderer != null && hitObject.GetComponent<GridTile>() != null)
                    {
                        hitRenderer.material = _highlightMaterial;
                        _highlightedTile = hitObject;
                    }
                }
            }
        }
        else
        {
            // Reset highlight if not hovering over any tile and it's not the selected tile
            if (_highlightedTile != null && _highlightedTile != _selectedTile)
            {
                Renderer rend = _highlightedTile.GetComponent<Renderer>();
                if (rend != null) rend.material = _defaultMaterial;
            }
            _highlightedTile = null;
        }
    }

    private void generateGridTiles()
    {
        var grid = _interactionGrid.Grid;
        Vector3 tileZOffset = Vector3.forward * (grid.CellDimension.z / 2);
        Vector3 scaleValue = grid.CellDimension * 0.98f;
        scaleValue.z = 1;

        foreach (var cell in grid)
        {
            var tile = Instantiate(_gridTilePrefab, grid.GetCellCenter(cell.Index) - tileZOffset, Quaternion.identity);
            tile.transform.localScale = scaleValue;
            tile.transform.parent = transform;

            // Add GridTile component and set its index
            GridTile gridTileComponent = tile.AddComponent<GridTile>();
            gridTileComponent.Index = cell.Index;

            // Store the default material
            Renderer rend = tile.GetComponent<Renderer>();
            if (rend != null) rend.material = _defaultMaterial;
        }
    }


    // Lifecycle methods
    
    private void Start()
    {
        _interactionGrid = GetComponent<GridManager>();
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

        // Handle mouse down
        if (Input.GetMouseButtonDown(0))
        {
            Cell<SKUEntity> selectedCell;
            if (trySelectCell(out selectedCell))
            {
                onCellSelection(selectedCell);

                // Set the visual state to selected
                if (_highlightedTile != null)
                {
                    Renderer rend = _highlightedTile.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        rend.material = _selectedMaterial;
                    }
                    _selectedTile = _highlightedTile;
                    _selectedOnDown = true;
                }
            }
        }

        // Handle mouse up
        if (Input.GetMouseButtonUp(0) && _selectedOnDown)
        {
            if (_selectedTile != null)
            {
                Renderer rend = _selectedTile.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material = _defaultMaterial;
                }
                _selectedTile = null;
            }
            _selectedOnDown = false;
        }
    }


    // Nested Types

    public enum GridSelectionMode
    {
        Continous,
        OnButtonUp,
        OnButtonDown
    }
}