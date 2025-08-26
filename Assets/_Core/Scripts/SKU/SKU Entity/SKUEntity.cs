using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;


/// <summary>
/// The core data model for a Stock Keeping Unit (SKU). This is a simple data container
/// that uses a component-based approach to store various properties of an SKU.
/// It is decoupled from any Unity MonoBehaviour and can be easily serialized.
/// </summary>
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
