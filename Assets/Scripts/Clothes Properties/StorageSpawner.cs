using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns stored clothing items when storage scene loads
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

        // Spawn all stored items
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