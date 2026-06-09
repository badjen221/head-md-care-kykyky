using UnityEngine;
using System.Collections;

public class RockingCradle : MonoBehaviour
{
    [Header("Rocking Settings")]
    public float rockAngle     = 15f;   // max degrees to rock side to side
    public float rockSpeed     = 1.5f;  // how fast it rocks
    public int   rockCycles    = 5;     // how many back-and-forth swings before stopping

    [Header("Pivot")]
    // If your cradle's pivot point is not at the bottom center,
    // create an empty parent GameObject at the pivot and attach this script there
    public bool useSmoothStop = true;   // gradually slows down at the end

    private bool     isRocking   = false;
    private Coroutine rockRoutine = null;

    // ── Public trigger ────────────────────────────────────────────
    // Call this from anywhere: other scripts, UI buttons, triggers
    public void StartRocking()
    {
        if (isRocking) return;          // already rocking, ignore
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

        // Snap back to rest position smoothly
        StartCoroutine(ReturnToRest());
    }

    // ── Core rocking coroutine ────────────────────────────────────
    IEnumerator RockCoroutine()
    {
        isRocking = true;

        float elapsed    = 0f;
        float totalTime  = rockCycles / rockSpeed; // total duration based on cycles

        while (elapsed < totalTime)
        {
            elapsed += Time.deltaTime;

            // Progress 0→1 over the full rocking duration
            float progress = elapsed / totalTime;

            // Envelope: starts at 1, fades to 0 at the end for natural slowdown
            float envelope = useSmoothStop
                ? Mathf.SmoothStep(1f, 0f, progress)
                : 1f;

            // Sine wave drives the back-and-forth
            float angle = Mathf.Sin(elapsed * rockSpeed * Mathf.PI * 2f)
                          * rockAngle
                          * envelope;

            transform.localRotation = Quaternion.Euler(0f, 0f, angle);

            yield return null;
        }

        // Settle back to exactly zero
        yield return StartCoroutine(ReturnToRest());

        isRocking   = false;
        rockRoutine = null;
    }

    // Smoothly returns cradle to upright rest position
    IEnumerator ReturnToRest()
    {
        Quaternion startRot = transform.localRotation;
        Quaternion restRot  = Quaternion.identity;
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