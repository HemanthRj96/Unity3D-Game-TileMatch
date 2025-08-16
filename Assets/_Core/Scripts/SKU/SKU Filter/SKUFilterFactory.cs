using UnityEngine;

public static class SKUFilterFactory
{
    public static ISKUFilter CreateBrandFilter(BrandType brand)
    {
        SKUFilter filter = new SKUFilter();
        filter.AddCondition(sku => sku.GetComponent<BrandComponent>()?.brand == brand);

        return filter;
    }

    public static ISKUFilter CreateColdCompatibleFilter()
    {
        return new SKUFilter().AddCondition
        (
            sku => sku.GetComponent<TemperatureInfoComponent>()?.isColdCompatible ?? false
        );
    }

    public static ISKUFilter CreateGenericColorFilter(Color[] color)
    {
        SKUFilter colorFilter = new SKUFilter();

        foreach (Color c in color)
            colorFilter.AddCondition(sku => sku.GetComponent<GenericMatchComponent>()?.color == c);

        return colorFilter;
    }

    public static ISKUFilter CreateGenericIDFilter(int[] ids)
    {
        SKUFilter colorFilter = new SKUFilter();

        foreach (int id in ids)
            colorFilter.AddCondition(sku => sku.GetComponent<GenericMatchComponent>()?.value == id);

        return colorFilter;
    }
}