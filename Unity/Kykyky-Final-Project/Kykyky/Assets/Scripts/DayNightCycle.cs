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
    public bool startAtNight = false;

    private bool isRunning = false;

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

        if (startAtNight)
            SetNightState();
        else
            SetDayState();
    }

    private void SetDayState()
    {
        directionalLight.transform.rotation = Quaternion.Euler(dayAngle, -30f, 0f);
        directionalLight.intensity = dayIntensity;
        directionalLight.color = dayLightColor;
        RenderSettings.ambientLight = dayAmbientColor;
    }

    private void SetNightState()
    {
        directionalLight.transform.rotation = Quaternion.Euler(nightAngle, -30f, 0f);
        directionalLight.intensity = nightIntensity;
        directionalLight.color = nightLightColor;
        RenderSettings.ambientLight = nightAmbientColor;
    }

    // No scene transition:  DayNightCycle.Instance.PlayDayNightEffect();
    // With scene transition: DayNightCycle.Instance.PlayDayNightEffect("SceneName");
    public void PlayDayNightEffect(string sceneToLoad = null)
    {
        if (!isRunning)
            StartCoroutine(DayNightSequence(sceneToLoad));
    }

    private IEnumerator DayNightSequence(string sceneToLoad = null)
    {
        isRunning = true;

        // Day -> Night
        yield return StartCoroutine(Fade(false, fadeToDarkDuration));

        if (sceneToLoad != null)
        {
            Debug.Log("Attempting to load scene: '" + sceneToLoad + "'");
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
            asyncLoad.allowSceneActivation = false;

            while (asyncLoad.progress < 0.9f)
                yield return null;

            yield return new WaitForSeconds(nightHoldDuration);

            // Swap scene while dark — player sees nothing
            asyncLoad.allowSceneActivation = true;
            yield return null;

            // Re-grab the light from the new scene
            if (directionalLight == null)
                directionalLight = RenderSettings.sun;
        }
        else
        {
            yield return new WaitForSeconds(nightHoldDuration);
        }

        // Night -> Day
        yield return StartCoroutine(Fade(true, fadeToLightDuration));

        isRunning = false;
    }

    private IEnumerator Fade(bool toDay, float duration)
    {
        float fromAngle       = toDay ? nightAngle     : dayAngle;
        float toAngle         = toDay ? dayAngle       : nightAngle;
        float fromIntensity   = toDay ? nightIntensity : dayIntensity;
        float toIntensity     = toDay ? dayIntensity   : nightIntensity;
        Color fromLightColor  = toDay ? nightLightColor : dayLightColor;
        Color toLightColor    = toDay ? dayLightColor   : nightLightColor;
        Color fromAmbient     = toDay ? nightAmbientColor : dayAmbientColor;
        Color toAmbient       = toDay ? dayAmbientColor   : nightAmbientColor;

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
    }
}