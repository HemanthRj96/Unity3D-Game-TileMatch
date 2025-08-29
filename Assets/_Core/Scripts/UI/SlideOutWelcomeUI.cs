using UnityEngine;
using UnityEngine.UI;
using System.Collections;



public class SlideOutWelcomeUI : MonoBehaviour
{
    // The UI panel's RectTransform to be animated.
    // It's recommended to attach this script to the panel itself.
    [Tooltip("The RectTransform of the UI panel to be animated.")]
    public RectTransform uiPanelRectTransform;

    // The button that triggers the slide-out animation.
    [Tooltip("The button that triggers the slide-out animation.")]
    public Button hideButton;

    // Duration of the slide-out animation in seconds.
    [Tooltip("The duration of the animation in seconds.")]
    public float animationDuration = 0.5f;

    // A flag to prevent multiple animations from starting simultaneously.
    private bool isAnimating = false;

    // The initial position of the panel.
    private Vector3 initialPosition;

    private void Awake()
    {
        // Get a reference to the UI panel's RectTransform if it's not set.
        if (uiPanelRectTransform == null)
        {
            uiPanelRectTransform = GetComponent<RectTransform>();
        }

        // Store the initial position to be able to reset the panel if needed.
        initialPosition = uiPanelRectTransform.localPosition;

        // Ensure the hideButton is assigned and set up the event listener.
        if (hideButton != null)
        {
            // Add a listener to the button's click event.
            hideButton.onClick.AddListener(StartSlideOutAnimation);
        }
        else
        {
            Debug.LogError("Hide Button is not assigned on " + gameObject.name);
        }
    }

    /// <summary>
    /// This method is called by the button's onClick event. It starts the coroutine for the animation.
    /// </summary>
    public void StartSlideOutAnimation()
    {
        // Only start the animation if one is not already in progress.
        if (!isAnimating)
        {
            StartCoroutine(SlideOutCoroutine());
        }
    }

    /// <summary>
    /// Coroutine to smoothly slide the UI panel off the screen.
    /// </summary>
    private IEnumerator SlideOutCoroutine()
    {
        isAnimating = true;
        hideButton.gameObject.SetActive(false);
        // Calculate the target position. We'll move it upwards, off-screen.
        // The target position is based on the panel's height to ensure it's fully hidden.
        Vector3 startPosition = uiPanelRectTransform.localPosition;
        Vector3 targetPosition = startPosition + new Vector3(0, uiPanelRectTransform.rect.height, 0);

        float elapsedTime = 0f;

        // Use a while loop to interpolate the position over time.
        while (elapsedTime < animationDuration)
        {
            // Calculate the current position based on the elapsed time and duration.
            float t = elapsedTime / animationDuration;
            // Use Lerp to create a smooth transition.
            uiPanelRectTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);

            // Increment the elapsed time by the time since the last frame.
            elapsedTime += Time.deltaTime;

            // Yield control back to Unity, so it can render the next frame.
            yield return null;
        }

        // Ensure the panel ends up exactly at the target position.
        uiPanelRectTransform.localPosition = targetPosition;

        // Optionally, you can deactivate the GameObject after the animation is complete.
        // gameObject.SetActive(false);
        
        isAnimating = false;
    }
}
