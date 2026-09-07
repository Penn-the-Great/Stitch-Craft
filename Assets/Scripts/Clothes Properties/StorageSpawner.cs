using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns stored clothing items when Storage scene loads.
/// Now listens for DeskPurchaseManager.onDeliveryArrived so deliveries show up immediately
/// when the Storage scene is loaded (or already open).
/// </summary>
public class StorageSpawner : MonoBehaviour
{
    [SerializeField] private Canvas storageCanvas;
    [SerializeField] private GameObject hangerPrefab;
    [SerializeField] private float secondarySnapY = -200f;
    [SerializeField] private bool debugMode = false;

    void Start()
    {
        if (storageCanvas == null)
        {
            Debug.LogError("Storage canvas not assigned!");
            return;
        }

        if (hangerPrefab == null)
        {
            Debug.LogError("Hanger prefab not assigned!");
            return;
        }

        // Spawn all stored items on start
        SpawnStoredItems();

        // Subscribe to delivery events so new purchases appear immediately
        TrySubscribeToDelivery();
    }

    private void OnEnable()
    {
        TrySubscribeToDelivery();
    }

    private void OnDisable()
    {
        UnsubscribeFromDelivery();
    }

    void OnDestroy()
    {
        UnsubscribeFromDelivery();
    }

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
        RefreshStoredHangers();
    }

    // Destroys existing in-storage hangers and respawns stored items from StorageManager
    private void RefreshStoredHangers()
    {
        if (storageCanvas == null)
        {
            if (debugMode) Debug.LogError("Storage canvas not assigned for refresh!");
            return;
        }

        // Destroy existing storage hangers (only the ones that are in storage)
        DraggableLerpImage[] allHangers = storageCanvas.GetComponentsInChildren<DraggableLerpImage>(true);
        foreach (DraggableLerpImage hanger in allHangers)
        {
            if (hanger.IsInStorage)
                Destroy(hanger.gameObject);
        }

        // Spawn fresh set from StorageManager
        SpawnStoredItems();
    }

    private void SpawnStoredItems()
    {
        if (StorageManager.Instance == null)
        {
            if (debugMode) Debug.LogWarning("StorageManager not found!");
            return;
        }

        List<StorageManager.StoredClothingItem> storedItems = StorageManager.Instance.GetAllStoredItems();

        if (storedItems.Count == 0)
        {
            if (debugMode) Debug.Log("No stored items to spawn");
            return;
        }

        if (debugMode) Debug.Log($"Spawning {storedItems.Count} stored items");

        // Spawn each stored item
        float xOffset = -300f;
        float xStep = storedItems.Count > 0 ? 600f / (storedItems.Count + 1) : 0;

        for (int i = 0; i < storedItems.Count; i++)
        {
            GameObject newHanger = Instantiate(hangerPrefab, storageCanvas.transform, false);
            RectTransform newRect = newHanger.GetComponent<RectTransform>();

            // Position in storage
            if (newRect != null)
            {
                newRect.anchoredPosition = new Vector2(xOffset + (i + 1) * xStep, secondarySnapY);
            }

            // Apply stored properties
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

            // Configure the hanger as in storage
            DraggableLerpImage draggable = newHanger.GetComponent<DraggableLerpImage>();
            if (draggable != null)
            {
                draggable.IsInStorage = true;
            }

            if (debugMode) Debug.Log($"Spawned stored item: {storedItems[i].displayName}");
        }
    }
}