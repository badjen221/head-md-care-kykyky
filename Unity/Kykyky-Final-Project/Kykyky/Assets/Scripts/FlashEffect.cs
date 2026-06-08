using UnityEngine;

// This script creates a flashing effect on a GameObject's material emission color, which can be used to draw attention to it. 
//The flashing starts after a specified delay and can be stopped when the object is clicked.
public class FlashEffect : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float delayBeforeStart = 1f;

    [Header("Flash Settings")]
    [SerializeField] private float flashSpeed = 5f;
    [SerializeField] private float minBrightness = 0.3f;
    [SerializeField] private float maxBrightness = 1.8f;

    [Header("Glow Color")]
    [SerializeField] private Color glowColor = new Color(1f, 0.8f, 0f);

    private Material mat;
    private bool isFlashing = false;
    private float timer = 0f;

    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogWarning($"FlashEffect on {gameObject.name}: no Renderer found.");
            return;
        }

        mat = rend.material;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", Color.black);

        Invoke(nameof(StartFlashing), delayBeforeStart);
    }

    void Update()
    {
        if (!isFlashing) return;

        timer += Time.deltaTime * flashSpeed;
        float brightness = Mathf.Lerp(minBrightness, maxBrightness, (Mathf.Sin(timer) + 1f) / 2f);
        mat.SetColor("_EmissionColor", glowColor * brightness);
    }

    public void StartFlashing()
    {
        isFlashing = true;
    }

    public void StopFlashing()
    {
        isFlashing = false;
        mat.SetColor("_EmissionColor", Color.black);
    }

    void OnMouseDown()
    {
        StopFlashing();
        // your click logic here
    }
}