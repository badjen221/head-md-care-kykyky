using UnityEngine;
using System.Collections;

public class SceneSequence : MonoBehaviour
{
    public RockingCradle rockingCradle;

    [Header("Camera Zoom Out")]
    public Camera targetCamera;
    public float zoomOutPercent = 30f;    // 30 = zoom out by 30%, 50 = zoom out by 50%
    public float zoomDuration   = 2f;

    void Start()
    {
        if (rockingCradle == null)
        {
            rockingCradle = FindObjectOfType<RockingCradle>();
            if (rockingCradle == null)
                Debug.LogWarning("RockingCradle not found in scene!");
        }

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    public void OnArrivedAtTarget()
    {
        // Trigger day/night effect
        if (DayNightCycle.Instance != null)
            DayNightCycle.Instance.PlayDayNightEffect();
        else
            Debug.LogWarning("DayNightCycle instance not found.");

        // Trigger cradle rocking
        if (rockingCradle != null)
            rockingCradle.StartRocking();
        else
            Debug.LogWarning("RockingCradle reference is missing on SceneSequence!");

        // Trigger camera zoom out
        if (targetCamera != null)
            StartCoroutine(ZoomOut());
        else
            Debug.LogWarning("No camera found for zoom out!");
    }

    IEnumerator ZoomOut()
    {
        float elapsed = 0f;

        float startValue = targetCamera.orthographic
            ? targetCamera.orthographicSize
            : targetCamera.fieldOfView;

        // zoom out = increase the value by the percentage
        float targetValue = startValue * (1f + zoomOutPercent / 100f);

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / zoomDuration));

            if (targetCamera.orthographic)
                targetCamera.orthographicSize = Mathf.Lerp(startValue, targetValue, t);
            else
                targetCamera.fieldOfView = Mathf.Lerp(startValue, targetValue, t);

            yield return null;
        }

        if (targetCamera.orthographic)
            targetCamera.orthographicSize = targetValue;
        else
            targetCamera.fieldOfView = targetValue;
    }
}