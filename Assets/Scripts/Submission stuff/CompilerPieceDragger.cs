using UnityEngine;
using UnityEngine.EventSystems;

public class CompilerPieceDragger : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CompilerManager compilerManager;
    private RectTransform rectTransform;
    private Vector2 dragOffset;

    public void Setup(CompilerManager manager)
    {
        compilerManager = manager;
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 pointerPosition
        );

        dragOffset = rectTransform.anchoredPosition - pointerPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 pointerPosition
        );

        rectTransform.anchoredPosition = pointerPosition + dragOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (compilerManager == null)
            compilerManager = CompilerManager.Instance;

        if (compilerManager != null)
            compilerManager.ReturnPieceToHanger(gameObject, eventData.position, eventData.pressEventCamera);
    }
}
