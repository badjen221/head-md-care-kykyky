using UnityEngine;
using UnityEngine.InputSystem;

public class ClickToWalk : MonoBehaviour
{
    public Transform target;
    public float moveSpeed = 3.5f;
    public float stoppingDistance = 0.1f;

    private bool _isMoving = false;
    private Vector3 _currentDestination;
    private int _clickCount = 0;

    void Start()
    {
        _currentDestination = transform.position;
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            _clickCount++;
            float fraction = _clickCount / 4f; // after 4 clicks = full distance
            fraction = Mathf.Clamp01(fraction);

            Vector3 startPos = transform.position;
            _currentDestination = Vector3.Lerp(startPos, target.position, 1f / (5f - _clickCount));
            _isMoving = true;
        }

        if (_isMoving)
        {
            float distance = Vector3.Distance(transform.position, _currentDestination);

            if (distance <= stoppingDistance)
            {
                _isMoving = false;
                return;
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                _currentDestination,
                moveSpeed * Time.deltaTime
            );

            transform.LookAt(new Vector3(_currentDestination.x, transform.position.y, _currentDestination.z));
        }
    }
}
