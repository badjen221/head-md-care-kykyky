using UnityEngine;
using System.Collections;

public class RockingCradle : MonoBehaviour
{
    [Header("Rocking Settings")]
    public float rockAngle   = 15f;
    public float rockSpeed   = 1.5f;
    public int   rockCycles  = 20;
    public bool  rockOnStart = true; // ← untick in Inspector to prevent rocking at start

    [Header("Pivot")]
    public bool useSmoothStop = true;

    private bool      isRocking      = false;
    private Coroutine rockRoutine    = null;
    private Quaternion initialRotation;

    // ── Public trigger ────────────────────────────────────────────
    public void StartRocking()
    {
        if (isRocking) return;
        rockRoutine = StartCoroutine(RockCoroutine());
    }

    public void StopRocking()
    {
        if (rockRoutine != null)
        {
            StopCoroutine(rockRoutine);
            rockRoutine = null;
        }
        isRocking = false;
        StartCoroutine(ReturnToRest());
    }

    void Awake()
    {
        // Cache rotation even when script is disabled
        initialRotation = transform.localRotation;
    }

    void Start()
    {
        if (rockOnStart)
            StartRocking();
    }

    // ── Core rocking coroutine ────────────────────────────────────
    IEnumerator RockCoroutine()
    {
        isRocking = true;
        float elapsed   = 0f;
        float totalTime = rockCycles / rockSpeed;

        while (elapsed < totalTime)
        {
            elapsed += Time.deltaTime;

            float progress = elapsed / totalTime;
            float envelope = useSmoothStop
                ? Mathf.SmoothStep(1f, 0f, progress)
                : 1f;

            float angle = Mathf.Sin(elapsed * rockSpeed * Mathf.PI * 2f)
                * rockAngle
                * envelope;

            transform.localRotation = initialRotation * Quaternion.Euler(0f, 0f, angle);

            yield return null;
        }

        yield return StartCoroutine(ReturnToRest());
        isRocking   = false;
        rockRoutine = null;
    }

    // ── Smoothly returns cradle to upright rest position ──────────
    IEnumerator ReturnToRest()
    {
        Quaternion startRot = transform.localRotation;
        Quaternion restRot  = initialRotation;
        float      duration = 0.5f;
        float      elapsed  = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.localRotation = Quaternion.Lerp(startRot, restRot, t);
            yield return null;
        }

        transform.localRotation = restRot;
    }
}