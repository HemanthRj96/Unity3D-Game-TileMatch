using System;
using System.Collections.Generic;
using System.Linq;


public interface ISKUFilter
{
    List<SKUEntity> Filter(List<SKUEntity> allSkus);
}

public class SKUFilter : ISKUFilter
{
    private readonly List<Func<SKUEntity, bool>> _conditions = new List<Func<SKUEntity, bool>>();

    public SKUFilter AddCondition(Func<SKUEntity, bool> condition)
    {
        _conditions.Add(condition);
        return this;
    }

    public List<SKUEntity> Filter(List<SKUEntity> allSkus)
    {
        if (allSkus == null || !allSkus.Any())
        {
            return new List<SKUEntity>();
        }

        return allSkus.Where(sku => _conditions.All(condition => condition(sku))).ToList();
    }
}