using UnityEngine;


/* 
This script is attached to the GameObject that has the AudioSource component for looping sounds.
It is used to play a random sound from the list, then wait for the sound to finish before playing another random sound from the list. 
The script has a random pause before playing the first sound and before player the next sound. 
The random pause range is configurable in the inspector (min default: 0, max default: 30).
*/
public class SoundsLoop : MonoBehaviour
{

    [SerializeField] private AudioClip[] sounds; // List of sounds to loop
    [SerializeField] private float minFirstPause = 0f; // Minimum random pause before playing the first sound
    [SerializeField] private float maxFirstPause = 30f; // Maximum random pause before playing the first sound
    [SerializeField] private float minNextPause = 0f; // Minimum random pause before playing the next sound
    [SerializeField] private float maxNextPause = 30f; // Maximum random pause before playing the next sound

    private AudioSource audioSource; // Reference to the AudioSource component
    private int lastSoundIndex = -1;
    private Coroutine playSoundsRoutine;


    void OnEnable()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (playSoundsRoutine == null)
        {
            playSoundsRoutine = StartCoroutine(PlaySoundsLoop());
        }
    }

    private void OnDisable()
    {
        if (playSoundsRoutine != null)
        {
            StopCoroutine(playSoundsRoutine);
            playSoundsRoutine = null;
        }
    }


    // Coroutine to play sounds in a loop with random pauses
    private System.Collections.IEnumerator PlaySoundsLoop()
    {
        if (audioSource == null || sounds == null || sounds.Length == 0)
        {
            yield break;
        }

        // Wait for a random pause before playing the first sound
        yield return new WaitForSeconds(Random.Range(minFirstPause, maxFirstPause));
        while (true)
        {
            if (this == null || audioSource == null || sounds == null || sounds.Length == 0)
            {
                yield break;
            }

            // Play a random sound from the list
            int soundIndex = Random.Range(0, sounds.Length);
            if (sounds.Length > 1)
            {
                while (soundIndex == lastSoundIndex)
                {
                    soundIndex = Random.Range(0, sounds.Length);
                }
            }

            lastSoundIndex = soundIndex;
            audioSource.clip = sounds[soundIndex];
            // Debug.Log("Playing sound: " + audioSource.clip.name);
            audioSource.Play();

            // Wait for the sound to finish playing
            yield return new WaitForSeconds(audioSource.clip.length);

            // Wait for a random pause before playing the next sound
            yield return new WaitForSeconds(Random.Range(minNextPause, maxNextPause));
        }
    }
}