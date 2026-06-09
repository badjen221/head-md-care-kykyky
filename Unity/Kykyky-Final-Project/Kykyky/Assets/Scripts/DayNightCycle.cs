using UnityEngine;
using System.Collections;

public class DayNightCycle : MonoBehaviour
{
    // Singleton access
    public static DayNightCycle Instance { get; private set; }

    [Header("Sun")]
    public Light directionalLight;

    [Header("Timing")]
    public float fadeToDarkDuration = 5f;
    public float nightHoldDuration = 3f;
    public float fadeToLightDuration = 5f;

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

    private bool isRunning = false;

    void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Use Unity's configured Sun Source if not assigned manually
        if (directionalLight == null)
            directionalLight = RenderSettings.sun;

        if (directionalLight == null)
        {
            Debug.LogError("DayNightCycle: No Directional Light assigned and no Sun Source configured.");
            return;
        }

        SetDayState();
    }

    private void SetDayState()
    {
        directionalLight.transform.rotation =
            Quaternion.Euler(dayAngle, -30f, 0f);

        directionalLight.intensity = dayIntensity;
        directionalLight.color = dayLightColor;

        RenderSettings.ambientLight = dayAmbientColor;
    }

    /// <summary>
    /// Public method that can be called from anywhere:
    /// DayNightCycle.Instance.PlayDayNightEffect();
    /// </summary>
    public void PlayDayNightEffect()
    {
        if (!isRunning)
            StartCoroutine(DayNightSequence());
    }

    private IEnumerator DayNightSequence()
    {
        isRunning = true;

        // Day -> Night
        yield return StartCoroutine(Fade(false, fadeToDarkDuration));

        // Hold Night
        yield return new WaitForSeconds(nightHoldDuration);

        // Night -> Day
        yield return StartCoroutine(Fade(true, fadeToLightDuration));

        isRunning = false;
    }

    private IEnumerator Fade(bool toDay, float duration)
    {
        float fromAngle = toDay ? nightAngle : dayAngle;
        float toAngle = toDay ? dayAngle : nightAngle;

        float fromIntensity = toDay ? nightIntensity : dayIntensity;
        float toIntensity = toDay ? dayIntensity : nightIntensity;

        Color fromLightColor = toDay ? nightLightColor : dayLightColor;
        Color toLightColor = toDay ? dayLightColor : nightLightColor;

        Color fromAmbient = toDay ? nightAmbientColor : dayAmbientColor;
        Color toAmbient = toDay ? dayAmbientColor : nightAmbientColor;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.Clamp01(elapsed / duration));

            // Rotate sun
            float angle = Mathf.Lerp(fromAngle, toAngle, t);

            directionalLight.transform.rotation =
                Quaternion.Euler(angle, -30f, 0f);

            // Sun intensity and color
            directionalLight.intensity =
                Mathf.Lerp(fromIntensity, toIntensity, t);

            directionalLight.color =
                Color.Lerp(fromLightColor, toLightColor, t);

            // Ambient light
            RenderSettings.ambientLight =
                Color.Lerp(fromAmbient, toAmbient, t);

            yield return null;
        }

        // Ensure exact final values
        directionalLight.transform.rotation =
            Quaternion.Euler(toAngle, -30f, 0f);

        directionalLight.intensity = toIntensity;
        directionalLight.color = toLightColor;
        RenderSettings.ambientLight = toAmbient;
    }
}