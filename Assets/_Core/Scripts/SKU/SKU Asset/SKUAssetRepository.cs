using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// This is the main data bank that stores all the information about SKUs and serves as a static database for any script
/// This list can be populated manually or automatically using CSV files
/// </summary>
[CreateAssetMenu(fileName = "SKU Asset Repository", menuName = "Create SKU Asset Repository")]
public class SKUAssetRepository : ScriptableObject
{
    // Fields

    [Header("List of all assets statically generated using CSV loader")]
    [SerializeField]
    private List<SKUAssetContainer> _assets = new List<SKUAssetContainer>();
    private Dictionary<string, SKUAssetContainer> _assetLookup;


    // Public methods

    public void Initialize()
    {
        if (_assets.Count > 0)
            _assetLookup = _assets.ToDictionary(asset => asset.skuID, asset => asset);
    }

    public SKUAssetContainer GetAsset(string skuId)
    {
        if (_assetLookup == null || _assetLookup.Count == 0)
            Initialize();

        if (_assetLookup != null && _assetLookup.TryGetValue(skuId, out SKUAssetContainer asset)) 
            return asset;

        Debug.LogWarning($"SKU with ID '{skuId}' not found in repository.");
        return null;
    }

#if UNITY_EDITOR
    [ContextMenu("Load SKU Assets from CSV")]
    public void LoadFromCSV()
    {
        // Use a simple prompt to get the file path from the user.
        string filePath = EditorUtility.OpenFilePanel("Select SKU CSV File", "", "csv");
        if (string.IsNullOrEmpty(filePath))
        {
            Debug.Log("CSV file load canceled.");
            return;
        }

        // Load the assets directly from the selected file path.
        List<SKUAssetContainer> loadedAssets = SKUAssetLoader.LoadSKUAssetsFromCSV(filePath);
        if (loadedAssets.Count > 0)
        {
            _assets.Clear();
            _assets.AddRange(loadedAssets);
            EditorUtility.SetDirty(this); // Mark the object as dirty to save changes.
            AssetDatabase.SaveAssets();
            Debug.Log($"Successfully loaded {_assets.Count} SKU assets from CSV.");
        }
    }
#endif
}


[System.Serializable]
public class SKUAssetContainer
{
    public string displayName;
    public string skuID;
    public GameObject prefab;
    public Sprite icon;

    public BrandComponent brandComponent;
    public CategoryComponent categoryComponent;
    public PackagingInfoComponent packagingInfoComponent;
    public TemperatureInfoComponent temperatureInfoComponent;
    public ZoneComponent zoneComponent;
    public EntityTypeComponent entityTypeComponent;
    public GenericMatchComponent genericMatchComponent;
}