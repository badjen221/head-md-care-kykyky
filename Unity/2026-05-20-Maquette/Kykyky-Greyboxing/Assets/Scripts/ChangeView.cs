using UnityEngine;

// to detect touch/click we need to access the event system code library
using UnityEngine.EventSystems;

public class ChangeView : MonoBehaviour, IPointerClickHandler
{

    public void OnPointerClick(PointerEventData eventData)
    {
        // Turnoff all the cameras in the scene
        Camera[] cameras = FindObjectsOfType<Camera>();
        foreach (Camera cam in cameras)        
        {
            cam.enabled = false;   
        }
        // Turn on the camera that is a child of this game object
        Camera camToEnable = GetComponentInChildren<Camera>();
        camToEnable.enabled = true;

    }


}
