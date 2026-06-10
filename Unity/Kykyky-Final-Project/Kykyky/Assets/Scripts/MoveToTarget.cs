using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MoveToTarget : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Target")]
    [SerializeField] private Transform target;
    public Transform Target => target;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stoppingDistance = 0.8f;

    [Header("Audio - Stop when movement starts")]
    [Tooltip("SoundsLoop components that will be stopped when movement begins (e.g. SoundCrying)")]
    [SerializeField] private SoundsLoop[] stopOnMoveStart;

    [Header("Audio - Play when movement starts")]
    [Tooltip("SoundsLoop components that will be played when movement begins (e.g. footsteps)")]
    [SerializeField] private SoundsLoop[] playOnMoveStart;

    [Header("Audio - Play on arrival")]
    [Tooltip("SoundsLoop components that will be started when the GameObject reaches the target (e.g. celebration)")]
    [SerializeField] private SoundsLoop[] playOnArrival;

    public bool arrivedOnTarget = false;
    public bool movementActivated = false;
    private bool isMoving = false;
    private bool wasInputHeld = false;
    public SceneSequence sceneSequence;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();  // ← this line is missing
        if (animator == null)
            Debug.LogWarning("No Animator found on " + gameObject.name);

        SetSoundsLoopActive(playOnMoveStart, false);
        SetSoundsLoopActive(playOnArrival, false);
    }

    void Update()
    {
        if (movementActivated)
        {
            bool inputHeld = IsGlobalInputHeld();
            if (inputHeld && !wasInputHeld)
            {
                StartMoving();
            }
            else if (!inputHeld && wasInputHeld)
            {
                StopMoving();
            }
            wasInputHeld = inputHeld;
        }

        if (!isMoving || target == null) return;

        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = target.position;
        Vector3 flatCurrent = new Vector3(currentPosition.x, 0f, currentPosition.z);
        Vector3 flatTarget = new Vector3(targetPosition.x, 0f, targetPosition.z);
        float distance = Vector3.Distance(flatCurrent, flatTarget);

        // ← add these two lines
        Debug.Log("Distance to target: " + distance);
        Debug.Log("Stopping distance: " + stoppingDistance);

        if (distance <= stoppingDistance)
        {
            StopMoving(true);
            return;
        }

        arrivedOnTarget = false;

        Vector3 direction = (flatTarget - flatCurrent).normalized;
        Vector3 nextPosition = currentPosition + new Vector3(direction.x, 0f, direction.z) * moveSpeed * Time.deltaTime;
        transform.position = new Vector3(nextPosition.x, currentPosition.y, nextPosition.z);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!movementActivated)
        {
            movementActivated = true;
        }
        wasInputHeld = true;
        StartMoving();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        wasInputHeld = false;
        StopMoving();
    }

    private void StartMoving()
    {
        if (isMoving) return;

        isMoving = true;
        arrivedOnTarget = false;

        SetSoundsLoopActive(stopOnMoveStart, false);
        SetSoundsLoopActive(playOnMoveStart, true);

        FlashEffect flashEffect = GetComponent<FlashEffect>();
        if (flashEffect != null)
            flashEffect.StopFlashing();

        if (animator != null)
            animator.SetBool("isMoving", true);
    }

    private void StopMoving(bool arrived = false)
    {
        if (!isMoving && !arrived) return;

        isMoving = false;
        arrivedOnTarget = arrived;

        SetSoundsLoopActive(playOnMoveStart, false);

        if (animator != null)
            animator.SetBool("isMoving", false);

        if (arrivedOnTarget)
        {
            SetSoundsLoopActive(stopOnMoveStart, false);
            SetSoundsLoopActive(playOnArrival, true);
            if (sceneSequence != null)
                sceneSequence.OnArrivedAtTarget();
            else
                Debug.LogWarning("SceneSequence reference missing on MoveToTarget!");
        }
        else
        {
            SetSoundsLoopActive(stopOnMoveStart, true);

            FlashEffect flashEffect = GetComponent<FlashEffect>();
            if (flashEffect != null)
                flashEffect.StartFlashing();
        }
    }

    private void SetSoundsLoopActive(SoundsLoop[] loops, bool active)
    {
        if (loops == null) return;
        foreach (SoundsLoop loop in loops)
        {
            if (loop != null)
            {
                if (active) loop.StartLoop();
                else loop.StopLoop();
            }
        }
    }

    private bool IsGlobalInputHeld()
    {
        if (Touchscreen.current != null)
            return Touchscreen.current.primaryTouch.press.isPressed;
        if (Mouse.current != null)
            return Mouse.current.leftButton.isPressed;
        return false;
    }
}