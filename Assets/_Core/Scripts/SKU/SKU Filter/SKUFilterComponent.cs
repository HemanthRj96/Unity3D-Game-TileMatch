using UnityEngine;
using System.Collections.Generic;
using System.Linq.Expressions;
using TMPro;
using System;


public class SKUFilterComponent : MonoBehaviour
{
    [SerializeField] private GridManager _gridManager;

    Action fixedUpdate = delegate { };
    int index;

    public void SubmitAnswer()
    {
        
        CompositeSKUFilter filter;
        switch (index)
        {
            case 1:
                filter = (CompositeSKUFilter)SKUFilterFactory.CreateMultipleBrandFilter(BrandType.Coke, BrandType.Fanta, BrandType.Sprite, BrandType.SmartWater);
                break;
            case 2:
                filter = (CompositeSKUFilter)SKUFilterFactory.CreateMultipleBrandFilter(BrandType.Sprite, BrandType.SmartWater);
                break;
            case 3:
                filter = (CompositeSKUFilter)SKUFilterFactory.CreateMultipleBrandFilter(BrandType.SmartWater);
                break;
            default:
                filter = (CompositeSKUFilter)SKUFilterFactory.CreateMultipleBrandFilter(BrandType.SmartWater);
                break;
        }

        var frontFacing = _gridManager.GetFrontFacingEntities();

        foreach (var ff in frontFacing)
        {
            ff.GetComponent<UnityMetadataComponent>().ResetAnimation();
        }

        fixedUpdate = delegate { };
        var entities = filter.Filter(_gridManager.GetFrontFacingEntities());
        foreach (var e in entities)
        {
            fixedUpdate += e.GetComponent<UnityMetadataComponent>().AnimateIdle;
        }
    }

    public void QuestionIndex(int value)
    {
        index = value;
    }

    private void FixedUpdate()
    {
        fixedUpdate();
    }
}
