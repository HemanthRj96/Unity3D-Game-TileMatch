using System;
using System.Collections.Generic;
using System.Linq;



// Enum for logical operators
public enum LogicalOperator
{
    AND,
    OR
}

/// <summary>
/// The core interface for all SKU filters.
/// </summary>
public interface ISKUFilter
{
    List<SKUEntity> Filter(List<SKUEntity> allSkus);
}

/// <summary>
/// A simple filter class that applies a single condition.
/// </summary>
public class SimpleSKUFilter : ISKUFilter
{
    private readonly Func<SKUEntity, bool> _condition;

    public SimpleSKUFilter(Func<SKUEntity, bool> condition)
    {
        _condition = condition ?? throw new ArgumentNullException(nameof(condition));
    }

    public List<SKUEntity> Filter(List<SKUEntity> allSkus)
    {
        if (allSkus == null || !allSkus.Any())
        {
            return new List<SKUEntity>();
        }

        return allSkus.Where(sku => _condition(sku)).ToList();
    }
}

/// <summary>
/// A composite filter that combines multiple filters using a logical operator.
/// This allows for complex filtering logic like A AND B OR C.
/// </summary>
public class CompositeSKUFilter : ISKUFilter
{
    private readonly List<ISKUFilter> _filters = new List<ISKUFilter>();
    private readonly LogicalOperator _operator;

    public CompositeSKUFilter(LogicalOperator op)
    {
        _operator = op;
    }

    public CompositeSKUFilter AddFilter(ISKUFilter filter)
    {
        _filters.Add(filter);
        return this;
    }

    public List<SKUEntity> Filter(List<SKUEntity> allSkus)
    {
        if (allSkus == null || !allSkus.Any())
        {
            return new List<SKUEntity>();
        }

        // Apply all filters and then combine based on the operator.
        List<List<SKUEntity>> filteredLists = _filters.Select(filter => filter.Filter(allSkus)).ToList();

        if (_operator == LogicalOperator.AND)
        {
            // For AND, find the intersection of all lists.
            return filteredLists.Skip(1).Aggregate(filteredLists.FirstOrDefault(), (current, list) => current.Intersect(list).ToList());
        }
        else // LogicalOperator.OR
        {
            // For OR, find the union of all lists.
            return filteredLists.SelectMany(x => x).Distinct().ToList();
        }
    }
}