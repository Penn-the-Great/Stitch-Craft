using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns stored clothing items when Storage scene loads.
/// Listens for DeskPurchaseManager.onDeliveryArrived and can spawn single delivered items immediately.
/// </summary>
public class StorageSpawner : MonoBehaviour
{
    [SerializeField] private Canvas storageCanvas;
    [SerializeField] private GameObject hangerPrefab;
    [SerializeField] private float secondarySnapY = -200f;
    [SerializeField] private bool debugMode = false;

    void Start()
    {
        if (storageCanvas == null) { Debug.LogError("Storage canvas not assigned!"); return; }
        if (hangerPrefab == null) { Debug.LogError("Hanger prefab not assigned!"); return; }

        SpawnStoredItems();
        TrySubscribeToDelivery();
    }

    private void OnEnable() => TrySubscribeToDelivery();
    private void OnDisable() => UnsubscribeFromDelivery();
    private void OnDestroy() => UnsubscribeFromDelivery();

    private void TrySubscribeToDelivery()
    {
        if (DeskPurchaseManager.Instance != null)
        {
            DeskPurchaseManager.Instance.onDeliveryArrived.AddListener(OnDeliveryArrived);
            if (debugMode) Debug.Log("StorageSpawner: subscribed to onDeliveryArrived");
        }
    }

    private void UnsubscribeFromDelivery()
    {
        if (DeskPurchaseManager.Instance != null)
        {
            DeskPurchaseManager.Instance.onDeliveryArrived.RemoveListener(OnDeliveryArrived);
            if (debugMode) Debug.Log("StorageSpawner: unsubscribed from onDeliveryArrived");
        }
    }

    private void OnDeliveryArrived()
    {
        if (debugMode) Debug.Log("OnDeliveryArrived: refreshing storage hangers.");
        if (StorageManager.Instance != null) Debug.Log($"OnDeliveryArrived: StorageManager has {StorageManager.Instance.GetStorageCount()} items.");
        RefreshStoredHangers();
    }

    private void RefreshStoredHangers()
    {
        if (storageCanvas == null) { if (debugMode) Debug.LogError("Storage canvas not assigned for refresh!"); return; }

        DraggableLerpImage[] allHangers = storageCanvas.GetComponentsInChildren<DraggableLerpImage>(true);
        foreach (DraggableLerpImage hanger in allHangers)
        {
            if (hanger.IsInStorage) Destroy(hanger.gameObject);
        }

        SpawnStoredItems();
    }

    private void SpawnStoredItems()
    {
        if (StorageManager.Instance == null) { if (debugMode) Debug.LogWarning("StorageManager not found!"); return; }

        List<StorageManager.StoredClothingItem> storedItems = StorageManager.Instance.GetAllStoredItems();
        if (debugMode) Debug.Log($"SpawnStoredItems: storage contains {storedItems.Count} items.");

        if (storedItems.Count == 0) { if (debugMode) Debug.Log("No stored items to spawn"); return; }
        if (debugMode) Debug.Log($"Spawning {storedItems.Count} stored items");

        float xOffset = -300f;
        float xStep = storedItems.Count > 0 ? 600f / (storedItems.Count + 1) : 0;

        for (int i = 0; i < storedItems.Count; i++)
        {
            GameObject newHanger = Instantiate(hangerPrefab, storageCanvas.transform, false);
            RectTransform newRect = newHanger.GetComponent<RectTransform>();
            if (newRect != null) newRect.anchoredPosition = new Vector2(xOffset + (i + 1) * xStep, secondarySnapY);

            TopProperty newProperties = newHanger.GetComponent<TopProperty>();
            if (newProperties != null)
            {
                StorageManager.StoredClothingItem item = storedItems[i];
                newProperties.piece = item.piece;
                newProperties.displayName = item.displayName;
                newProperties.color = item.color;
                newProperties.material = item.material;
                newProperties.style = item.style;
                newProperties.grade = item.grade;
                newProperties.cost = item.cost;
            }

            DraggableLerpImage draggable = newHanger.GetComponent<DraggableLerpImage>();
            if (draggable != null) draggable.IsInStorage = true;

            if (debugMode) Debug.Log($"Spawned stored item: {storedItems[i].displayName}");
        }
    }

    // Spawn a single delivered item into the currently-open storage UI (does NOT modify StorageManager).
    public void SpawnSingleItem(StorageManager.StoredClothingItem item)
    {
        if (storageCanvas == null || hangerPrefab == null || item == null) return;

        DraggableLerpImage[] current = storageCanvas.GetComponentsInChildren<DraggableLerpImage>(true);
        int count = current != null ? current.Length : 0;

        float xOffset = -300f;
        float xStep = 600f / (Mathf.Max(1, count + 1));
        float xPos = xOffset + (count + 1) * xStep;

        GameObject newHanger = Instantiate(hangerPrefab, storageCanvas.transform, false);
        RectTransform newRect = newHanger.GetComponent<RectTransform>();
        if (newRect != null) newRect.anchoredPosition = new Vector2(xPos, secondarySnapY);

        TopProperty props = newHanger.GetComponent<TopProperty>();
        if (props != null)
        {
            props.piece = item.piece;
            props.displayName = item.displayName;
            props.color = item.color;
            props.material = item.material;
            props.style = item.style;
            props.grade = item.grade;
            props.cost = item.cost;
        }

        DraggableLerpImage d = newHanger.GetComponent<DraggableLerpImage>();
        if (d != null) d.IsInStorage = true;

        if (debugMode) Debug.Log($"SpawnSingleItem: spawned {item.displayName}");
    }
}