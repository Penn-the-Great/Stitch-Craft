using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableLerpImage : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas currentCanvas;
    private float targetY;
    private bool isDragging = false;
    private Vector2 dragOffset;

    [SerializeField] private float lerpSpeed = 10f;
    [SerializeField] private Canvas storageCanvas; // Assign in inspector
    [SerializeField] private float storageY = 0f; // Y position in storage
    [SerializeField] private float workspaceY = 0f; // Y position in workspace
    
    private bool isInStorage = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        currentCanvas = GetComponentInParent<Canvas>();
        targetY = rectTransform.anchoredPosition.y;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out var localPointerPos
        );
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
        rectTransform.anchoredPosition = localPointerPos + dragOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        
        // Check if item was dragged to storage canvas
        if (storageCanvas != null && IsPointerOverCanvas(eventData, storageCanvas))
        {
            MoveToStorage();
        }
        // Check if item was dragged from storage back to workspace
        else if (isInStorage && currentCanvas != storageCanvas)
        {
            MoveFromStorage();
        }
        else
        {
            // Snap back to appropriate Y position
            targetY = isInStorage ? storageY : workspaceY;
        }
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

    private bool IsPointerOverCanvas(PointerEventData eventData, Canvas canvas)
    {
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        return RectTransformUtility.RectangleContainsScreenPoint(
            canvasRect, 
            eventData.position, 
            eventData.pressEventCamera
        );
    }

    private void MoveToStorage()
    {
        TopProperty property = GetComponent<TopProperty>();
        if (property != null)
        {
            // Save to storage
            StorageManager.Instance.AddItemToStorage(property);
            
            // Move to storage canvas
            rectTransform.SetParent(storageCanvas.transform, false);
            currentCanvas = storageCanvas;
            isInStorage = true;
            targetY = storageY;
            
            // Randomize X position in storage
            Vector2 pos = rectTransform.anchoredPosition;
            pos.x = Random.Range(-300f, 300f);
            rectTransform.anchoredPosition = pos;
        }
    }

    private void MoveFromStorage()
    {
        isInStorage = false;
        targetY = workspaceY;
        // The item will snap back to workspace Y position via the Update lerp
    }

    public bool IsInStorage => isInStorage;
}