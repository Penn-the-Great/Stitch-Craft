using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages persistent storage of hanger objects across scenes.
/// Handles serialization/deserialization of clothing items.
/// </summary>
public class StorageManager : MonoBehaviour
{
    public static StorageManager Instance { get; private set; }

    [System.Serializable]
    public class StoredClothingItem
    {
        public string piece;
        public string displayName;
        public Color color;
        public string material;
        public string style;
        public char grade;
        public int cost;
    }

    private List<StoredClothingItem> storedItems = new List<StoredClothingItem>();
    private const string STORAGE_KEY = "StitchCraft_Storage";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadStorageFromJson();
    }

    /// <summary>
    /// Adds a clothing item to storage (from a TopProperty)
    /// </summary>
    public void AddItemToStorage(TopProperty property)
    {
        var item = new StoredClothingItem
        {
            piece = property.piece,
            displayName = property.displayName,
            color = property.color,
            material = property.material,
            style = property.style,
            grade = property.grade,
            cost = property.cost
        };

        storedItems.Add(item);
        SaveStorageToJson();
    }

    /// <summary>
    /// Adds a clothing item to storage (stored item struct)
    /// </summary>
    public void AddItemToStorage(StoredClothingItem item)
    {
        storedItems.Add(item);
        SaveStorageToJson();
    }

    public void RemoveItemFromStorage(int index)
    {
        if (index >= 0 && index < storedItems.Count)
        {
            storedItems.RemoveAt(index);
            SaveStorageToJson();
        }
    }

    public List<StoredClothingItem> GetAllStoredItems() => new List<StoredClothingItem>(storedItems);

    public int GetStorageCount() => storedItems.Count;

    public void ClearStorage()
    {
        storedItems.Clear();
        PlayerPrefs.DeleteKey(STORAGE_KEY);
        PlayerPrefs.Save();
    }

    private void SaveStorageToJson()
    {
        StorageWrapper wrapper = new StorageWrapper { items = storedItems };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(STORAGE_KEY, json);
        PlayerPrefs.Save();
        Debug.Log($"SaveStorageToJson: saved {storedItems.Count} items to PlayerPrefs.");
    }

    private void LoadStorageFromJson()
    {
        storedItems = new List<StoredClothingItem>();
        if (PlayerPrefs.HasKey(STORAGE_KEY))
        {
            string json = PlayerPrefs.GetString(STORAGE_KEY);
            StorageWrapper wrapper = JsonUtility.FromJson<StorageWrapper>(json);
            if (wrapper != null && wrapper.items != null)
            {
                storedItems = wrapper.items;
                Debug.Log($"LoadStorageFromJson: loaded {storedItems.Count} items from PlayerPrefs.");
            }
            else
            {
                Debug.Log("LoadStorageFromJson: wrapper or items null after JSON parse.");
            }
        }
        else
        {
            Debug.Log("LoadStorageFromJson: no storage key present in PlayerPrefs.");
        }
    }

    // Debug helper
    public void DebugPrintStoredItems()
    {
        Debug.Log($"Stored items count: {storedItems.Count}");
        for (int i = 0; i < storedItems.Count; i++)
        {
            var it = storedItems[i];
            Debug.Log($"Stored[{i}] = {it.displayName} ({it.piece}) grade:{it.grade}");
        }
    }

    [System.Serializable]
    private class StorageWrapper { public List<StoredClothingItem> items; }
}