using UnityEngine;

public class SceneSequence : MonoBehaviour
{
    public RockingCradle rockingCradle;   // ← this was missing

    public void OnArrivedAtTarget()
    {
        // Trigger day/night effect
        if (DayNightCycle.Instance != null)
            DayNightCycle.Instance.PlayDayNightEffect();
        else
            Debug.LogWarning("DayNightCycle instance not found.");

        // Trigger cradle rocking
        if (rockingCradle != null)
            rockingCradle.StartRocking();
        else
            Debug.LogWarning("RockingCradle reference is missing on SceneSequence!");
    }
}