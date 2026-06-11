using UnityEngine;
using System.Collections;

public class SceneSequence : MonoBehaviour
{
    [Header("Rocking Object (optional)")]
    public RockingCradle rockingObject;

    [Header("Camera Zoom Out")]
    public Camera targetCamera;
    public float zoomOutPercent = 50f;
    public float zoomDuration   = 5f;
    public float returnDuration = 10f;

    [Header("Scene Transition")]
    public string nextSceneName = "";

    [Header("Scene Entry — tick ON if this scene starts at night and should fade to day")]
    public bool fadeFromNightToDayOnStart = false;
    public float fadeOutDelay = 2f; // ← delay before fade out starts

    private CameraFollowActor cameraFollow;
    private Vector3    initialCameraPosition;
    private Quaternion initialCameraRotation;
    private float      initialFOVOrSize;

    void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        cameraFollow = targetCamera.GetComponent<CameraFollowActor>();
        if (cameraFollow == null)
            Debug.LogWarning("CameraFollowActor not found on camera!");

        initialCameraPosition = targetCamera.transform.position;
        initialCameraRotation = targetCamera.transform.rotation;
        initialFOVOrSize      = targetCamera.orthographic
            ? targetCamera.orthographicSize
            : targetCamera.fieldOfView;

        // If this scene starts at night, fade to day automatically
        if (fadeFromNightToDayOnStart)
        {
            if (DayNightCycle.Instance != null)
                DayNightCycle.Instance.FadeToDay();
            else
                Debug.LogWarning("DayNightCycle instance not found.");

            if (ScreenFader.Instance != null)
                StartCoroutine(DelayedFadeOut());
            else
                Debug.LogWarning("ScreenFader instance not found.");
        }
    }

    public void OnArrivedAtTarget()
    {
        // Trigger rocking object — completely optional
        if (rockingObject != null)
            rockingObject.StartRocking();

        // Trigger camera sequence
        if (targetCamera != null)
            StartCoroutine(CameraSequence());
        else
            Debug.LogWarning("No camera found!");

        // Start day/night and transition after camera finishes
        if (DayNightCycle.Instance != null)
        {
            if (string.IsNullOrEmpty(nextSceneName))
                DayNightCycle.Instance.FadeToDark();
            else
                StartCoroutine(FadeAndLoad(nextSceneName));
        }
        else
            Debug.LogWarning("DayNightCycle instance not found.");
    }

    IEnumerator CameraSequence()
    {
        // Step 1 — disable follow script so we take full control
        if (cameraFollow != null)
            cameraFollow.enabled = false;

        // Step 2 — zoom out
        float startValue     = targetCamera.orthographic
            ? targetCamera.orthographicSize
            : targetCamera.fieldOfView;

        float zoomedOutValue = startValue * (1f + zoomOutPercent / 100f);

        float elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / zoomDuration));

            if (targetCamera.orthographic)
                targetCamera.orthographicSize = Mathf.Lerp(startValue, zoomedOutValue, t);
            else
                targetCamera.fieldOfView = Mathf.Lerp(startValue, zoomedOutValue, t);

            yield return null;
        }

        // Step 3 — hold zoomed out for a moment
        yield return new WaitForSeconds(1f);

        // Step 4 — return to initial position, rotation and FOV/size
        elapsed = 0f;
        Vector3    fromPosition = targetCamera.transform.position;
        Quaternion fromRotation = targetCamera.transform.rotation;
        float      fromValue    = targetCamera.orthographic
            ? targetCamera.orthographicSize
            : targetCamera.fieldOfView;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / returnDuration));

            targetCamera.transform.position = Vector3.Lerp(fromPosition, initialCameraPosition, t);
            targetCamera.transform.rotation = Quaternion.Lerp(fromRotation, initialCameraRotation, t);

            if (targetCamera.orthographic)
                targetCamera.orthographicSize = Mathf.Lerp(fromValue, initialFOVOrSize, t);
            else
                targetCamera.fieldOfView = Mathf.Lerp(fromValue, initialFOVOrSize, t);

            yield return null;
        }

        // Lock to exact final values
        targetCamera.transform.position = initialCameraPosition;
        targetCamera.transform.rotation = initialCameraRotation;

        if (targetCamera.orthographic)
            targetCamera.orthographicSize = initialFOVOrSize;
        else
            targetCamera.fieldOfView = initialFOVOrSize;
    }

    IEnumerator FadeAndLoad(string sceneName)
    {
        // Wait for camera sequence to fully finish first
        float totalCameraDuration = zoomDuration + 1f + returnDuration;
        yield return new WaitForSeconds(totalCameraDuration);

        // Fade light to dark first
        DayNightCycle.Instance.FadeToDark();

        // Wait for day/night fade to fully finish
        yield return new WaitUntil(() => !DayNightCycle.Instance.IsRunning);

        // Start loading scene in background BEFORE fading to black
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // Fade screen to black while scene loads in background
        if (ScreenFader.Instance != null)
            ScreenFader.Instance.FadeIn();

        // Wait for BOTH black fade and scene load to finish
        yield return new WaitUntil(() =>
            (!ScreenFader.Instance.IsFading) &&
            asyncLoad.progress >= 0.9f);

        // Hold in black just briefly
        yield return new WaitForSeconds(DayNightCycle.Instance.sceneTransitionHoldDuration);

        // Activate scene — already loaded, switches instantly
        asyncLoad.allowSceneActivation = true;
    }

    IEnumerator DelayedFadeOut()
    {
        yield return new WaitForSeconds(fadeOutDelay); // ← adjust this
        ScreenFader.Instance.FadeOut();
    }
    
}