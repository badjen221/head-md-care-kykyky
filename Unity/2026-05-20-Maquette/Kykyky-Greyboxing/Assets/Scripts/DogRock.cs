using UnityEngine;

public class DogRock : MonoBehaviour
{
    [Header("Rocking Settings")]
    public float rockAngle = 15f;       // max tilt in degrees
    public float rockSpeed = 3f;        // oscillations per second
    public float rockDuration = 2.5f;   // how long it rocks

    private bool _isRocking = false;
    private float _rockTimer = 0f;
    private Quaternion _originalRotation;

    void Start()
    {
        _originalRotation = transform.rotation;
    }

    void Update()
    {
        if (!_isRocking) return;

        _rockTimer += Time.deltaTime;

        // Fade out the rock angle over time so it smoothly settles
        float fade = 1f - Mathf.Clamp01(_rockTimer / rockDuration);
        float tilt = Mathf.Sin(_rockTimer * rockSpeed * Mathf.PI * 2f) * rockAngle * fade;

        transform.rotation = _originalRotation * Quaternion.Euler(0f, 0f, tilt);

        if (_rockTimer >= rockDuration)
        {
            _isRocking = false;
            transform.rotation = _originalRotation;
        }
    }

    public void StartRocking()
    {
        _isRocking = true;
        _rockTimer = 0f;
    }
}