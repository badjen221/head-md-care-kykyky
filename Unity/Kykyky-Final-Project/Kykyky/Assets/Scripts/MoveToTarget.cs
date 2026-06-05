using UnityEngine;
using UnityEngine.EventSystems;
public class MoveToTarget : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stoppingDistance = 0.5f;

    public bool arrivedOnTarget = false;
    public bool movementActivated = false;

    private bool isMoving = false;

    void Update()
    {
        if (!isMoving || target == null) return;

        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = target.position;
        Vector3 flatCurrent = new Vector3(currentPosition.x, 0f, currentPosition.z);
        Vector3 flatTarget = new Vector3(targetPosition.x, 0f, targetPosition.z);
        float distance = Vector3.Distance(flatCurrent, flatTarget);

        if (distance <= stoppingDistance)
        {
            isMoving = false;
            arrivedOnTarget = true;
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
        isMoving = true;

        if (!movementActivated)
        {
            movementActivated = true;
        }

        FlashEffect flashEffect = GetComponent<FlashEffect>();
        if (flashEffect != null)        {
            flashEffect.StopFlashing();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isMoving = false;
        FlashEffect flashEffect = GetComponent<FlashEffect>();

        if (flashEffect != null && !arrivedOnTarget)
        {
            flashEffect.StartFlashing();
        }
    }
}