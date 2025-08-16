using UnityEngine;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Class that primarly manages grid, used to load and unload entities, get handle to the grid itself and get all SKUs inside a grid
/// </summary>
public class GridManager : MonoBehaviour
{
    // Fields 

    [Header("Grid Settings Configuration")]
    [SerializeField] private GridConfig _gridConfig;


    // Properties

    public Grid<SKUEntity> Grid { get; private set; }


    // Public Methods

    public List<SKUEntity> ExtractAllEntitiesFromGrid() => Grid.Select(cell => cell.Entity).Where(sku => sku != null).ToList();

    public void LoadEntityToGridAt(SKUEntity entity, Vector3Int gridIndex)
    {
        Grid.SetCellContentAt(gridIndex, entity);
        Grid.SetCellUsabilityAt(gridIndex, true);
    }

    public void UnloadEntityFromGridAt(Vector3Int gridIndex)
    {
        Grid.SetCellContentAt(gridIndex, null);
        Grid.SetCellUsabilityAt(gridIndex, false);
    }


    // Lifecycle methods

    private void Awake()
    {
        // Grid preparation
        Grid = new Grid<SKUEntity>(_gridConfig.gridOffset + transform.position, _gridConfig.gridDimension, _gridConfig.cellDimension);
        Grid.PrepareGrid();
    }

    // No changes to these
    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;

        if (_gridConfig.shouldDrawGizmos == false) return;

        Grid = new Grid<SKUEntity>(_gridConfig.gridOffset + transform.position, _gridConfig.gridDimension, _gridConfig.cellDimension);
        Grid.DrawGridGizmos(_gridConfig.gridLineColor);
    }


    // Nested types

    [System.Serializable]
    private class GridConfig
    {
        // Changed to Vector3Int for a 3D grid.
        [SerializeField]
        public Vector3Int gridDimension = new Vector3Int();
        // Changed to Vector3 for a 3D grid.
        [SerializeField]
        public Vector3 cellDimension = new Vector3();
        // Changed to Vector3 for a 3D grid.
        [SerializeField]
        public Vector3 gridOffset = new Vector3();
        // Gizmos and editor things
        [SerializeField]
        public bool shouldDrawGizmos = false;
        [SerializeField]
        public Color gridLineColor = Color.green;
    }
}