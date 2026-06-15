using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class IntroTapToStart : MonoBehaviour
{
    [Header("Scene Transition")]
    [Tooltip("Scene to load when the user taps. Must be added to Build Settings.")]
    public string nextSceneName = "";

    [Header("Scene Entry (optional)")]
    [Tooltip("Set > 0 to fade the black screen away this many seconds after the intro opens. Leave 0 if the intro already opens fully visible.")]
    public float screenFadeOutDelay = 0f;

    private bool hasTriggered = false;

    void Start()
    {
        // Reveal the intro by fading the black screen out, if requested.
        if (screenFadeOutDelay > 0f)
        {
            if (ScreenFader.Instance != null)
                StartCoroutine(DelayedFadeOut());
            else
                Debug.LogWarning("ScreenFader instance not found.");
        }
    }

    void Update()
    {
        if (hasTriggered)
            return;

        // Read the devices directly so a tap/click anywhere counts, regardless
        // of UI on top. Null-guarded because not every device exists on every
        // platform (no Mouse on a phone, no Touchscreen in the editor).
        bool tapped =
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
            (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame);

        if (tapped)
        {
            hasTriggered = true;
            StartCoroutine(FadeAndLoad());
        }
    }

    IEnumerator FadeAndLoad()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("IntroTapToStart: nextSceneName is empty — nothing to load.");
            hasTriggered = false;
            yield break;
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
        asyncLoad.allowSceneActivation = false;

        // Same fade-to-black-then-load flow used by SceneSequence.FadeAndLoad.
        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.FadeIn();   // fade to black
            yield return new WaitUntil(() =>
                !ScreenFader.Instance.IsFading &&
                asyncLoad.progress >= 0.9f);
        }
        else
        {
            Debug.LogWarning("ScreenFader instance not found — loading without fade.");
            yield return new WaitUntil(() => asyncLoad.progress >= 0.9f);
        }

        asyncLoad.allowSceneActivation = true;
    }

    IEnumerator DelayedFadeOut()
    {
        yield return new WaitForSeconds(screenFadeOutDelay);
        ScreenFader.Instance.FadeOut();
    }
}