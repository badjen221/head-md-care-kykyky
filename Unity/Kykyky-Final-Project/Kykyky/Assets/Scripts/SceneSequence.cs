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
    [Tooltip("Extra seconds to wait after the camera sequence before the transition (fade + load) begins. 0 = start immediately, as before.")]
    public float preTransitionHoldDuration = 0f;

    [Header("Day/Night on Arrival")]
    [Tooltip("ON = run the day→night lighting fade on arrival (also before loading the next scene). " +
             "OFF = no lighting fade; scene stays as-is and (if a next scene is set) just does the screen fade + load. " +
             "Scene A: ON. Scene B: OFF (already night, loops back to intro without re-fading).")]
    public bool fadeToDarkOnArrival = true;

    [Header("Scene Entry")]
    [Tooltip("Set > 0 to fade out the black screen after this many seconds. Leave 0 for no fade out.")]
    public float screenFadeOutDelay = 0f;

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

        // Fade out black screen on scene entry if delay is set
        if (screenFadeOutDelay > 0f)
        {
            if (ScreenFader.Instance != null)
                StartCoroutine(DelayedFadeOut());
            else
                Debug.LogWarning("ScreenFader instance not found.");
        }
    }

    public void OnArrivedAtTarget()
    {
        if (rockingObject != null)
            rockingObject.StartRocking();

        if (targetCamera != null)
            StartCoroutine(CameraSequence());
        else
            Debug.LogWarning("No camera found!");

        if (DayNightCycle.Instance != null)
        {
            if (!string.IsNullOrEmpty(nextSceneName))
                StartCoroutine(FadeAndLoad(nextSceneName));   // Scene A → load next scene
            else if (fadeToDarkOnArrival)
                DayNightCycle.Instance.FadeToDark();           // Scene A standalone → fade to night
            // Scene B: nextSceneName empty + flag off → do nothing, stays at night
        }
        else
            Debug.LogWarning("DayNightCycle instance not found.");
    }

    IEnumerator CameraSequence()
    {
        if (cameraFollow != null)
            cameraFollow.enabled = false;

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

        yield return new WaitForSeconds(1f);

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

        targetCamera.transform.position = initialCameraPosition;
        targetCamera.transform.rotation = initialCameraRotation;

        if (targetCamera.orthographic)
            targetCamera.orthographicSize = initialFOVOrSize;
        else
            targetCamera.fieldOfView = initialFOVOrSize;
    }

    IEnumerator FadeAndLoad(string sceneName)
    {
        float totalCameraDuration = zoomDuration + 1f + returnDuration;
        yield return new WaitForSeconds(totalCameraDuration);

        // Optional extra hold before the transition begins.
        if (preTransitionHoldDuration > 0f)
            yield return new WaitForSeconds(preTransitionHoldDuration);

        // Only run the day→night lighting fade if this scene wants it.
        // Scene A: ON → fades to night before loading.
        // Scene B: OFF → skips it, so looping back to intro keeps the night look
        //          and only the screen fade plays.
        if (fadeToDarkOnArrival && DayNightCycle.Instance != null)
        {
            DayNightCycle.Instance.FadeToDark();
            yield return new WaitUntil(() => !DayNightCycle.Instance.IsRunning);
        }

        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        if (ScreenFader.Instance != null)
            ScreenFader.Instance.FadeIn();

        yield return new WaitUntil(() =>
            !ScreenFader.Instance.IsFading &&
            asyncLoad.progress >= 0.9f);

        asyncLoad.allowSceneActivation = true;
    }

    IEnumerator DelayedFadeOut()
    {
        yield return new WaitForSeconds(screenFadeOutDelay);
        ScreenFader.Instance.FadeOut();
    }
}