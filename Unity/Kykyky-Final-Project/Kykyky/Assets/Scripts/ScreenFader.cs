using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [Header("Fader")]
    public Image fadeImage;
    public float fadeInDuration  = 1.5f; // transparent -> black
    public float fadeOutDuration = 1.5f; // black -> transparent

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Start fully transparent
        SetAlpha(0f);
    }

    // Fade screen to black
    public void FadeIn()
    {
        if (!isFading)
            StartCoroutine(Fade(0f, 1f, fadeInDuration));
    }

    // Fade screen back to clear
    public void FadeOut()
    {
        if (!isFading)
            StartCoroutine(Fade(1f, 0f, fadeOutDuration));
    }

    public bool IsFading => isFading;
    private bool isFading = false;

    private void SetAlpha(float alpha)
    {
        if (fadeImage == null) return;
        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
    }

    private IEnumerator Fade(float fromAlpha, float toAlpha, float duration)
    {
        isFading = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            SetAlpha(Mathf.Lerp(fromAlpha, toAlpha, t));
            yield return null;
        }

        SetAlpha(toAlpha);
        isFading = false;
    }
}