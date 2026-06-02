using UnityEngine;
using UnityEngine.EventSystems;
public class complain : MonoBehaviour, IPointerClickHandler
{
    // create a public list of objects called complaints
    public GameObject[] complaints;

    // start from the first element and deactivate one per click
    private int currentIndex = 0;
    public void OnPointerClick(PointerEventData eventData)
    {
        // deactivate element0 to element5 one by one, keep element6 active
        if (complaints == null || complaints.Length == 0)
        {
            return;
        }

        if (currentIndex >= 0 && currentIndex < complaints.Length - 1 && complaints[currentIndex] != null)
        {
            complaints[currentIndex].SetActive(false);
        }

        if (currentIndex < complaints.Length - 2)
        {
            currentIndex++;
        }
   }
}
