using UnityEngine;

public class RockingMotion : MonoBehaviour
{
    [Header("Rocking Settings")]
    public float angle     = 15f;   // max tilt in degrees
    public float speed     = 1f;    // rocks per second
    public float phaseOffset = 0f; // stagger multiple objects

    private Quaternion _baseRotation;

    void Start()
    {
        _baseRotation = transform.rotation;
    }

    void Update()
    {
        float tilt = angle * Mathf.Sin(
            Time.time * speed * Mathf.PI * 2f + phaseOffset
        );

        transform.rotation = _baseRotation *
            Quaternion.Euler(0f, 0f, tilt);
    }
}
