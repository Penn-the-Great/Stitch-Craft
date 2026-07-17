using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class DraggableLerpImage : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas currentCanvas;
    private Canvas storageCanvas;
    private Canvas workspaceCanvas;
    private float targetY;
    private bool isDragging = false;
    private Vector2 dragOffset;
    private bool isInStorage = false;
    private TopProperty storedProperties;

    [SerializeField] private float lerpSpeed = 10f;
    [SerializeField] private float primarySnapY = 0f;
    [SerializeField] private float secondarySnapY = -200f;
    [SerializeField] private float snapThreshold = 50f;
    [SerializeField] private string storageCanvasTag = "StorageCanvas";
    [SerializeField] private string storageSceneName = "Storage";
    [SerializeField] private GameObject hangerPrefab;
    [SerializeField] private bool debugMode = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        currentCanvas = GetComponentInParent<Canvas>();
        storedProperties = GetComponent<TopProperty>();
    }

    void Start()
    {
        // Check if this hanger is in storage based on parent canvas
        storageCanvas = GameObject.FindWithTag(storageCanvasTag)?.GetComponent<Canvas>();
        if (storageCanvas != null && rectTransform.IsChildOf(storageCanvas.transform))
        {
            isInStorage = true;
            targetY = secondarySnapY;
        }
        else
        {
            isInStorage = false;
            targetY = primarySnapY;
        }

        // Immediately snap to correct Y
        Vector2 pos = rectTransform.anchoredPosition;
        pos.y = targetY;
        rectTransform.anchoredPosition = pos;
    }

    void Update()
    {
        // Try to find storage canvas if not found yet
        if (storageCanvas == null)
        {
            GameObject storageCanvasObj = GameObject.FindWithTag(storageCanvasTag);
            if (storageCanvasObj != null)
            {
                storageCanvas = storageCanvasObj.GetComponent<Canvas>();
                if (debugMode) Debug.Log($"Found storage canvas: {storageCanvas.gameObject.name}");
            }
        }

        // Lerp to target Y when not dragging
        if (!isDragging)
        {
            float currentY = rectTransform.anchoredPosition.y;
            if (Mathf.Abs(currentY - targetY) > 0.1f)
            {
                Vector2 pos = rectTransform.anchoredPosition;
                pos.y = Mathf.Lerp(currentY, targetY, Time.deltaTime * lerpSpeed);
                rectTransform.anchoredPosition = pos;
            }
            else if (Mathf.Abs(currentY - targetY) <= 0.1f)
            {
                Vector2 pos = rectTransform.anchoredPosition;
                pos.y = targetY;
                rectTransform.anchoredPosition = pos;
            }
        }
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
        
        if (debugMode) Debug.Log("Drag started");
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

        if (TryMoveToCompiler(eventData))
            return;

        // Check threshold on release
        float currentY = rectTransform.anchoredPosition.y;
        bool isStorageSceneLoaded = SceneManager.GetSceneByName(storageSceneName).isLoaded;

        // Check if in storage threshold and NOT already in storage
        if (isStorageSceneLoaded && storageCanvas != null && !isInStorage)
        {
            float distanceToSecondary = Mathf.Abs(currentY - secondarySnapY);
            if (distanceToSecondary < snapThreshold)
            {
                if (debugMode) Debug.Log($"Released in storage threshold! Distance: {distanceToSecondary}");
                MoveToStorage();
                return;
            }
        }

        // Check if in workspace threshold AND currently in storage
        if (isInStorage)
        {
            float distanceToPrimary = Mathf.Abs(currentY - primarySnapY);
            if (distanceToPrimary < snapThreshold)
            {
                if (debugMode) Debug.Log($"Released in workspace threshold! Distance: {distanceToPrimary}");
                MoveFromStorage();
                return;
            }
        }

        // If not moved to storage/workspace, just snap to current target
        targetY = isInStorage ? secondarySnapY : primarySnapY;
        if (debugMode) Debug.Log($"Released outside threshold. Snapping to Y: {targetY}, Is in storage: {isInStorage}");
    }

    private bool TryMoveToCompiler(PointerEventData eventData)
    {
        if (CompilerManager.Instance == null)
            return false;

        bool droppedOnCompiler = CompilerManager.Instance.IsPointerOverDropArea(
            eventData.position,
            eventData.pressEventCamera
        );

        if (!droppedOnCompiler)
            return false;

        TopProperty properties = GetComponent<TopProperty>();
        if (properties == null)
        {
            if (debugMode) Debug.LogError("No TopProperty component found!");
            return false;
        }

        bool movedToCompiler = CompilerManager.Instance.AddHangerToMannequin(properties);
        if (movedToCompiler)
            Destroy(gameObject);

        return movedToCompiler;
    }

    private void MoveToStorage()
    {
        if (storageCanvas == null || isInStorage)
        {
            if (debugMode && storageCanvas == null) Debug.LogError("Storage canvas not found!");
            if (debugMode && isInStorage) Debug.LogError("Already in storage!");
            return;
        }

        TopProperty properties = GetComponent<TopProperty>();
        if (properties == null)
        {
            if (debugMode) Debug.LogError("No TopProperty component found!");
            return;
        }

        // Save to StorageManager
        if (StorageManager.Instance != null)
        {
            StorageManager.Instance.AddItemToStorage(properties);
            if (debugMode) Debug.Log($"Saved to StorageManager: {properties.displayName}");
        }

        // Save current position before spawning
        Vector2 currentPosition = rectTransform.anchoredPosition;

        if (debugMode) Debug.Log($"Moving to storage: {properties.displayName}");

        // Spawn new hanger in storage at current position
        SpawnInStorage(properties, currentPosition);

        // Destroy original hanger
        Destroy(gameObject);
    }

    private void MoveFromStorage()
    {
        if (!isInStorage)
        {
            if (debugMode) Debug.LogError("Not in storage!");
            return;
        }

        TopProperty properties = GetComponent<TopProperty>();
        if (properties == null)
        {
            if (debugMode) Debug.LogError("No TopProperty component found!");
            return;
        }

        // Remove from StorageManager
        if (StorageManager.Instance != null)
        {
            int count = StorageManager.Instance.GetStorageCount();
            if (count > 0)
            {
                StorageManager.Instance.RemoveItemFromStorage(count - 1);
                if (debugMode) Debug.Log($"Removed from StorageManager: {properties.displayName}");
            }
        }

        // Save current position before spawning
        Vector2 currentPosition = rectTransform.anchoredPosition;

        if (debugMode) Debug.Log($"Moving from storage: {properties.displayName}");

        // Spawn new hanger in workspace at current position
        SpawnInWorkspace(properties, currentPosition);

        // Destroy storage hanger
        Destroy(gameObject);
    }

    private void SpawnInStorage(TopProperty properties, Vector2 spawnPosition)
    {
        if (hangerPrefab == null)
        {
            if (debugMode) Debug.LogError("Hanger prefab not assigned!");
            return;
        }

        GameObject newHanger = Instantiate(hangerPrefab, storageCanvas.transform, false);
        RectTransform newRect = newHanger.GetComponent<RectTransform>();

        if (newRect != null)
        {
            newRect.anchoredPosition = spawnPosition;
        }

        TopProperty newProperties = newHanger.GetComponent<TopProperty>();
        if (newProperties != null)
        {
            newProperties.piece = properties.piece;
            newProperties.displayName = properties.displayName;
            newProperties.color = properties.color;
            newProperties.material = properties.material;
            newProperties.style = properties.style;
            newProperties.grade = properties.grade;
            newProperties.cost = properties.cost;
        }

        DraggableLerpImage draggable = newHanger.GetComponent<DraggableLerpImage>();
        if (draggable != null)
        {
            draggable.isInStorage = true;
            draggable.targetY = secondarySnapY;
            draggable.currentCanvas = storageCanvas;
        }

        if (debugMode) Debug.Log($"Spawned in storage at position: {spawnPosition}");
    }

    private void SpawnInWorkspace(TopProperty properties, Vector2 spawnPosition)
    {
        if (hangerPrefab == null)
        {
            if (debugMode) Debug.LogError("Hanger prefab not assigned!");
            return;
        }

        if (workspaceCanvas == null)
        {
            workspaceCanvas = GetWorkspaceCanvas();
        }

        if (workspaceCanvas == null)
        {
            if (debugMode) Debug.LogError("Workspace canvas not found!");
            return;
        }

        GameObject newHanger = Instantiate(hangerPrefab, workspaceCanvas.transform, false);
        RectTransform newRect = newHanger.GetComponent<RectTransform>();

        if (newRect != null)
        {
            newRect.anchoredPosition = spawnPosition;
        }

        TopProperty newProperties = newHanger.GetComponent<TopProperty>();
        if (newProperties != null)
        {
            newProperties.piece = properties.piece;
            newProperties.displayName = properties.displayName;
            newProperties.color = properties.color;
            newProperties.material = properties.material;
            newProperties.style = properties.style;
            newProperties.grade = properties.grade;
            newProperties.cost = properties.cost;
        }

        DraggableLerpImage draggable = newHanger.GetComponent<DraggableLerpImage>();
        if (draggable != null)
        {
            draggable.isInStorage = false;
            draggable.targetY = primarySnapY;
            draggable.currentCanvas = workspaceCanvas;
        }

        if (debugMode) Debug.Log($"Spawned in workspace at position: {spawnPosition}");
    }

    private Canvas GetWorkspaceCanvas()
    {
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in allCanvases)
        {
            if (!canvas.CompareTag(storageCanvasTag))
            {
                return canvas;
            }
        }
        return null;
    }

    public bool IsInStorage
    { 
    get => isInStorage;
    set => isInStorage = value;
}

}
