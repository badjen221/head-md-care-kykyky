using UnityEngine;

public class LogoColorFlashSequential : MonoBehaviour
{
    public float flashDuration = 0.2f;   // how long each letter stays dark
    public float delayBetween = 0.3f;    // pause before the next letter flashes

    public Color darkColor = new Color(0.05f, 0.05f, 0.05f);

    private Renderer rend;
    private Color[] brightColors;
    private float timer;
    private int activeIndex = 0;
    private bool flashing = false;

    void Start()
    {
        rend = GetComponent<Renderer>();
        int count = rend.materials.Length;
        brightColors = new Color[count];

        for (int i = 0; i < count; i++)
            brightColors[i] = rend.materials[i].color;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (!flashing && timer >= delayBetween)
        {
            timer = 0f;
            flashing = true;
            rend.materials[activeIndex].color = darkColor;
        }
        else if (flashing && timer >= flashDuration)
        {
            timer = 0f;
            flashing = false;
            rend.materials[activeIndex].color = brightColors[activeIndex];
            activeIndex = (activeIndex + 1) % brightColors.Length;
        }
    }
}