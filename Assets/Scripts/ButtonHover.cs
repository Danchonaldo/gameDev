using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    private Vector3 startScale;
    private Vector3 startPosition;

    public float hoverScale = 1.05f;
    public float hoverMove = 8f;
    public float speed = 10f;

    private Vector3 targetScale;
    private Vector3 targetPosition;

    void Start()
    {
        startScale = transform.localScale;
        startPosition = transform.localPosition;

        targetScale = startScale;
        targetPosition = startPosition;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * speed
        );

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            Time.deltaTime * speed
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = startScale * hoverScale;
        targetPosition = startPosition + new Vector3(0, hoverMove, 0);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = startScale;
        targetPosition = startPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = startScale * 0.97f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = startScale * hoverScale;
    }
}