using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;


public class InventoryUI : MonoBehaviour
{
    // Events

    public static event Action<SKUAssetContainer> OnSKUSelected;

    // Fields

    [Header("UI References")]
    [SerializeField] private GameObject _inventoryPanel;
    [SerializeField] private Transform _contentParent;
    [SerializeField] private GameObject _inventoryButtonPrefab;
    [SerializeField] private SKUAssetRepository _skuRepository;


    // Public methods

    public void TogglePanel(bool shouldOpen)
    {
        _inventoryPanel.SetActive(shouldOpen);
    }

    public void PopulateInventoryUI()
    {
        // Clear old buttons

        foreach (Transform child in _contentParent)
        {
            Destroy(child.gameObject);
        }

        List<SKUAssetContainer> allAssets = _skuRepository.GetAllSKUAssets();
        foreach (SKUAssetContainer asset in allAssets)
        {
            GameObject buttonInstance = Instantiate(_inventoryButtonPrefab, _contentParent);
            buttonInstance.GetComponentInChildren<TextMeshProUGUI>().text = asset.displayName;
            Button button = buttonInstance.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                OnSKUSelected?.Invoke(asset);
            });
        }
    }
}