using UnityEngine;


/// <summary>
/// A factory for creating various types of SKU filters.
/// </summary>
public static class SKUFilterFactory
{
    // Brand Filters
    public static ISKUFilter CreateBrandFilter(BrandType brand)
    {
        return new SimpleSKUFilter(sku => sku.GetComponent<BrandComponent>()?.brand == brand);
    }

    public static ISKUFilter CreateMultipleBrandFilter(params BrandType[] brands)
    {
        var orFilter = new CompositeSKUFilter(LogicalOperator.OR);
        foreach (var brand in brands)
        {
            orFilter.AddFilter(CreateBrandFilter(brand));
        }
        return orFilter;
    }

    // Category Filters
    public static ISKUFilter CreateCategoryFilter(CategoryType category)
    {
        return new SimpleSKUFilter(sku => sku.GetComponent<CategoryComponent>()?.category == category);
    }

    public static ISKUFilter CreateMultipleCategoryFilter(params CategoryType[] categories)
    {
        var orFilter = new CompositeSKUFilter(LogicalOperator.OR);
        foreach (var category in categories)
        {
            orFilter.AddFilter(CreateCategoryFilter(category));
        }
        return orFilter;
    }

    // Packaging Filters
    public static ISKUFilter CreatePackagingSizeFilter(PackagingSize size)
    {
        return new SimpleSKUFilter(sku => sku.GetComponent<PackagingInfoComponent>()?.size == size);
    }

    public static ISKUFilter CreateContainerTypeFilter(ContainerType containerType)
    {
        return new SimpleSKUFilter(sku => sku.GetComponent<PackagingInfoComponent>()?.containerType == containerType);
    }

    public static ISKUFilter CreateIsSingleServeFilter(bool isSingleServe)
    {
        return new SimpleSKUFilter(sku => sku.GetComponent<PackagingInfoComponent>()?.isSingleServe == isSingleServe);
    }

    // Temperature Filters
    public static ISKUFilter CreateTemperatureTypeFilter(TemperatureType type)
    {
        return new SimpleSKUFilter(sku => sku.GetComponent<TemperatureInfoComponent>()?.type == type);
    }

    public static ISKUFilter CreateColdCompatibleFilter()
    {
        return new SimpleSKUFilter(sku => sku.GetComponent<TemperatureInfoComponent>()?.isColdCompatible ?? false);
    }

    // Zone Filters
    public static ISKUFilter CreateZoneFilter(ZoneType zone)
    {
        return new SimpleSKUFilter(sku => sku.GetComponent<ZoneComponent>()?.zoneType == zone);
    }

    // Entity Type Filters
    public static ISKUFilter CreateEntityTypeFilter(EntityType entityType)
    {
        return new SimpleSKUFilter(sku => sku.GetComponent<EntityTypeComponent>()?.entityType == entityType);
    }

    // Generic Match Filters
    public static ISKUFilter CreateGenericColorFilter(params Color[] colors)
    {
        var orFilter = new CompositeSKUFilter(LogicalOperator.OR);
        foreach (Color c in colors)
        {
            orFilter.AddFilter(new SimpleSKUFilter(sku => sku.GetComponent<GenericMatchComponent>()?.color == c));
        }
        return orFilter;
    }

    public static ISKUFilter CreateGenericIDFilter(params int[] ids)
    {
        var orFilter = new CompositeSKUFilter(LogicalOperator.OR);
        foreach (int id in ids)
        {
            orFilter.AddFilter(new SimpleSKUFilter(sku => sku.GetComponent<GenericMatchComponent>()?.value == id));
        }
        return orFilter;
    }

    // Composite Filter
    public static ISKUFilter CreateCompositeFilter(LogicalOperator op, params ISKUFilter[] filters)
    {
        var compositeFilter = new CompositeSKUFilter(op);
        foreach (var filter in filters)
        {
            compositeFilter.AddFilter(filter);
        }
        return compositeFilter;
    }
}