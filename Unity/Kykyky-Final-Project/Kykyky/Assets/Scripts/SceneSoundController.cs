using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSoundController : MonoBehaviour
{
    [SerializeField] private GameObject introSoundObject;
    [SerializeField] private GameObject partBSoundObject;
    [SerializeField] private GameObject partASoundObject;
    [SerializeField] private GameObject logoSoundObject;

    void Start()
    {
        //Check which scene is active and play the corresponding music
        //I have four scenes: L0-Intro-Sounds, L1-PartB-Sounds, L1-PartA-Sounds, and L0-Logo-Sounds
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
