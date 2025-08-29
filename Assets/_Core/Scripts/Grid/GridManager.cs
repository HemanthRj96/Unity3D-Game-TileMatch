using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Random = UnityEngine.Random;


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
    [SerializeField] private SKUEntityFactory _skuFactory;

    // The core data structure for the grid.
    public Grid<SKUEntity> Grid { get; private set; }

    // Events to broadcast changes, allowing other components to subscribe.
    public static event Action<Vector3Int, SKUEntity> OnEntitySpawned;
    public static event Action<Vector3Int> OnEntityDespawned;
    public static event Action OnGridCleared;

    /// <summary>
    /// Defines the type of pattern to spawn.
    /// </summary>
    public enum PatternType
    {
        Row,
        Column,
        Depth,
        Chunk,
        Random
    }

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
        if (Grid.GetCellUsability(gridIndex))
        {
            Debug.LogWarning($"No entity found at index {gridIndex}. Nothing to despawn.");
            return false;
        }

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
    /// Spawns a complex pattern of SKUs based on the specified pattern type.
    /// This method can spawn rows, columns, depth slices, or chunks of SKUs.
    /// </summary>
    /// <param name="patternType">The type of pattern to spawn (Row, Column, Depth, Chunk, Random).</param>
    /// <param name="skuId">The SKU ID to spawn. Required for non-random patterns.</param>
    /// <param name="startIndex">The starting index for the pattern (used for Row, Column, Depth, and Chunk).</param>
    /// <param name="endIndex">The ending index for a Chunk pattern.</param>
    /// <param name="clearFirst">If true, clears the entire grid before spawning the new pattern.</param>
    public void SpawnComplexPattern(PatternType patternType, string skuId = null, Vector3Int startIndex = default, Vector3Int endIndex = default, bool clearFirst = true)
    {
        if (clearFirst)
        {
            ClearGrid();
        }

        if (patternType != PatternType.Random && string.IsNullOrEmpty(skuId))
        {
            Debug.LogError("SKU ID is required for non-random patterns.");
            return;
        }

        switch (patternType)
        {
            case PatternType.Row:
                _spawnRow(skuId, startIndex.y);
                break;
            case PatternType.Column:
                _spawnColumn(skuId, startIndex.x);
                break;
            case PatternType.Depth:
                _spawnDepthSlice(skuId, startIndex.z);
                break;
            case PatternType.Chunk:
                _spawnChunk(skuId, startIndex, endIndex);
                break;
            case PatternType.Random:
                _spawnRandom();
                break;
        }
    }


    public SKUEntity GetEntityAt(Vector3Int index)
    {
        return Grid.GetCellContent(index);
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

        // Dynamically resize and set the gap arrays.
        ResizeAndSetGaps();

        Grid.SetGaps(_gridConfig.rowGaps, _gridConfig.columnGaps, _gridConfig.depthGaps);
        Grid.PrepareGrid();
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;
        if (_gridConfig.shouldDrawGizmos == false) return;

        // Re-instantiate the grid and prepare it for gizmo drawing.
        Grid = new Grid<SKUEntity>(_gridConfig.gridOffset + transform.position, _gridConfig.gridDimension, _gridConfig.cellDimension);

        // Dynamically resize and set the gap arrays for gizmo drawing.
        ResizeAndSetGaps();

        Grid.SetGaps(_gridConfig.rowGaps, _gridConfig.columnGaps, _gridConfig.depthGaps);
        Grid.DrawGridGizmos(_gridConfig.gridLineColor);
    }

    /// <summary>
    /// Ensures the gap arrays match the grid dimensions and resizes them if necessary.
    /// This keeps the inspector consistent.
    /// </summary>
    private void ResizeAndSetGaps()
    {
        if (_gridConfig.rowGaps == null || _gridConfig.rowGaps.Length != _gridConfig.gridDimension.x)
        {
            _gridConfig.rowGaps = new float[_gridConfig.gridDimension.x];
        }

        if (_gridConfig.columnGaps == null || _gridConfig.columnGaps.Length != _gridConfig.gridDimension.y)
        {
            _gridConfig.columnGaps = new float[_gridConfig.gridDimension.y];
        }

        if (_gridConfig.depthGaps == null || _gridConfig.depthGaps.Length != _gridConfig.gridDimension.z)
        {
            _gridConfig.depthGaps = new float[_gridConfig.gridDimension.z];
        }
    }


    // Private helper methods for spawning patterns

    /// <summary>
    /// Spawns a full row of a given SKU.
    /// </summary>
    private void _spawnRow(string skuId, int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= Grid.GridDimension.y)
        {
            Debug.LogWarning($"Invalid row index: {rowIndex}. Must be between 0 and {Grid.GridDimension.y - 1}.");
            return;
        }

        for (int x = 0; x < Grid.GridDimension.x; x++)
        {
            for (int z = 0; z < Grid.GridDimension.z; z++)
            {
                SpawnEntityAt(skuId, new Vector3Int(x, rowIndex, z));
            }
        }
    }

    /// <summary>
    /// Spawns a full column of a given SKU.
    /// </summary>
    private void _spawnColumn(string skuId, int columnIndex)
    {
        if (columnIndex < 0 || columnIndex >= Grid.GridDimension.x)
        {
            Debug.LogWarning($"Invalid column index: {columnIndex}. Must be between 0 and {Grid.GridDimension.x - 1}.");
            return;
        }

        for (int y = 0; y < Grid.GridDimension.y; y++)
        {
            for (int z = 0; z < Grid.GridDimension.z; z++)
            {
                SpawnEntityAt(skuId, new Vector3Int(columnIndex, y, z));
            }
        }
    }

    /// <summary>
    /// Spawns a full depth slice of a given SKU.
    /// </summary>
    private void _spawnDepthSlice(string skuId, int depthIndex)
    {
        if (depthIndex < 0 || depthIndex >= Grid.GridDimension.z)
        {
            Debug.LogWarning($"Invalid depth index: {depthIndex}. Must be between 0 and {Grid.GridDimension.z - 1}.");
            return;
        }

        for (int x = 0; x < Grid.GridDimension.x; x++)
        {
            for (int y = 0; y < Grid.GridDimension.y; y++)
            {
                SpawnEntityAt(skuId, new Vector3Int(x, y, depthIndex));
            }
        }
    }

    /// <summary>
    /// Spawns a rectangular chunk of a given SKU.
    /// </summary>
    private void _spawnChunk(string skuId, Vector3Int start, Vector3Int end)
    {
        int startX = Mathf.Min(start.x, end.x);
        int endX = Mathf.Max(start.x, end.x);
        int startY = Mathf.Min(start.y, end.y);
        int endY = Mathf.Max(start.y, end.y);
        int startZ = Mathf.Min(start.z, end.z);
        int endZ = Mathf.Max(start.z, end.z);

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                for (int z = startZ; z <= endZ; z++)
                {
                    SpawnEntityAt(skuId, new Vector3Int(x, y, z));
                }
            }
        }
    }

    /// <summary>
    /// Spawns a random SKU in every cell of the grid.
    /// </summary>
    private void _spawnRandom()
    {
        for (int x = 0; x < Grid.GridDimension.x; x++)
        {
            for (int y = 0; y < Grid.GridDimension.y; y++)
            {
                for (int z = 0; z < Grid.GridDimension.z; z++)
                {
                    string randomSkuId = GetRandomSKUID();
                    SpawnEntityAt(randomSkuId, new Vector3Int(x, y, z));
                }
            }
        }
    }

    /// <summary>
    /// Helper method to get a random SKU ID from the repository.
    /// </summary>
    private string GetRandomSKUID()
    {
        var allAssets = _skuRepository.GetAllSKUAssets();
        if (allAssets.Count == 0)
        {
            Debug.LogError("No SKU assets found to spawn randomly.");
            return null;
        }
        int randomIndex = Random.Range(0, allAssets.Count);
        return allAssets[randomIndex].skuID;
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

        // New fields for gap configuration
        [Header("Gaps")]
        public float[] rowGaps;
        public float[] columnGaps;
        public float[] depthGaps;
    }
}
