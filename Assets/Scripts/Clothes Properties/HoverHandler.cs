using UnityEngine;
using UnityEngine.EventSystems;

public class HoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler
{
    private TopProperty tp;
        private bool isPointerOver = false;
    private bool isDragging = false;

    void Awake()
    {
        tp = GetComponent<TopProperty>();
        Debug.Log($"[HoverHandler] Awake - tp assigned? {tp != null}");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        ShowPanel();
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        if (!isDragging) HidePanel();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        ShowPanel();
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        if (!isPointerOver) HidePanel();
    }

    void ShowPanel()
    {
        var propertyPanel = GameObject.FindGameObjectWithTag("HoverUI");
        var hoverUI = propertyPanel?.GetComponent<HoverUI>();
        var myRect = GetComponent<RectTransform>();
        if (hoverUI != null && tp != null && myRect != null)
            hoverUI.Show(tp, myRect);
    }

    void HidePanel()
    {
        var propertyPanel = GameObject.FindGameObjectWithTag("HoverUI");
        var hoverUI = propertyPanel?.GetComponent<HoverUI>();
        if (hoverUI != null)
            hoverUI.Hide();
    }
}