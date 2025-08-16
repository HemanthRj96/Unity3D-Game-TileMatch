using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public class TestingScript_01 : MonoBehaviour
{
    public GridManager manager;
    public GridCellSelector handle;
    public SKUEntityFactory factory;
    public string skuID;


    private void Awake()
    {
        handle.OnCellSelectionEvent += Handle_OnCellSelectionEvent;
    }

    private void OnDestroy()
    {
        handle.OnCellSelectionEvent -= Handle_OnCellSelectionEvent;
    }

    private void Handle_OnCellSelectionEvent()
    {
        var cell = handle.GetCurrentCellSelection();

        if (cell == null) return;


        Debug.Log($"{cell.Index}");
    }
}