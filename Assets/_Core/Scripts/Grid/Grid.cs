using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Use this class to create grids of custom types in 3D.
/// </summary>
public class Grid<TType> : IEnumerable<Cell<TType>> where TType : class, new()
{
    // Fields

    private Vector3 _gridOrigin;
    private Vector3Int _gridDimension;
    private Vector3 _cellDimension;
    private int _totalCellCount;

    // The grid is now a 3D array.
    private Cell<TType>[,,] _cellArray3D;
    private Cell<TType>[] _cellArray1D;


    // Constructors

    public Grid()
        : this(Vector3.zero, 1, 1, 1, Vector3.one) { }

    public Grid(Vector3 origin, Vector3Int cellCount, Vector3 cellSize)
        : this(origin, cellCount.x, cellCount.y, cellCount.z, cellSize) { }

    public Grid(Vector3 origin, int xCellCount, int yCellCount, int zCellCount, Vector3 cellSize)
    {
        _gridOrigin = origin;
        _gridDimension = new Vector3Int(Mathf.Max(xCellCount, 1), Mathf.Max(yCellCount, 1), Mathf.Max(zCellCount, 1));
        _cellDimension = new Vector3(Mathf.Max(cellSize.x, 0.01f), Mathf.Max(cellSize.y, 0.01f), Mathf.Max(cellSize.z, 0.01f));
        _totalCellCount = GridDimension.x * GridDimension.y * GridDimension.z;
    }


    // Properties

    /// <summary>
    /// Returns the origin of the grid
    /// </summary>
    public Vector3 GridOrigin { get { return _gridOrigin; } set { _gridOrigin = value; } }

    /// <summary>
    /// Returns the dimension of the grid
    /// </summary>
    public Vector3Int GridDimension { get { return _gridDimension; } }

    /// <summary>
    /// Returns the dimension of a cell
    /// </summary>
    public Vector3 CellDimension { get { return _cellDimension; } }

    /// <summary>
    /// Returns the total number of cells
    /// </summary>
    public int TotalCellCount { get { return _totalCellCount; } }


    // Public methods

    /// <summary>
    /// Call this method before using the grid to initialize the cells.
    /// </summary>
    public void PrepareGrid()
    {
        _cellArray3D = new Cell<TType>[GridDimension.x, GridDimension.y, GridDimension.z];
        _cellArray1D = new Cell<TType>[TotalCellCount];

        for (int z = 0; z < GridDimension.z; z++)
        {
            for (int y = 0; y < GridDimension.y; y++)
            {
                for (int x = 0; x < GridDimension.x; x++)
                {
                    var cell = new Cell<TType>();
                    cell.Index = new Vector3Int(x, y, z);
                    cell.IsUsable = true;
                    cell.Entity = new TType();

                    _cellArray3D[x, y, z] = cell;
                    _cellArray1D[x + y * GridDimension.x + z * GridDimension.x * GridDimension.y] = cell;
                }
            }
        }
    }


    /// <summary>
    /// Call this method from OnDrawGizmos to draw a 3D wireframe grid.
    /// </summary>
    public void DrawGridGizmos(Color gridLineColor)
    {
        Gizmos.color = gridLineColor;

        Vector3 corner = GridOrigin + new Vector3(
            GridDimension.x * CellDimension.x,
            GridDimension.y * CellDimension.y,
            GridDimension.z * CellDimension.z
        );

        // Draw horizontal lines along X and Z
        for (int z = 0; z <= GridDimension.z; ++z)
        {
            for (int y = 0; y <= GridDimension.y; ++y)
            {
                Gizmos.DrawLine(
                    GridOrigin + new Vector3(0, y * CellDimension.y, z * CellDimension.z),
                    GridOrigin + new Vector3(GridDimension.x * CellDimension.x, y * CellDimension.y, z * CellDimension.z)
                );
            }
        }

        // Draw vertical lines along Y and Z
        for (int z = 0; z <= GridDimension.z; ++z)
        {
            for (int x = 0; x <= GridDimension.x; ++x)
            {
                Gizmos.DrawLine(
                    GridOrigin + new Vector3(x * CellDimension.x, 0, z * CellDimension.z),
                    GridOrigin + new Vector3(x * CellDimension.x, GridDimension.y * CellDimension.y, z * CellDimension.z)
                );
            }
        }

        // Draw lines along X and Y
        for (int y = 0; y <= GridDimension.y; ++y)
        {
            for (int x = 0; x <= GridDimension.x; ++x)
            {
                Gizmos.DrawLine(
                    GridOrigin + new Vector3(x * CellDimension.x, y * CellDimension.y, 0),
                    GridOrigin + new Vector3(x * CellDimension.x, y * CellDimension.y, GridDimension.z * CellDimension.z)
                );
            }
        }
    }



    /// <summary>
    /// Returns the cell itself in the grid
    /// </summary>
    public Cell<TType> GetCell(int x, int y, int z)
    {
        if (IsInside(x, y, z))
            return _cellArray3D[x, y, z];
        return null;
    }

    public Cell<TType> GetCell(Vector3Int index)
    {
        return GetCell(index.x, index.y, index.z);
    }

    public Cell<TType> GetCell(Vector3 worldPosition)
    {
        if (ConvertToXYZ(worldPosition, out int x, out int y, out int z))
            return _cellArray3D[x, y, z];
        return null;
    }

    /// <summary>
    /// Returns the 3D cell array itself
    /// </summary>
    public Cell<TType>[,,] GetCellArray3D()
    {
        return _cellArray3D;
    }

    /// <summary>
    /// Returns the 1D cell array itself
    /// </summary>
    public Cell<TType>[] GetCellArray1D()
    {
        return _cellArray1D;
    }

    /// <summary>
    /// Returns the data of a grid cell
    /// </summary>
    public TType GetCellContent(int x, int y, int z)
    {
        if (IsInside(x, y, z))
            return _cellArray3D[x, y, z].Entity;
        return null;
    }

    public TType GetCellContent(Vector3Int index)
    {
        return GetCellContent(index.x, index.y, index.z);
    }

    public TType GetCellContent(Vector3 worldPosition)
    {
        ConvertToXYZ(worldPosition, out int x, out int y, out int z);
        return GetCellContent(x, y, z);
    }


    /// <summary>
    /// Returns the cell validity
    /// </summary>
    public bool GetCellUsability(int x, int y, int z)
    {
        if (IsInside(x, y, z))
            return _cellArray3D[x, y, z].IsUsable;
        return false;
    }

    public bool GetCellUsability(Vector3Int index)
    {
        return GetCellUsability(index.x, index.y, index.z);
    }

    public bool GetCellUsability(Vector3 worldPosition)
    {
        if (ConvertToXYZ(worldPosition, out int x, out int y, out int z))
            return _cellArray3D[x, y, z].IsUsable;
        return false;
    }



    /// <summary>
    /// Returns true if x, y, and z are inside the grid
    /// </summary>
    public bool IsInside(int x, int y, int z)
    {
        if (x >= 0 && y >= 0 && z >= 0 && x < GridDimension.x && y < GridDimension.y && z < GridDimension.z)
            return true;
        return false;
    }

    /// <summary>
    /// Returns true if the point is inside the grid
    /// </summary>
    public bool IsInside(Vector3 worldPosition)
    {
        ConvertToXYZ(worldPosition, out int x, out int y, out int z);
        return IsInside(x, y, z);
    }

    /// <summary>
    /// Returns true if the index is inside the grid
    /// </summary>
    public bool IsInside(Vector3Int index)
    {
        return IsInside(index.x, index.y, index.z);
    }


    /// <summary>
    /// Converts world position into grid sections
    /// </summary>
    public bool ConvertToXYZ(Vector3 worldPosition, out int x, out int y, out int z)
    {
        bool ret = ConvertToXYZ(worldPosition, out Vector3Int index);
        x = index.x;
        y = index.y;
        z = index.z;
        return ret;
    }

    public bool ConvertToXYZ(Vector3 worldPosition, out Vector3Int index)
    {
        int x = Mathf.FloorToInt((worldPosition - GridOrigin).x / CellDimension.x);
        int y = Mathf.FloorToInt((worldPosition - GridOrigin).y / CellDimension.y);
        int z = Mathf.FloorToInt((worldPosition - GridOrigin).z / CellDimension.z);

        index = new Vector3Int(x, y, z);

        return IsInside(x, y, z);
    }


    /// <summary>
    /// Converts grid sections to world points
    /// </summary>
    public Vector3 ConvertToWorldPosition(int x, int y, int z)
    {
        return new Vector3(x * CellDimension.x, y * CellDimension.y, z * CellDimension.z) + GridOrigin;
    }

    public Vector3 ConvertToWorldPosition(Vector3Int index)
    {
        return new Vector3(index.x * CellDimension.x, index.y * CellDimension.y, index.z * CellDimension.z) + GridOrigin;
    }

    /// <summary>
    /// Returns the center of a cell in world space.
    /// </summary>
    public Vector3 GetCellCenter(int x, int y, int z)
    {
        if (IsInside(x, y, z))
            return (ConvertToWorldPosition(x, y, z) + new Vector3(CellDimension.x, CellDimension.y, CellDimension.z) / 2);
        return Vector3.positiveInfinity;
    }

    public Vector3 GetCellCenter(Vector3Int index)
    {
        return GetCellCenter(index.x, index.y, index.z);
    }

    public Vector3 GetCellCenter(Vector3 worldPosition)
    {
        ConvertToXYZ(worldPosition, out int x, out int y, out int z);
        return GetCellCenter(x, y, z);
    }


    /// <summary>
    /// Sets the value of the cell at target index
    /// </summary>
    public void SetCellContentAt(int x, int y, int z, TType data)
    {
        if (IsInside(x, y, z))
        {
            _cellArray3D[x, y, z].Entity = data;
        }
    }

    public void SetCellContentAt(Vector3Int index, TType data)
    {
        SetCellContentAt(index.x, index.y, index.z, data);
    }

    public void SetCellContentAt(Vector3 worldPosition, TType data)
    {
        if (ConvertToXYZ(worldPosition, out int x, out int y, out int z))
        {
            _cellArray3D[x, y, z].Entity = data;
        }
    }

    /// <summary>
    /// Set the cell validity at index
    /// </summary>
    public void SetCellUsabilityAt(int x, int y, int z, bool isValid)
    {
        if (IsInside(x, y, z))
        {
            _cellArray3D[x, y, z].IsUsable = isValid;
        }
    }

    public void SetCellUsabilityAt(Vector3Int index, bool isValid)
    {
        SetCellUsabilityAt(index.x, index.y, index.z, isValid);
    }

    public void SetCellUsabilityAt(Vector3 worldPosition, bool isValid)
    {
        if (ConvertToXYZ(worldPosition, out int x, out int y, out int z))
        {
            _cellArray3D[x, y, z].IsUsable = isValid;
        }
    }


    /// <summary>
    /// Overrided
    /// </summary>
    public override string ToString()
    {
        string retValue = "";
        for (int z = 0; z < GridDimension.z; ++z)
        {
            retValue += "Depth: " + z + "\n";
            for (int y = 0; y < GridDimension.y; ++y)
            {
                for (int x = 0; x < GridDimension.x; ++x)
                    retValue += _cellArray3D[x, y, z].ToString() + " ";
                retValue += "\n";
            }
            retValue += "\n";
        }
        return retValue;
    }


    // IEnumerable interface implemetation

    public IEnumerator<Cell<TType>> GetEnumerator()
    {
        foreach (var cell in _cellArray1D)
        {
            yield return cell;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}