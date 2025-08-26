using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Core component for managing the 3D grid, entities, and their manipulation.
/// This class now acts as the central hub for all grid-related operations.
/// </summary>
public class GridManager : MonoBehaviour
{
    // Fields

    [Header("Grid Settings Configuration")]
    [SerializeField] private GridConfig _gridConfig;

    [Header("Data & Factories")]
    [SerializeField] private SKUAssetRepository _skuRepository;
    // Assuming SKUAssetFactory is a typo and should be SKUEntityFactory
    [SerializeField] private SKUEntityFactory _skuFactory;

    // The core data structure for the grid.
    public Grid<SKUEntity> Grid { get; private set; }

    // Events to broadcast changes, allowing other components to subscribe.
    public static event Action<Vector3Int, SKUEntity> OnEntitySpawned;
    public static event Action<Vector3Int> OnEntityDespawned;
    public static event Action OnGridCleared;


    // Public Methods

    /// <summary>
    /// Spawns an SKU entity at a specific grid index using its SKU ID.
    /// Returns true if successful, false otherwise.
    /// </summary>
    /// <param name="skuId">The unique ID of the SKU to spawn.</param>
    /// <param name="gridIndex">The target grid index (Vector3Int).</param>
    public bool SpawnEntityAt(string skuId, Vector3Int gridIndex)
    {
        // Check if the cell is usable before attempting to spawn.
        if (!Grid.GetCellUsability(gridIndex))
        {
            Debug.LogWarning($"Cell at index {gridIndex} is not usable. Cannot spawn entity.");
            return false;
        }

        // Get the asset container from the central repository.
        SKUAssetContainer assetContainer = _skuRepository.GetAsset(skuId);
        if (assetContainer == null) return false;

        // Use the factory to create a new SKUEntity object and its GameObject.
        SKUEntity newSkuEntity = _skuFactory.CreateSKUEntityGameObject(skuId, transform);
        GameObject skuGameObject = newSkuEntity.GetComponent<UnityMetadataComponent>().skuGameObject;

        // Position the new GameObject correctly within the grid cell.
        skuGameObject.transform.position = Grid.GetCellCenter(gridIndex) -
                                          (Vector3.up * (Grid.CellDimension.y / 2));

        // Load the new entity into the grid's internal data structure.
        Grid.SetCellContentAt(gridIndex, newSkuEntity);
        Grid.SetCellUsabilityAt(gridIndex, false); // Mark the cell as unusable once an item is placed.

        // Broadcast the event so other systems can react.
        OnEntitySpawned?.Invoke(gridIndex, newSkuEntity);

        return true;
    }


    /// <summary>
    /// Despawns the SKU entity at the specified grid index.
    /// Returns true if an entity was successfully despawned, false otherwise.
    /// </summary>
    /// <param name="gridIndex">The index of the cell to clear.</param>
    public bool DespawnEntityAt(Vector3Int gridIndex)
    {
        SKUEntity entity = Grid.GetCellContent(gridIndex);
        if (entity == null)
        {
            Debug.LogWarning($"No entity found at index {gridIndex}. Nothing to despawn.");
            return false;
        }

        // Destroy the GameObject associated with the SKU.
        Destroy(entity.GetComponent<UnityMetadataComponent>().skuGameObject);

        // Clear the entity from the grid's internal data.
        Grid.SetCellContentAt(gridIndex, null);
        Grid.SetCellUsabilityAt(gridIndex, true); // Make the cell usable again.

        // Broadcast the event.
        OnEntityDespawned?.Invoke(gridIndex);

        return true;
    }


    /// <summary>
    /// Clears the entire grid, removing all entities.
    /// </summary>
    public void ClearGrid()
    {
        foreach (var cell in Grid)
        {
            if (cell.Entity != null)
            {
                DespawnEntityAt(cell.Index);
            }
        }
        OnGridCleared?.Invoke();
    }


    /// <summary>
    /// Spawns a series of entities in a bulk pattern, which is great for loading levels.
    /// </summary>
    /// <param name="pattern">A dictionary where the key is the SKU ID and the value is a list of grid indices.</param>
    public void SpawnPattern(Dictionary<string, List<Vector3Int>> pattern)
    {
        foreach (var entry in pattern)
        {
            string skuId = entry.Key;
            foreach (var index in entry.Value)
            {
                SpawnEntityAt(skuId, index);
            }
        }
    }


    /// <summary>
    /// Get all SKUs in a specific row with an optional count limit.
    /// </summary>
    /// <param name="rowIndex">The row index (Y coordinate).</param>
    /// <param name="count">The number of entities to retrieve from the front (Z-axis). Defaults to the full depth.</param>
    public List<SKUEntity> GetEntitiesInRow(int rowIndex, int count = -1)
    {
        List<SKUEntity> rowEntities = new List<SKUEntity>();
        int maxCountZ = ((count == -1) || (count > Grid.GridDimension.z)) ? Grid.GridDimension.z : count;

        for (int x = 0; x < Grid.GridDimension.x; x++)
        {
            for (int z = 0; z < maxCountZ; z++)
            {
                SKUEntity entity = Grid.GetCellContent(x, rowIndex, z);
                if (entity != null)
                {
                    rowEntities.Add(entity);
                }
            }
        }
        return rowEntities;
    }


    /// <summary>
    /// Get all SKUs in a specific column with an optional count limit.
    /// </summary>
    /// <param name="columnIndex">The column index (X coordinate).</param>
    /// <param name="count">The number of entities to retrieve from the front (Z-axis). Defaults to the full depth.</param>
    public List<SKUEntity> GetEntitiesInColumn(int columnIndex, int count = -1)
    {
        List<SKUEntity> colEntities = new List<SKUEntity>();
        int maxCountZ = ((count == -1) || (count > Grid.GridDimension.z)) ? Grid.GridDimension.z : count;

        for (int y = 0; y < Grid.GridDimension.y; y++)
        {
            for (int z = 0; z < maxCountZ; z++)
            {
                SKUEntity entity = Grid.GetCellContent(columnIndex, y, z);
                if (entity != null)
                {
                    colEntities.Add(entity);
                }
            }
        }
        return colEntities;
    }

    /// <summary>
    /// Get all SKUs in a specific depth layer with an optional count limit.
    /// </summary>
    /// <param name="depthIndex">The depth index (Z coordinate).</param>
    /// <param name="count">The number of entities to retrieve from the front (X-axis). Defaults to the full width.</param>
    public List<SKUEntity> GetEntitiesInDepth(int depthIndex, int count = -1)
    {
        List<SKUEntity> depthEntities = new List<SKUEntity>();
        int maxCountX = ((count == -1) || (count > Grid.GridDimension.x)) ? Grid.GridDimension.x : count;

        for (int x = 0; x < maxCountX; x++)
        {
            for (int y = 0; y < Grid.GridDimension.y; y++)
            {
                SKUEntity entity = Grid.GetCellContent(x, y, depthIndex);
                if (entity != null)
                {
                    depthEntities.Add(entity);
                }
            }
        }
        return depthEntities;
    }

    /// <summary>
    /// Gets all entities on the front-facing plane of the grid (where z = 0).
    /// </summary>
    public List<SKUEntity> GetFrontFacingEntities()
    {
        List<SKUEntity> frontEntities = new List<SKUEntity>();
        for (int x = 0; x < Grid.GridDimension.x; x++)
        {
            for (int y = 0; y < Grid.GridDimension.y; y++)
            {
                SKUEntity entity = Grid.GetCellContent(x, y, 0);
                if (entity != null)
                {
                    frontEntities.Add(entity);
                }
            }
        }
        return frontEntities;
    }

    /// <summary>
    /// Gets all entities from the back sections of the grid (where z > 0).
    /// </summary>
    public List<SKUEntity> GetBackSectionEntities()
    {
        List<SKUEntity> backEntities = new List<SKUEntity>();
        for (int z = 1; z < Grid.GridDimension.z; z++)
        {
            for (int x = 0; x < Grid.GridDimension.x; x++)
            {
                for (int y = 0; y < Grid.GridDimension.y; y++)
                {
                    SKUEntity entity = Grid.GetCellContent(x, y, z);
                    if (entity != null)
                    {
                        backEntities.Add(entity);
                    }
                }
            }
        }
        return backEntities;
    }


    // Lifecycle methods

    private void Awake()
    {
        // Grid preparation
        Grid = new Grid<SKUEntity>(_gridConfig.gridOffset + transform.position, _gridConfig.gridDimension, _gridConfig.cellDimension);
        Grid.PrepareGrid();
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;
        if (_gridConfig.shouldDrawGizmos == false) return;

        // We re-instantiate the grid here to allow for editor gizmo drawing without play mode.
        Grid = new Grid<SKUEntity>(_gridConfig.gridOffset + transform.position, _gridConfig.gridDimension, _gridConfig.cellDimension);
        Grid.DrawGridGizmos(_gridConfig.gridLineColor);
    }


    // Nested types

    [System.Serializable]
    private class GridConfig
    {
        [SerializeField]
        public Vector3Int gridDimension = new Vector3Int();
        [SerializeField]
        public Vector3 cellDimension = new Vector3();
        [SerializeField]
        public Vector3 gridOffset = new Vector3();
        [SerializeField]
        public bool shouldDrawGizmos = false;
        [SerializeField]
        public Color gridLineColor = Color.green;
    }
}
