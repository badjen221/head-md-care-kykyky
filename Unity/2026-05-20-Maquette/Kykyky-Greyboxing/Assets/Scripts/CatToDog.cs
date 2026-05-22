using UnityEngine;
using UnityEngine.InputSystem;

public class CatToDog : MonoBehaviour
{
    [Header("References")]
    public Transform dog;
    private DogRock _dogRock;

    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float stoppingDistance = 0.3f;

    [Header("Circling")]
    public float circleRadius = 3f;
    public float circleSpeed = 80f;
    public int circleCount = 2;

    private enum State { Idle, CirclingApproach, Approaching }
    private State _state = State.Idle;

    private float _currentAngle = 0f;
    private float _totalAngleCircled = 0f;
    private float _requiredAngle;
    private Vector3 _circleCenter;

    void Start()
    {
        _requiredAngle = 360f * circleCount;
        _dogRock = dog.GetComponent<DogRock>();
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && _state == State.Idle)
        {
            StartCircling();
        }

        switch (_state)
        {
            case State.CirclingApproach:
                DoCircling();
                break;
            case State.Approaching:
                DoApproach();
                break;
        }
    }

    void StartCircling()
    {
        _state = State.CirclingApproach;
        _totalAngleCircled = 0f;
        _circleCenter = dog.position;

        Vector3 offset = transform.position - _circleCenter;
        _currentAngle = Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;
    }

    void DoCircling()
    {
        float progress = _totalAngleCircled / _requiredAngle;
        float currentRadius = Mathf.Lerp(circleRadius, stoppingDistance + 0.5f, progress);

        _currentAngle += circleSpeed * Time.deltaTime;
        _totalAngleCircled += circleSpeed * Time.deltaTime;

        float rad = _currentAngle * Mathf.Deg2Rad;
        Vector3 targetPos = _circleCenter + new Vector3(
            Mathf.Cos(rad) * currentRadius,
            0f,
            Mathf.Sin(rad) * currentRadius
        );

        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        transform.LookAt(new Vector3(dog.position.x, transform.position.y, dog.position.z));

        if (_totalAngleCircled >= _requiredAngle)
            _state = State.Approaching;
    }

    void DoApproach()
    {
        float distance = Vector3.Distance(transform.position, dog.position);

        if (distance <= stoppingDistance)
        {
            _state = State.Idle;
            _dogRock?.StartRocking();   // 👈 trigger the dog
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, dog.position, moveSpeed * Time.deltaTime);
        transform.LookAt(new Vector3(dog.position.x, transform.position.y, dog.position.z));
    }
}