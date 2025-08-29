using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionManager : MonoBehaviour
{
    [Tooltip("The camera transform to be moved.")]
    [SerializeField] private Transform mainCamera;
    [SerializeField] private GodViewCameraController cameraController;
    [SerializeField] private Transform cameraAnchor;
    [SerializeField] private GameObject focusUI;
    [SerializeField] private GameObject isoUI;


    [Tooltip("The CanvasGroup of the black screen UI element to control its alpha.")]
    [SerializeField] private CanvasGroup blackScreenCanvasGroup;

    Vector3 prevPosition;
    Quaternion prevRotation;
    Camera cam;

    public void GotoIsometric()
    {
        StartTransition(prevPosition, prevRotation, 0.01f, 0.5f, false);
    }

    public void GotoFocused()
    {
        prevPosition = cam.transform.position;
        prevRotation = cam.transform.rotation;
        StartTransition(cameraAnchor.position, cameraAnchor.rotation, 2f, 0.5f, true);
    }

    private void Start()
    {
        cam = Camera.main;
    }



    private void Awake()
    {
        // Ensure the black screen starts transparent.
        if (blackScreenCanvasGroup != null)
        {
            blackScreenCanvasGroup.alpha = 0;
            blackScreenCanvasGroup.interactable = false;
            blackScreenCanvasGroup.blocksRaycasts = false;
        }
    }



    public void StartTransition(Vector3 endPosition, Quaternion endRotation, float delay, float transitionDuration, bool shouldGoToFocus)
    {
        if (mainCamera == null || blackScreenCanvasGroup == null)
        {
            Debug.LogError("Camera or Black Screen CanvasGroup is not assigned. Transition failed.");
            return;
        }

        StartCoroutine(TransitionCoroutine(endPosition, endRotation, delay, transitionDuration, shouldGoToFocus));
    }

    private IEnumerator TransitionCoroutine(Vector3 endPosition, Quaternion endRotation, float delay, float transitionDuration, bool shouldGoToFocus)
    {
        yield return new WaitForSeconds(delay);
        cameraController.enabled = false;

        float elapsedTime = 0f;
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            blackScreenCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / transitionDuration);
            yield return null;
        }
        blackScreenCanvasGroup.alpha = 1; // Ensure it's fully opaque.

        mainCamera.position = endPosition; // Ensure it reaches the exact target.
        mainCamera.rotation = endRotation;
        yield return null;
        if (!shouldGoToFocus)
        {
            cameraController.enabled = true;
            cameraController.ExitFocusMode();
        }


            elapsedTime = 0f;
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            blackScreenCanvasGroup.alpha = 1 - Mathf.Clamp01(elapsedTime / transitionDuration);
            yield return null;
        }
        blackScreenCanvasGroup.alpha = 0; // Ensure it's fully transparent.

        focusUI.SetActive(shouldGoToFocus);
        isoUI.SetActive(!shouldGoToFocus);
    }
}

