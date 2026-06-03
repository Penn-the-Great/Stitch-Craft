using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Manages the UI representation of the storage system
/// Displays stored items and allows players to retrieve them
/// </summary>
public class StorageUI : MonoBehaviour
{
    [SerializeField] private Canvas storageCanvas;
    [SerializeField] private GameObject storageItemPrefab; // Same prefab as workspace hangers
    [SerializeField] private TopPropertySpawner spawner;
    [SerializeField] private Button closeStorageButton;
    
    private List<GameObject> displayedItems = new List<GameObject>();

    void Start()
    {
        if (closeStorageButton != null)
        {
            closeStorageButton.onClick.AddListener(HideStorage);
        }
    }

    public void ShowStorage()
    {
        if (storageCanvas != null)
        {
            storageCanvas.gameObject.SetActive(true);
            RefreshStorageDisplay();
        }
    }

    public void HideStorage()
    {
        if (storageCanvas != null)
        {
            storageCanvas.gameObject.SetActive(false);
        }
    }

    private void RefreshStorageDisplay()
    {
        // Clear displayed items
        foreach (GameObject item in displayedItems)
        {
            Destroy(item);
        }
        displayedItems.Clear();

        // Spawn visual representations of stored items
        List<StorageManager.StoredClothingItem> items = StorageManager.Instance.GetAllStoredItems();
        float xOffset = -300f;
        float xStep = 600f / (items.Count + 1);

        for (int i = 0; i < items.Count; i++)
        {
            GameObject displayItem = Instantiate(storageItemPrefab, storageCanvas.transform);
            RectTransform rt = displayItem.GetComponent<RectTransform>();
            
            if (rt != null)
            {
                rt.anchoredPosition = new Vector2(xOffset + (i + 1) * xStep, 0f);
            }

            // Apply stored properties
            TopProperty prop = displayItem.GetComponent<TopProperty>();
            if (prop != null)
            {
                prop.piece = items[i].piece;
                prop.displayName = items[i].displayName;
                prop.color = items[i].color;
                prop.material = items[i].material;
                prop.style = items[i].style;
                prop.grade = items[i].grade;
            }

            displayedItems.Add(displayItem);
        }
    }

    public void RemoveItemFromDisplay(int index)
    {
        StorageManager.Instance.RemoveItemFromStorage(index);
        RefreshStorageDisplay();
    }
}