using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;



public class SKUEntity
{
    private readonly Dictionary<Type, ISKUComponent> _components = new Dictionary<Type, ISKUComponent>();

    public void AddComponent(ISKUComponent component)
    {
        _components[component.GetType()] = component;
    }

    public T GetComponent<T>() where T : class, ISKUComponent
    {
        if (_components.TryGetValue(typeof(T), out ISKUComponent component))
        {
            return component as T;
        }
        return null;
    }

    public bool HasComponent<T>() where T : class, ISKUComponent
    {
        return _components.ContainsKey(typeof(T));
    }
}