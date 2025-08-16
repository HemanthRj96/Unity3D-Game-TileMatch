using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;


/// <summary>
/// This class is used by the Asset Repository to load SKUs from CSV file, useful for Editor SKU creation
/// </summary>
public static class SKUAssetLoader
{
    public static List<SKUAssetContainer> LoadSKUAssetsFromCSV(string filePath)
    {
        List<SKUAssetContainer> assets = new List<SKUAssetContainer>();

        try
        {
            // Read all lines from the file using the provided absolute path.
            string[] lines = File.ReadAllLines(filePath);

            if (lines.Length > 0)
            {
                // Parse each line and create a SKUAssetContainer for it.
                // We skip the first row (index 0) as we assume it is a header.
                foreach (string line in lines.Skip(1))
                {
                    string[] dataRow = line.Split(',');
                    SKUAssetContainer container = CreateSKUAssetContainerFromRow(dataRow);
                    if (container != null)
                    {
                        assets.Add(container);
                    }
                }
            }
        }
        catch (FileNotFoundException)
        {
            Debug.LogError($"SKUAssetLoader: The file was not found at the specified path: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"SKUAssetLoader: An error occurred while loading the CSV file. Error: {e.Message}");
        }
        return assets;
    }

    private static SKUAssetContainer CreateSKUAssetContainerFromRow(string[] dataRow)
    {
        if (dataRow.Length < 10)
        {
            Debug.LogError("SKUAssetLoader: Row does not contain enough data. Skipping.");
            return null;
        }

        SKUAssetContainer container = new SKUAssetContainer();

        try
        {
            container.displayName = dataRow[1].Trim();
            container.skuID = dataRow[0].Trim();
            container.brandComponent = new BrandComponent { brand = (BrandType)Enum.Parse(typeof(BrandType), dataRow[2].Trim(), true) };
            container.categoryComponent = new CategoryComponent { category = (CategoryType)Enum.Parse(typeof(CategoryType), dataRow[3].Trim(), true) };

            // Corrected parsing for PackagingSize to match the enum format.
            container.packagingInfoComponent = new PackagingInfoComponent
            {
                size = (PackagingSize)Enum.Parse(typeof(PackagingSize), dataRow[4].Trim(), true),
                isSingleServe = bool.Parse(dataRow[5].Trim()),
                containerType = (ContainerType)Enum.Parse(typeof(ContainerType), dataRow[6].Trim(), true)
            };

            container.temperatureInfoComponent = new TemperatureInfoComponent
            {
                type = (TemperatureType)Enum.Parse(typeof(TemperatureType), dataRow[7].Trim(), true),
                isColdCompatible = (dataRow[7].Trim() == "Cold" || dataRow[7].Trim() == "Both")
            };
            container.zoneComponent = new ZoneComponent { zoneType = (ZoneType)Enum.Parse(typeof(ZoneType), dataRow[8].Trim().Replace(" ", "_"), true) };
            container.entityTypeComponent = new EntityTypeComponent { entityType = (EntityType)Enum.Parse(typeof(EntityType), dataRow[9].Trim(), true) };
        }
        catch (Exception e)
        {
            Debug.LogError($"SKUAssetLoader: Failed to parse data row. Row data: [{string.Join(", ", dataRow)}]. Error: {e.Message}");
            return null;
        }
        return container;
    }
}