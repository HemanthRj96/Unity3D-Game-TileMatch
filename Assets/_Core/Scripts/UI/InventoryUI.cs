using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Runtime.CompilerServices;

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

    [Header("Camera Movement")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private float moveDistanceX = 5f;
    [SerializeField] private float moveDuration = 0.5f;

    // Private fields
    private Vector3 originalCamPos;
    private Coroutine moveCoroutine;

    void Start()
    {
        // Store the camera's original position at the start
        if (_mainCamera != null)
        {
            originalCamPos = _mainCamera.transform.position;
        }
    }

    // Public methods
    public void TogglePanel(bool shouldOpen)
    {
        _inventoryPanel.SetActive(shouldOpen);
        //MoveCamera(shouldOpen);
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

    // Private methods for camera movement
    private void MoveCamera(bool shouldMoveOut)
    {
        // Stop any currently running movement
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        Vector3 targetPos;
        if (shouldMoveOut)
        {
            // Move the camera out
            targetPos = originalCamPos + Camera.main.transform.right * moveDistanceX;
        }
        else
        {
            // Move the camera back to the original position
            targetPos = originalCamPos;
        }

        // Start the new movement coroutine
        moveCoroutine = StartCoroutine(MoveCameraCoroutine(targetPos));
    }

    private IEnumerator MoveCameraCoroutine(Vector3 targetPos)
    {
        Vector3 startPos = _mainCamera.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;
            _mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Snap to the final position to avoid inaccuracies
        _mainCamera.transform.position = targetPos;
    }
}