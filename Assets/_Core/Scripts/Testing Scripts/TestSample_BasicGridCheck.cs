using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Data;


public class TestSample_BasicGridCheck : MonoBehaviour
{
    public GridManager manager;
    public GridCellSelector handle;
    //public string skuID;
    public float time = 1.0f;
    public int loopCount = 10;

    string skuID => UnityEngine.Random.Range(1, 7).ToString();



    private IEnumerator Start()
    {
        Vector3Int startIndex = Vector3Int.zero;
        Vector3Int endIndex = manager.Grid.GridDimension - Vector3Int.one;

        for (int l = 0; l < loopCount; l++)
        {
            Debug.Log($"Loop {l}, Step 1");
            manager.SpawnEntityAt(skuID, startIndex);
            manager.SpawnEntityAt(skuID, endIndex);
            yield return new WaitForSeconds(time);


            Debug.Log($"Loop {l}, Step 2");
            for (int y = 0; y <= endIndex.x; ++y)
            {
                manager.SpawnComplexPattern(GridManager.PatternType.Row, skuID, new Vector3Int(0, y, 0));
                yield return new WaitForSeconds(time);
            }
            yield return new WaitForSeconds(time);


            Debug.Log($"Loop {l}, Step 3");
            for (int x = 0; x <= endIndex.y; ++x)
            {
                manager.SpawnComplexPattern(GridManager.PatternType.Column, skuID, new Vector3Int(x, 0, 0));
                yield return new WaitForSeconds(time);
            }
            yield return new WaitForSeconds(time);


            Debug.Log($"Loop {l}, Step 4");
            for (int z = 0; z <= endIndex.z; ++z)
            {
                manager.SpawnComplexPattern(GridManager.PatternType.Depth, skuID, new Vector3Int(0, 0, z));
                yield return new WaitForSeconds(time);
            }
            yield return new WaitForSeconds(time);

            Debug.Log($"Loop {l}, Step 5");
            manager.SpawnComplexPattern(GridManager.PatternType.Chunk, skuID, startIndex + Vector3Int.one, endIndex - Vector3Int.one);
            yield return new WaitForSeconds(time);

            Debug.Log($"Loop {l}, Step 6");
            for (int i = 0; i < 100; i++)
            {
                manager.SpawnComplexPattern(GridManager.PatternType.Random);
                yield return new WaitForSeconds(time * 0.25f);
            }
            yield return null;
        }
    }
}