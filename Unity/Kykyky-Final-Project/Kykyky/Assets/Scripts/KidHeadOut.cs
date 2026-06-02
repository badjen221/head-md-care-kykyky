using UnityEngine;
using UnityEngine.EventSystems;

public class KidHeadOut : MonoBehaviour, IPointerClickHandler
{
    public GameObject kid;
    public float moveUpSpeed = 2f;
    public float moveDownSpeed = 2f;
    public float holdTime = 3f;
    public float headOffsetY = 0.7f;

    private Vector3 originalPos;
    private bool isHeadOut;

    private void Awake()
    {
        if (kid != null)
        {
            originalPos = kid.transform.position;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (kid == null || isHeadOut)
        {
            return;
        }

        isHeadOut = true;
        StartCoroutine(HeadOutRoutine());
    }

    private System.Collections.IEnumerator HeadOutRoutine()
    {
        Vector3 raisedPos = new Vector3(originalPos.x, originalPos.y + headOffsetY, originalPos.z);

        yield return MoveKid(originalPos, raisedPos, moveUpSpeed);
        yield return new WaitForSeconds(holdTime);
        yield return MoveKid(raisedPos, originalPos, moveDownSpeed);

        isHeadOut = false;
    }

    private System.Collections.IEnumerator MoveKid(Vector3 startPos, Vector3 endPos, float speed)
    {
        float duration = Vector3.Distance(startPos, endPos) / Mathf.Max(speed, 0.0001f);
        if (duration <= 0f)
        {
            if (kid != null)
            {
                kid.transform.position = endPos;
            }

            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t);

            if (kid != null)
            {
                kid.transform.position = Vector3.Lerp(startPos, endPos, t);
            }

            yield return null;
        }

        if (kid != null)
        {
            kid.transform.position = endPos;
        }
    }
}
