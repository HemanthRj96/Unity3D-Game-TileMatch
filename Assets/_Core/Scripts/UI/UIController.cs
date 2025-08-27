using UnityEngine;

/// <summary>
/// Manages the state and visibility of the main UI panels.
/// Use this to switch between the dashboard and the store map view.
/// </summary>
public class UIController : MonoBehaviour
{
    // Fields
    [Header("UI Panels")]
    [SerializeField] private GameObject _dashboardUI;
    [SerializeField] private GameObject _storeMapUI;

    // Public methods

    /// <summary>
    /// Shows the Dashboard UI and hides all other panels.
    /// </summary>
    public void ShowDashboard()
    {
        _dashboardUI.SetActive(true);
        _storeMapUI.SetActive(false);
    }

    /// <summary>
    /// Shows the Store Map UI and hides all other panels.
    /// </summary>
    public void ShowStoreMap()
    {
        _dashboardUI.SetActive(false);
        _storeMapUI.SetActive(true);
    }
}
