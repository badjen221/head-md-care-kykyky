using UnityEngine;
using System.Collections;

public class SceneSequence : MonoBehaviour
{
    public RockingCradle rockingCradle;

    [Header("Camera Zoom Out")]
    public Camera targetCamera;
    public float zoomOutPercent = 30f;
    public float zoomDuration   = 2f;
    public float returnDuration = 2f;

    private CameraFollowActor cameraFollow;
    private Vector3    initialCameraPosition;
    private Quaternion initialCameraRotation;
    private float      initialFOVOrSize;

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

        // Get CameraFollowActor directly from the camera GameObject
        cameraFollow = targetCamera.GetComponent<CameraFollowActor>();
        if (cameraFollow == null)
            Debug.LogWarning("CameraFollowActor not found on camera!");

        // Cache initial camera state before following starts
        initialCameraPosition = targetCamera.transform.position;
        initialCameraRotation = targetCamera.transform.rotation;
        initialFOVOrSize      = targetCamera.orthographic
            ? targetCamera.orthographicSize
            : targetCamera.fieldOfView;
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

        // Trigger camera sequence
        if (targetCamera != null)
            StartCoroutine(CameraSequence());
        else
            Debug.LogWarning("No camera found!");
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

        // Lock to exact initial values
        targetCamera.transform.position = initialCameraPosition;
        targetCamera.transform.rotation = initialCameraRotation;

        if (targetCamera.orthographic)
            targetCamera.orthographicSize = initialFOVOrSize;
        else
            targetCamera.fieldOfView = initialFOVOrSize;

        // Step 5 — camera stays at initial position, follow stays disabled
    }
}