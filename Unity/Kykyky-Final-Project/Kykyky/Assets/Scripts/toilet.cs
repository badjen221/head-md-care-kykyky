using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class toilet : MonoBehaviour, IPointerClickHandler
{
    // Public list to store one or more toilet window game objects
    public List<GameObject> toiletWindows;
    // (Script is attached to the clickable object itself; no clickTargets list needed)
    // Z offsets exposed to the Inspector (closed offset relative to original)
    public float offset0 = 1.31f;
    public float offset1 = -1.31f;
    // Slide settings
    public float slideDuration = 0.5f;
    // Hold before reopening and reopen duration
    public float holdDuration = 2f;
    public float reopenDuration = 1f;

    private List<Vector3> originalPositions;
    private bool isAnimating = false;
    private bool isClosed = false;

    // When this room is clicked, move all windows to a specific position
    public void OnPointerClick(PointerEventData eventData)
    {
        // Only trigger once while animating or if already closed
        if (isAnimating || isClosed) return;

        if (toiletWindows == null || toiletWindows.Count == 0) return;

        StartCoroutine(SlideClose());
    }

    void Start()
    {
        // Cache original positions
        originalPositions = new List<Vector3>();
        if (toiletWindows != null)
        {
            foreach (var w in toiletWindows)
                originalPositions.Add(w != null ? w.transform.position : Vector3.zero);
        }
    }
    private IEnumerator SlideClose()
    {
        isAnimating = true;

        List<Vector3> startPositions = new List<Vector3>();
        List<Vector3> targetPositions = new List<Vector3>();

        for (int i = 0; i < toiletWindows.Count; i++)
        {
            var w = toiletWindows[i];
            if (w == null)
            {
                startPositions.Add(Vector3.zero);
                targetPositions.Add(Vector3.zero);
                continue;
            }

            startPositions.Add(w.transform.position);
            Vector3 orig = (originalPositions != null && originalPositions.Count > i) ? originalPositions[i] : w.transform.position;
            float offs = 0f;
            if (i == 0) offs = offset0;
            else if (i == 1) offs = offset1;
            Vector3 closedPos = orig + new Vector3(0f, 0f, offs);
            targetPositions.Add(closedPos);
        }

        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);

            for (int i = 0; i < toiletWindows.Count; i++)
            {
                var w = toiletWindows[i];
                if (w == null) continue;
                w.transform.position = Vector3.Lerp(startPositions[i], targetPositions[i], t);
            }

            yield return null;
        }

        // Ensure final positions
        for (int i = 0; i < toiletWindows.Count; i++)
        {
            var w = toiletWindows[i];
            if (w == null) continue;
            w.transform.position = targetPositions[i];
        }

        isAnimating = false;
        isClosed = true;
        // After holding closed for holdDuration, reopen smoothly
        StartCoroutine(WaitAndReopen());
    }

    private IEnumerator WaitAndReopen()
    {
        yield return new WaitForSeconds(holdDuration);
        yield return StartCoroutine(SlideOpen());
    }

    private IEnumerator SlideOpen()
    {
        isAnimating = true;

        List<Vector3> startPositions = new List<Vector3>();
        List<Vector3> targetPositions = new List<Vector3>();

        for (int i = 0; i < toiletWindows.Count; i++)
        {
            var w = toiletWindows[i];
            if (w == null)
            {
                startPositions.Add(Vector3.zero);
                targetPositions.Add(Vector3.zero);
                continue;
            }

            startPositions.Add(w.transform.position);
            Vector3 orig = (originalPositions != null && originalPositions.Count > i) ? originalPositions[i] : w.transform.position;
            targetPositions.Add(orig);
        }

        float elapsed = 0f;
        while (elapsed < reopenDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / reopenDuration);

            for (int i = 0; i < toiletWindows.Count; i++)
            {
                var w = toiletWindows[i];
                if (w == null) continue;
                w.transform.position = Vector3.Lerp(startPositions[i], targetPositions[i], t);
            }

            yield return null;
        }

        // Ensure final positions
        for (int i = 0; i < toiletWindows.Count; i++)
        {
            var w = toiletWindows[i];
            if (w == null) continue;
            w.transform.position = targetPositions[i];
        }

        isAnimating = false;
        isClosed = false;
    }
}
