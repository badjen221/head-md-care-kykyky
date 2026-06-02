using UnityEngine;
using UnityEngine.EventSystems;

public class cat_jump : MonoBehaviour, IPointerClickHandler
{
    public Transform[] balconies;
   

    // cat starts on zeroeth balcony
    private int currentIndex = 0;
    private Vector3 targetPos;

    void Start()
    {
        targetPos = transform.position;
        // Debug.Log("cat_jump started");
    }

    // on click
    public void OnPointerClick(PointerEventData eventData)
    {
        // Debug.Log("cat_jump clicked");
        currentIndex++;
        // if last balcony, reset to first
        if (currentIndex >= balconies.Length)
        {
            currentIndex = 0;
        }

        // debug log current index and target position
        // Debug.Log("currentIndex: " + currentIndex);
        // Debug.Log("targetPos: " + targetPos);

        targetPos = balconies[currentIndex].position;

        // jump to target position
        transform.position = targetPos;
    }
        
}