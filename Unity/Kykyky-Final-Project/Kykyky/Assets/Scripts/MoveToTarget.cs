using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
// This script allows a GameObject to move towards a specified target when the user clicks on it.
public class MoveToTarget : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Target")]
    [SerializeField] private Transform target;

    public Transform Target => target;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stoppingDistance = 0.8f;

    public bool arrivedOnTarget = false;
    public bool movementActivated = false;

    private bool isMoving = false;
    private bool wasInputHeld = false;

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

        if (distance <= stoppingDistance)
        {
            StopMoving(true);
            return;
        }

        arrivedOnTarget = false;

        // Move toward the target on the XZ plane while keeping Y frozen.
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
        if (isMoving)
        {
            return;
        }

        isMoving = true;
        arrivedOnTarget = false;

        FlashEffect flashEffect = GetComponent<FlashEffect>();
        if (flashEffect != null)
        {
            flashEffect.StopFlashing();
        }
    }

    private void StopMoving(bool arrived = false)
    {
        if (!isMoving && !arrived)
        {
            return;
        }

        isMoving = false;
        arrivedOnTarget = arrived;

        if (!arrivedOnTarget)
        {
            FlashEffect flashEffect = GetComponent<FlashEffect>();
            if (flashEffect != null)
            {
                flashEffect.StartFlashing();
            }
        }
    }

    private bool IsGlobalInputHeld()
    {
        if (Touchscreen.current != null)
        {
            return Touchscreen.current.primaryTouch.press.isPressed;
        }

        if (Mouse.current != null)
        {
            return Mouse.current.leftButton.isPressed;
        }

        return false;
    }
}