using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;
// This script allows a GameObject to move towards a specified target when the user clicks on it.
public class MoveToTarget : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Target")]
    [SerializeField] private Transform target;

    public Transform Target => target;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stoppingDistance = 0.5f;

    public bool arrivedOnTarget = false;
    public bool movementActivated = false;

    private bool isMoving = false;
    private readonly List<AudioSource> pausedAudioSources = new List<AudioSource>();

    void Update()
    {
        if (movementActivated)
        {
            bool inputHeld = IsGlobalInputHeld();

            if (inputHeld && !isMoving)
            {
                isMoving = true;

                FlashEffect flashEffect = GetComponent<FlashEffect>();
                if (flashEffect != null)
                {
                    flashEffect.StopFlashing();
                }
            }
            else if (!inputHeld && isMoving)
            {
                StopMoving();
            }
        }

        if (!isMoving || target == null) return;

        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = target.position;
        Vector3 flatCurrent = new Vector3(currentPosition.x, 0f, currentPosition.z);
        Vector3 flatTarget = new Vector3(targetPosition.x, 0f, targetPosition.z);
        float distance = Vector3.Distance(flatCurrent, flatTarget);

        if (distance <= stoppingDistance)
        {
            arrivedOnTarget = true;
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

        StartMoving();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
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

        PauseActiveAudioSources();

        FlashEffect flashEffect = GetComponent<FlashEffect>();
        if (flashEffect != null)
        {
            flashEffect.StopFlashing();
        }
    }

    private void StopMoving(bool arrived = false)
    {
        if (!isMoving && pausedAudioSources.Count == 0)
        {
            arrivedOnTarget = arrivedOnTarget || arrived;
            return;
        }

        isMoving = false;
        arrivedOnTarget = arrived;

        ResumePausedAudioSources();

        if (!arrivedOnTarget)
        {
            FlashEffect flashEffect = GetComponent<FlashEffect>();
            if (flashEffect != null)
            {
                flashEffect.StartFlashing();
            }
        }
    }

    private void PauseActiveAudioSources()
    {
        if (target == null)
        {
            return;
        }

        pausedAudioSources.Clear();

        AudioSource[] actorSources = GetComponentsInChildren<AudioSource>(true);
        AudioSource[] targetSources = target.GetComponentsInChildren<AudioSource>(true);

        HashSet<AudioSource> uniqueSources = new HashSet<AudioSource>();

        foreach (AudioSource audioSource in actorSources)
        {
            if (audioSource != null && audioSource.isPlaying && uniqueSources.Add(audioSource))
            {
                audioSource.Pause();
                pausedAudioSources.Add(audioSource);
            }
        }

        foreach (AudioSource audioSource in targetSources)
        {
            if (audioSource != null && audioSource.isPlaying && uniqueSources.Add(audioSource))
            {
                audioSource.Pause();
                pausedAudioSources.Add(audioSource);
            }
        }
    }

    private void ResumePausedAudioSources()
    {
        for (int i = pausedAudioSources.Count - 1; i >= 0; i--)
        {
            AudioSource audioSource = pausedAudioSources[i];
            if (audioSource != null)
            {
                audioSource.UnPause();
            }
        }

        pausedAudioSources.Clear();
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