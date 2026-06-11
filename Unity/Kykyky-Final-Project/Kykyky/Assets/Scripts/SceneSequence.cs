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
        }
    }

    public void OnArrivedAtTarget()
    {
        if (DayNightCycle.Instance != null)
        {
            if (string.IsNullOrEmpty(nextSceneName))
                DayNightCycle.Instance.FadeToDark();
            else
                StartCoroutine(FadeAndLoad(nextSceneName));
        }
        else
            Debug.LogWarning("DayNightCycle instance not found.");

        // Trigger rocking object — completely optional
        if (rockingObject != null)
            rockingObject.StartRocking();

        // Trigger camera sequence
        if (targetCamera != null)
            StartCoroutine(CameraSequence());
        else
            Debug.LogWarning("No camera found!");
    }

    // Waits for fade to complete then loads the scene
    /*IEnumerator FadeAndLoad(string sceneName)
    {
        DayNightCycle.Instance.FadeToDark();
        yield return new WaitUntil(() => !DayNightCycle.Instance.IsRunning);
        DayNightCycle.Instance.LoadScene(sceneName);
    }*/
    IEnumerator FadeAndLoad(string sceneName)
    {
        // Fade light to dark AND fade screen to black simultaneously
        DayNightCycle.Instance.FadeToDark();

        if (ScreenFader.Instance != null)
            ScreenFader.Instance.FadeIn();

        // Wait for both to finish
        yield return new WaitUntil(() =>
            !DayNightCycle.Instance.IsRunning &&
            !ScreenFader.Instance.IsFading);

        // Hold in black
        yield return new WaitForSeconds(DayNightCycle.Instance.sceneTransitionHoldDuration);

        // Load scene
        DayNightCycle.Instance.LoadScene(sceneName);

        // Wait for scene to load
        yield return new WaitUntil(() => !DayNightCycle.Instance.IsRunning);

        // Fade screen back to clear in new scene
        if (ScreenFader.Instance != null)
            ScreenFader.Instance.FadeOut();
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
}