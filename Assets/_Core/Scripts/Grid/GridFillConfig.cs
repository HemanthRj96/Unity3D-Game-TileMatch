using UnityEngine;
using System.Collections.Generic;


/// <summary>
/// This ScriptableObject holds all the configurable data for the GridFillHelper,
/// allowing for multiple, swappable presets for grid arrangements.
/// </summary>
[CreateAssetMenu(fileName = "Grid Fill Config", menuName = "SWB/Grid/Grid Fill Config")]
public class GridFillConfig : ScriptableObject
{
    // Nested class to make the SKU and count visible in the Inspector
    [System.Serializable]
    public class SkuCount
    {
        public string skuId;
        public int count;
    }

    // Enum for a more explicit filling direction
    public enum FrontLayerFillDirection
    {
        Row,
        Column
    }

    /// <summary>
    /// Defines how the back layers of the grid should be populated.
    /// </summary>
    public enum BackLayerFillMode
    {
        Mirror,
        Placeholder
    }

    [Header("Front Layer Configuration")]
    public List<SkuCount> FrontLayerSkus;
    public FrontLayerFillDirection FillDirection;

    [Header("Back Layer Configuration")]
    public BackLayerFillMode FillMode;
    public List<string> PlaceholderSkuIds;

    /// <summary>
    /// Returns a random SKU ID from the PlaceholderSkuIds list.
    /// </summary>
    public string GetRandomPlaceholderSkuId()
    {
        if (PlaceholderSkuIds == null || PlaceholderSkuIds.Count == 0)
        {
            return null;
        }

        int randomIndex = Random.Range(0, PlaceholderSkuIds.Count);
        return PlaceholderSkuIds[randomIndex];
    }
}
