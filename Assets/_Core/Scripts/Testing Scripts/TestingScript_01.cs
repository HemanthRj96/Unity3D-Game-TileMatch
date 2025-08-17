using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Data;


public class TestingScript_01 : MonoBehaviour
{
    public GridManager manager;
    public GridCellSelector handle;
    public SKUEntityFactory factory;
    public BrandType brandType;
    public int value = 0;

    SKUFilter someFilter;
    List<SKUEntity> filteredSKUs = new List<SKUEntity>();
    event Action onFixedUpdate = delegate { };


    private void Start()
    {
        someFilter = new SKUFilter();
        someFilter.AddCondition(sku => sku.GetComponent<BrandComponent>()?.brand == brandType);
        someFilter.AddCondition(sku => sku.GetComponent<GenericMatchComponent>()?.value == value);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (filteredSKUs.Count > 0)
            {
                foreach (var entity in filteredSKUs)
                {
                    if (entity != null && entity.HasComponent<UnityMetadataComponent>())
                    {
                        onFixedUpdate -= entity.GetComponent<UnityMetadataComponent>().AnimateIdle;
                        entity.GetComponent<UnityMetadataComponent>().ResetAnimation();
                    }
                }

                filteredSKUs.Clear();
            }

            filteredSKUs = someFilter.Filter(manager.ExtractAllEntitiesFromGrid());

            foreach (var entity in filteredSKUs)
            {
                if (entity != null && entity.HasComponent<UnityMetadataComponent>())
                    onFixedUpdate += entity.GetComponent<UnityMetadataComponent>().AnimateIdle;
            }
        }
    }

    private void FixedUpdate()
    {
        onFixedUpdate();
    }
}