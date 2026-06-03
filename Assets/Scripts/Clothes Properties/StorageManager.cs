using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
    }

    private List<StoredClothingItem> storedItems = new List<StoredClothingItem>();
    private const string STORAGE_KEY = "StitchCraft_Storage";

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadStorageFromJson();
    }

    /// <summary>
    /// Adds a clothing item to storage when player leaves scene
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
            grade = property.grade
        };

        storedItems.Add(item);
        SaveStorageToJson();
    }

    /// <summary>
    /// Removes an item from storage (when player takes it out)
    /// </summary>
    public void RemoveItemFromStorage(int index)
    {
        if (index >= 0 && index < storedItems.Count)
        {
            storedItems.RemoveAt(index);
            SaveStorageToJson();
        }
    }

    /// <summary>
    /// Gets all stored items
    /// </summary>
    public List<StoredClothingItem> GetAllStoredItems()
    {
        return new List<StoredClothingItem>(storedItems);
    }

    /// <summary>
    /// Gets stored item count
    /// </summary>
    public int GetStorageCount()
    {
        return storedItems.Count;
    }

    /// <summary>
    /// Clears all storage (optional reset function)
    /// </summary>
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
    }

    private void LoadStorageFromJson()
    {
        if (PlayerPrefs.HasKey(STORAGE_KEY))
        {
            string json = PlayerPrefs.GetString(STORAGE_KEY);
            StorageWrapper wrapper = JsonUtility.FromJson<StorageWrapper>(json);
            if (wrapper != null && wrapper.items != null)
            {
                storedItems = wrapper.items;
            }
        }
    }

    [System.Serializable]
    private class StorageWrapper
    {
        public List<StoredClothingItem> items;
    }
}