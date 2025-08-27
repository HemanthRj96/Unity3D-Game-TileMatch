using UnityEngine;
using System.Collections.Generic;
using static GridFillConfig;


/// <summary>
/// A helper component to fill a 3D grid with SKUs according to a configurable set of rules.
/// This script uses a ScriptableObject for its configuration, allowing for multiple presets.
/// </summary>
[RequireComponent(typeof(GridManager))]
public class GridFillHelper : MonoBehaviour
{
    // Fields

    [Header("Dependencies")]
    [SerializeField] private GridFillConfig _fillConfig;


    private GridManager _gridManager;


    // Public methods

    /// <summary>
    /// Fills the grid with equipment based on the specified rules in the config file.
    /// This method is designed to be called from a button or another script.
    /// </summary>
    [ContextMenu("Fill Equipment Grid")]
    public void FillEquipmentGrid()
    {
        if (_gridManager == null)
        {
            Debug.LogError("GridManager reference is missing. Cannot fill the grid.");
            return;
        }

        if (_fillConfig == null)
        {
            Debug.LogError("GridFillConfig reference is missing. Please assign a ScriptableObject configuration.");
            return;
        }

        // 1. Clear the entire grid first.
        _gridManager.ClearGrid();

        // 2. Fill the front layer (z=0) using the provided SKU IDs and counts.
        fillFrontLayer();

        // 3. Fill the back layers (z > 0) based on the selected mode.
        fillBackLayers();
    }


    // Lifecycle methods

    private void Awake()
    {
        _gridManager = GetComponent<GridManager>();
    }


    // Private methods

    /// <summary>
    /// Fills the front-facing plane of the grid (z=0) with the configured SKUs.
    /// The placement order is determined by the fill direction.
    /// </summary>
    private void fillFrontLayer()
    {
        if (_fillConfig.FrontLayerSkus == null || _fillConfig.FrontLayerSkus.Count == 0)
        {
            Debug.LogWarning("No SKU IDs provided for the front layer. Skipping front layer filling.");
            return;
        }

        int skuIndex = 0;
        int skuCount = 0;
        var grid = _gridManager.Grid;

        for (int y = 0; y < grid.GridDimension.y; y++)
        {
            for (int x = 0; x < grid.GridDimension.x; x++)
            {
                // Check if we've run out of SKUs to place
                if (skuIndex >= _fillConfig.FrontLayerSkus.Count)
                {
                    return;
                }

                // Get the current Sku and its remaining count
                SkuCount currentSku = _fillConfig.FrontLayerSkus[skuIndex];
                if (skuCount >= currentSku.count)
                {
                    skuIndex++;
                    if (skuIndex >= _fillConfig.FrontLayerSkus.Count)
                    {
                        return; // No more SKUs to place.
                    }
                    currentSku = _fillConfig.FrontLayerSkus[skuIndex];
                    skuCount = 0; // Reset the count for the next SKU.
                }

                _gridManager.SpawnEntityAt(currentSku.skuId, new Vector3Int(x, y, 0));
                skuCount++;
            }
        }
    }

    /// <summary>
    /// Fills the back layers (z > 0) according to the chosen fill mode.
    /// </summary>
    private void fillBackLayers()
    {
        var grid = _gridManager.Grid;

        for (int z = 1; z < grid.GridDimension.z; z++)
        {
            for (int x = 0; x < grid.GridDimension.x; x++)
            {
                for (int y = 0; y < grid.GridDimension.y; y++)
                {
                    Vector3Int frontLayerIndex = new Vector3Int(x, y, 0);
                    Vector3Int backLayerIndex = new Vector3Int(x, y, z);

                    if (_fillConfig.FillMode == BackLayerFillMode.Mirror)
                    {
                        SKUEntity frontEntity = grid.GetCellContent(frontLayerIndex);
                        if (frontEntity != null && frontEntity.HasComponent<IdentificationComponent>())
                        {
                            string skuId = frontEntity.GetComponent<IdentificationComponent>().skuID;
                            _gridManager.SpawnEntityAt(skuId, backLayerIndex);
                        }
                    }
                    else if (_fillConfig.FillMode == BackLayerFillMode.Placeholder)
                    {
                        string placeholderSkuId = _fillConfig.GetRandomPlaceholderSkuId();
                        if (!string.IsNullOrEmpty(placeholderSkuId))
                        {
                            _gridManager.SpawnEntityAt(placeholderSkuId, backLayerIndex);
                        }
                        else
                        {
                            Debug.LogWarning("Placeholder SKU ID list is empty. Skipping placeholder filling.");
                            return;
                        }
                    }
                }
            }
        }
    }
}
