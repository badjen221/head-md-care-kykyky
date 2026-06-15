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

    [Header("Background Color")]
    public Color dayBackgroundColor   = new Color(0.5f, 0.7f, 1f);
    public Color nightBackgroundColor = new Color(0.0f, 0.0f, 0.05f);

    [Header("Day/Night Objects")]
    public GameObject dayscape;
    public GameObject nightscape;
    public float objectFadeDuration = 2f;

    [Header("Lamp Light — on at night, off during day")]
    public Light lampLight;
    public float lampDayIntensity   = 0f;
    public float lampNightIntensity = 1f;

    [Header("Scene Entry")]
    public bool startAtNight = false; // ← tick ON in Scene B

    private bool isRunning = false;
    private Camera mainCamera;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        mainCamera = Camera.main;

        if (directionalLight == null)
            directionalLight = RenderSettings.sun;

        if (directionalLight == null)
        {
            Debug.LogError("DayNightCycle: No Directional Light assigned.");
            return;
        }

        if (startAtNight)
            SetNightState();
        else
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
        if (mainCamera != null) mainCamera.backgroundColor = dayBackgroundColor;
        if (lampLight != null) lampLight.intensity = lampDayIntensity;
        if (dayscape != null) dayscape.SetActive(true);
        if (nightscape != null) nightscape.SetActive(false);
    }

    private void SetNightState()
    {
        directionalLight.transform.rotation = Quaternion.Euler(nightAngle, -30f, 0f);
        directionalLight.intensity = nightIntensity;
        directionalLight.color = nightLightColor;
        RenderSettings.ambientLight = nightAmbientColor;
        if (mainCamera != null) mainCamera.backgroundColor = nightBackgroundColor;
        if (lampLight != null) lampLight.intensity = lampNightIntensity;
        if (dayscape != null) dayscape.SetActive(false);
        if (nightscape != null) nightscape.SetActive(true);
    }

    // -------------------------
    // Public methods
    // -------------------------

    public void FadeToDark()
    {
        if (!isRunning)
            StartCoroutine(FadeToDarkSequence());
    }

    public void FadeToDay()
    {
        if (!isRunning)
            StartCoroutine(FadeToDaySequence());
    }

    public void PlayFullCycle()
    {
        if (!isRunning)
            StartCoroutine(FullCycleSequence());
    }

    public void LoadScene(string sceneName)
    {
        if (!isRunning)
            StartCoroutine(LoadSceneSequence(sceneName));
    }

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

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
            yield return null;

        yield return new WaitForSeconds(sceneTransitionHoldDuration);

        asyncLoad.allowSceneActivation = true;
        yield return null;

        mainCamera = Camera.main;
        if (directionalLight == null)
            directionalLight = RenderSettings.sun;

        isRunning = false;
    }

    // -------------------------
    // Core fade
    // -------------------------

    private IEnumerator Fade(bool toDay, float duration)
    {
        float fromAngle         = toDay ? nightAngle         : dayAngle;
        float toAngle           = toDay ? dayAngle           : nightAngle;
        float fromIntensity     = toDay ? nightIntensity     : dayIntensity;
        float toIntensity       = toDay ? dayIntensity       : nightIntensity;
        Color fromLightColor    = toDay ? nightLightColor    : dayLightColor;
        Color toLightColor      = toDay ? dayLightColor      : nightLightColor;
        Color fromAmbient       = toDay ? nightAmbientColor  : dayAmbientColor;
        Color toAmbient         = toDay ? dayAmbientColor    : nightAmbientColor;
        Color fromBackground    = toDay ? nightBackgroundColor : dayBackgroundColor;
        Color toBackground      = toDay ? dayBackgroundColor   : nightBackgroundColor;
        float fromLampIntensity = toDay ? lampNightIntensity : lampDayIntensity;
        float toLampIntensity   = toDay ? lampDayIntensity   : lampNightIntensity;

        GameObject fadeOut = toDay ? nightscape : dayscape;
        GameObject fadeIn  = toDay ? dayscape   : nightscape;

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

            directionalLight.transform.rotation = Quaternion.Euler(Mathf.Lerp(fromAngle, toAngle, t), -30f, 0f);
            directionalLight.intensity          = Mathf.Lerp(fromIntensity, toIntensity, t);
            directionalLight.color              = Color.Lerp(fromLightColor, toLightColor, t);
            RenderSettings.ambientLight         = Color.Lerp(fromAmbient, toAmbient, t);
            if (mainCamera != null)
                mainCamera.backgroundColor      = Color.Lerp(fromBackground, toBackground, t);
            if (lampLight != null)
                lampLight.intensity             = Mathf.Lerp(fromLampIntensity, toLampIntensity, t);
            if (fadeOut != null) SetObjectAlpha(fadeOut, 1f - t);
            if (fadeIn != null)  SetObjectAlpha(fadeIn, t);

            yield return null;
        }

        directionalLight.transform.rotation = Quaternion.Euler(toAngle, -30f, 0f);
        directionalLight.intensity          = toIntensity;
        directionalLight.color              = toLightColor;
        RenderSettings.ambientLight         = toAmbient;
        if (mainCamera != null) mainCamera.backgroundColor = toBackground;
        if (lampLight != null) lampLight.intensity = toLampIntensity;

        if (fadeOut != null)
        {
            SetObjectAlpha(fadeOut, 0f);
            fadeOut.SetActive(false);
        }
        if (fadeIn != null) SetObjectAlpha(fadeIn, 1f);
    }

    // -------------------------
    // Object alpha helpers
    // -------------------------

    private void SetObjectAlpha(GameObject obj, float alpha)
    {
        foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>())
        {
            foreach (Material mat in renderer.materials)
            {
                Color c = mat.color;
                c.a = alpha;
                mat.color = c;
            }
        }
    }
}