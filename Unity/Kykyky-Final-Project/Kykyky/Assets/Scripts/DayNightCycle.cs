using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DayNightCycle : MonoBehaviour
{
    public static DayNightCycle Instance { get; private set; }

    [Header("Sun")]
    public Light directionalLight;

    [Header("Timing")]
    public float fadeToDarkDuration = 10f;
    public float nightHoldDuration = 5f;
    public float fadeToLightDuration = 10f;

    [Header("Scene Transition")]
    public float sceneTransitionHoldDuration = 3f;

    [Header("Sun Rotation")]
    public float dayAngle = 50f;
    public float nightAngle = -30f;

    [Header("Intensity")]
    public float dayIntensity = 2.0f;
    public float nightIntensity = 0.05f;

    [Header("Sun Color")]
    public Color dayLightColor = new Color(1f, 0.95f, 0.8f);
    public Color nightLightColor = new Color(0.05f, 0.05f, 0.15f);

    [Header("Ambient")]
    public Color dayAmbientColor = new Color(0.5f, 0.5f, 0.5f);
    public Color nightAmbientColor = new Color(0.02f, 0.02f, 0.05f);

    [Header("Scene Entry")]
    //public bool startAtNight = false;

    private bool isRunning = false;

    public GameObject dayscape;
    public GameObject nightscape;
    public float objectFadeDuration = 2f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (directionalLight == null)
            directionalLight = RenderSettings.sun;

        if (directionalLight == null)
        {
            Debug.LogError("DayNightCycle: No Directional Light assigned.");
            return;
        }

        /*if (startAtNight)
            SetNightState();
        else*/
            SetDayState();
    }

    // -------------------------
    // State setters
    // -------------------------

    private void SetDayState()
    {
        directionalLight.transform.rotation = Quaternion.Euler(dayAngle, -30f, 0f);
        directionalLight.intensity = dayIntensity;
        directionalLight.color = dayLightColor;
        RenderSettings.ambientLight = dayAmbientColor;

        if (dayscape != null) dayscape.SetActive(true);
        if (nightscape != null) nightscape.SetActive(false);
    }

    private void SetNightState()
    {
        directionalLight.transform.rotation = Quaternion.Euler(nightAngle, -30f, 0f);
        directionalLight.intensity = nightIntensity;
        directionalLight.color = nightLightColor;
        RenderSettings.ambientLight = nightAmbientColor;

        if (dayscape != null) dayscape.SetActive(false);
        if (nightscape != null) nightscape.SetActive(true);
    }

    // -------------------------
    // Public methods
    // -------------------------

    /// Day -> Night only
    /// DayNightCycle.Instance.FadeToDark();
    public void FadeToDark()
    {
        if (!isRunning)
            StartCoroutine(FadeToDarkSequence());
    }

    /// Night -> Day only
    /// DayNightCycle.Instance.FadeToDay();
    public void FadeToDay()
    {
        if (!isRunning)
            StartCoroutine(FadeToDaySequence());
    }

    /// Day -> Night -> Day (no scene load)
    /// DayNightCycle.Instance.PlayFullCycle();
    public void PlayFullCycle()
    {
        if (!isRunning)
            StartCoroutine(FullCycleSequence());
    }

    /// Load a scene while already dark — call this AFTER FadeToDark() completes
    /// DayNightCycle.Instance.LoadScene("SceneName");
    public void LoadScene(string sceneName)
    {
        if (!isRunning)
            StartCoroutine(LoadSceneSequence(sceneName));
    }

    /// Check if a fade or load is currently running
    public bool IsRunning => isRunning;

    // -------------------------
    // Sequences
    // -------------------------

    private IEnumerator FadeToDarkSequence()
    {
        isRunning = true;
        yield return StartCoroutine(Fade(false, fadeToDarkDuration));
        isRunning = false;
    }

    private IEnumerator FadeToDaySequence()
    {
        isRunning = true;
        yield return StartCoroutine(Fade(true, fadeToLightDuration));
        isRunning = false;
    }

    private IEnumerator FullCycleSequence()
    {
        isRunning = true;
        yield return StartCoroutine(Fade(false, fadeToDarkDuration));
        yield return new WaitForSeconds(nightHoldDuration);
        yield return StartCoroutine(Fade(true, fadeToLightDuration));
        isRunning = false;
    }

    private IEnumerator LoadSceneSequence(string sceneName)
    {
        isRunning = true;

        Debug.Log("Loading scene: '" + sceneName + "'");

        // Start loading in background
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // Wait until scene is ready
        while (asyncLoad.progress < 0.9f)
            yield return null;

        // Hold in darkness before swapping
        yield return new WaitForSeconds(sceneTransitionHoldDuration);

        // Activate the scene
        asyncLoad.allowSceneActivation = true;
        yield return null;

        // Re-grab the light from the new scene
        if (directionalLight == null)
            directionalLight = RenderSettings.sun;

        isRunning = false;
    }

    // -------------------------
    // Core fade
    // -------------------------

    private IEnumerator Fade(bool toDay, float duration)
    {
        float fromAngle      = toDay ? nightAngle        : dayAngle;
        float toAngle        = toDay ? dayAngle          : nightAngle;
        float fromIntensity  = toDay ? nightIntensity    : dayIntensity;
        float toIntensity    = toDay ? dayIntensity      : nightIntensity;
        Color fromLightColor = toDay ? nightLightColor   : dayLightColor;
        Color toLightColor   = toDay ? dayLightColor     : nightLightColor;
        Color fromAmbient    = toDay ? nightAmbientColor : dayAmbientColor;
        Color toAmbient      = toDay ? dayAmbientColor   : nightAmbientColor;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));

            directionalLight.transform.rotation = Quaternion.Euler(Mathf.Lerp(fromAngle, toAngle, t), -30f, 0f);
            directionalLight.intensity          = Mathf.Lerp(fromIntensity, toIntensity, t);
            directionalLight.color              = Color.Lerp(fromLightColor, toLightColor, t);
            RenderSettings.ambientLight         = Color.Lerp(fromAmbient, toAmbient, t);

            yield return null;
        }

        // Snap to exact final values
        directionalLight.transform.rotation = Quaternion.Euler(toAngle, -30f, 0f);
        directionalLight.intensity          = toIntensity;
        directionalLight.color              = toLightColor;
        RenderSettings.ambientLight         = toAmbient;

        // Swap day/night objects
        if (toDay)
        {
            if (dayscape != null) dayscape.SetActive(true);
            if (nightscape != null) nightscape.SetActive(false);
        }
        else
        {
            if (dayscape != null) dayscape.SetActive(false);
            if (nightscape != null) nightscape.SetActive(true);
        }
    }

    private IEnumerator FadeObjects(GameObject fadeOut, GameObject fadeIn, float duration)
    {
        // Make sure both are active during transition
        if (fadeOut != null) fadeOut.SetActive(true);
        if (fadeIn != null)
        {
            fadeIn.SetActive(true);
            SetObjectAlpha(fadeIn, 0f);
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));

            if (fadeOut != null) SetObjectAlpha(fadeOut, 1f - t);
            if (fadeIn != null)  SetObjectAlpha(fadeIn, t);

            yield return null;
        }

        // Snap to final values
        if (fadeOut != null)
        {
            SetObjectAlpha(fadeOut, 0f);
            fadeOut.SetActive(false);
        }
        if (fadeIn != null) SetObjectAlpha(fadeIn, 1f);
    }

    private void SetObjectAlpha(GameObject obj, float alpha)
    {
        foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>())
        {
            foreach (Material mat in renderer.materials)
            {
                // Works for Standard shader and URP/Lit with surface type Transparent
                Color c = mat.color;
                c.a = alpha;
                mat.color = c;
            }
        }
    }
}