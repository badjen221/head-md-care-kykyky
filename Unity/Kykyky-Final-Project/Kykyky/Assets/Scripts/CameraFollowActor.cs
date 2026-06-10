using UnityEngine;

public class CameraFollowActor : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform actor; // drag the actor GameObject here

    [Header("Position Behind Actor")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, -1f);
    [SerializeField] private float transitionSpeed = 2f;

    private bool isFollowing = false;
    private MoveToTarget actorMovement;

    void Start()
    {
        if (actor != null)
        {
            actorMovement = actor.GetComponent<MoveToTarget>();
        }
    }

    void Update()
    {
        if (actor == null || actorMovement == null)
        {
            return;
        }

        // Watch for first activation
        if (!isFollowing && actorMovement.movementActivated)
        {
            isFollowing = true;
        }
        if (!isFollowing || actorMovement.Target == null) return;

        Vector3 actorPosition = actor.position;
        Vector3 targetPosition = actorMovement.Target.position;
        Vector3 toTarget = (targetPosition - actorPosition).normalized;

        // Position the camera behind Lucky relative to the target so the camera faces through Lucky toward the target.
        Vector3 desiredPosition = actorPosition 
                                  - toTarget * Mathf.Abs(offset.z)
                                  + Vector3.up * offset.y
                                  + Vector3.Cross(Vector3.up, toTarget).normalized * offset.x;

        // Smooth transition to that position
        transform.position = Vector3.Lerp(transform.position, 
                                          desiredPosition, 
                                          transitionSpeed * Time.deltaTime);

        // Always look toward the target so Lucky sits between the camera and the target.
        transform.LookAt(targetPosition);
    }
}