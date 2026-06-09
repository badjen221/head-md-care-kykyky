using UnityEngine;
using System.Collections;

public class DayNightCycle : MonoBehaviour
{
    [Header("Lights")]
    public Light directionalLight;        // your sun
    public Light[] otherLights;           // any extra lights

    [Header("Timing")]
    public float fadeToDarkDuration  = 5f;
    public float nightHoldDuration   = 3f;
    public float fadeToLightDuration = 5f;

    [Header("Sun Rotation")]
    public float dayAngle   = 50f;    // sun angle during day   (above horizon)
    public float nightAngle = -30f;   // sun angle during night (below horizon)

    [Header("Intensity")]
    public float dayIntensity   = 1f;
    public float nightIntensity = 0f;

    [Header("Light Color")]
    public Color dayLightColor   = new Color(1f, 0.95f, 0.8f);
    public Color nightLightColor = new Color(0.05f, 0.05f, 0.15f);

    [Header("Ambient")]
    public Color dayAmbientColor   = new Color(0.5f, 0.5f, 0.5f);
    public Color nightAmbientColor = new Color(0.02f, 0.02f, 0.08f);

    void Start()
    {
        StartCoroutine(DayNightLoop());
    }

    IEnumerator DayNightLoop()
    {
        while (true)
        {
            yield return StartCoroutine(Fade(false, fadeToDarkDuration));
            yield return new WaitForSeconds(nightHoldDuration);
            yield return StartCoroutine(Fade(true, fadeToLightDuration));
        }
    }

    IEnumerator Fade(bool toDay, float duration)
    {
        float fromAngle     = toDay ? nightAngle     : dayAngle;
        float toAngle       = toDay ? dayAngle       : nightAngle;
        float fromIntensity = toDay ? nightIntensity : dayIntensity;
        float toIntensity   = toDay ? dayIntensity   : nightIntensity;
        Color fromColor     = toDay ? nightLightColor : dayLightColor;
        Color toColor       = toDay ? dayLightColor   : nightLightColor;
        Color fromAmbient   = toDay ? nightAmbientColor : dayAmbientColor;
        Color toAmbient     = toDay ? dayAmbientColor   : nightAmbientColor;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float smoothT = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));

            // Rotate the sun — this is what drives the Procedural Skybox
            if (directionalLight != null)
            {
                float angle = Mathf.Lerp(fromAngle, toAngle, smoothT);
                directionalLight.transform.rotation = Quaternion.Euler(angle, -30f, 0f);

                directionalLight.intensity = Mathf.Lerp(fromIntensity, toIntensity, smoothT);
                directionalLight.color     = Color.Lerp(fromColor, toColor, smoothT);
            }

            // Ambient
            RenderSettings.ambientLight = Color.Lerp(fromAmbient, toAmbient, smoothT);

            // Extra lights
            foreach (Light l in otherLights)
                if (l != null)
                    l.intensity = Mathf.Lerp(fromIntensity, toIntensity, smoothT);

            yield return null;
        }
    }
}