using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A generic cell class to be used within the grid.
/// Updated to handle a 3D index.
/// </summary>
public class Cell<TType> where TType : class
{
    // Fields

    private bool _isUsable;
    private TType _content;
    // Changed to Vector3Int to support 3D grids.
    private Vector3Int _index;


    // Properties

    /// <summary>
    /// Returns true if this cell is usable/ is valid
    /// </summary>
    public bool IsUsable
    {
        get
        {
            return _isUsable;
        }
        set
        {
            _isUsable = value;
        }
    }

    /// <summary>
    /// Returns the data stored in this cell
    /// </summary>
    public TType Entity
    {
        get
        {
            return _content;
        }
        set
        {
            if (_isUsable)
                _content = value;
        }
    }

    /// <summary>
    /// Returns the index of this cell as a Vector3Int.
    /// </summary>
    public Vector3Int Index
    {
        get
        {
            return _index;
        }
        set
        {
            _index = value;
        }
    }


    // Public methods

    /// <summary>
    /// Override method to format the values inside the cell
    /// </summary>
    public override string ToString() => $"Cell - [{Index}] / [{Entity}] / [{IsUsable}]";

    /// <summary>
    /// Returns true if the cell data and index are the same
    /// </summary>
    /// <param name="anotherCell">Cell you want to compare against</param>
    public bool IsSame(Cell<TType> anotherCell)
    {
        return Entity.Equals(anotherCell.Entity) && Index == anotherCell.Index;
    }
}