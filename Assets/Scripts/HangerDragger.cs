using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableLerpImage : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private float targetY;
    private bool isDragging = false;
    private Vector2 dragOffset;

    [SerializeField] private float lerpSpeed = 10f;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        targetY = rectTransform.anchoredPosition.y;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        // Get local pointer position within the image's parent
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform, // very important: parent, not self
            eventData.position,
            eventData.pressEventCamera,
            out var localPointerPos
        );
        // The offset from the anchor to the cursor (grab point)
        dragOffset = rectTransform.anchoredPosition - localPointerPos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out var localPointerPos
        );
        // The new anchoredPosition is where the mouse is, plus the saved offset
        rectTransform.anchoredPosition = localPointerPos + dragOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }

    void Update()
    {
        if (!isDragging && Mathf.Abs(rectTransform.anchoredPosition.y - targetY) > 0.1f)
        {
            Vector2 pos = rectTransform.anchoredPosition;
            pos.y = Mathf.Lerp(pos.y, targetY, Time.deltaTime * lerpSpeed);
            rectTransform.anchoredPosition = pos;
        }
        if (!isDragging && Mathf.Abs(rectTransform.anchoredPosition.y - targetY) <= 0.1f)
        {
            Vector2 pos = rectTransform.anchoredPosition;
            pos.y = targetY;
            rectTransform.anchoredPosition = pos;
        }
    }
}