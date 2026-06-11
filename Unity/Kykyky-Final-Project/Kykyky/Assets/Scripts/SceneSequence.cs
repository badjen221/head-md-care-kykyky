using UnityEngine;
using System.Collections;

public class SceneSequence : MonoBehaviour
{
    [Header("Rocking Object (optional)")]
    public RockingCradle rockingObject;    // optional — leave empty if not needed

    [Header("Camera Zoom Out")]
    public Camera targetCamera;
    public float zoomOutPercent = 50f;
    public float zoomDuration   = 5f;
    public float returnDuration = 10f;

    private CameraFollowActor cameraFollow;
    private Vector3    initialCameraPosition;
    private Quaternion initialCameraRotation;
    private float      initialFOVOrSize;

    [Header("Scene Transition")]          // ← add this
    public string nextSceneName = "L1-PartB";     // ← and this

    void Start()
    {
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
        // Trigger day/night effect — with optional scene transition
        if (DayNightCycle.Instance != null)
            DayNightCycle.Instance.PlayDayNightEffect(
                string.IsNullOrEmpty(nextSceneName) ? null : nextSceneName
            );
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
    }
}