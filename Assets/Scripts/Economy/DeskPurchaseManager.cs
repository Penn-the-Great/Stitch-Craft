using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles purchases and pending deliveries.
/// Logs key events and, when delivering, attempts to spawn the delivered item
/// into an open StorageSpawner so the player sees deliveries immediately.
/// </summary>
public class DeskPurchaseManager : MonoBehaviour
{
    public static DeskPurchaseManager Instance { get; private set; }

    [Header("Costume Costs")]
    [SerializeField] private int cheapCostumeBaseCost = 20;
    [SerializeField] private int expensiveCostumeBaseCost = 45;
    [SerializeField] private int gradeAExtraCost = 30;
    [SerializeField] private int gradeBExtraCost = 18;
    [SerializeField] private int gradeCExtraCost = 8;

    [Header("Delivery Time")]
    [SerializeField] private int cheapCostumeDeliveryWeeks = 3;
    [SerializeField] private int expensiveCostumeDeliveryWeeks = 1;

    [Header("Fabric")]
    [SerializeField] private int fabricBundleCost = 15;
    [SerializeField] private int fabricBundleAmount = 5;

    [Header("UI")]
    [SerializeField] private TMP_Text fabricLabel;
    [SerializeField] private TMP_Text pendingDeliveriesLabel;

    [Header("Events")]
    public UnityEvent onPurchaseMade;
    public UnityEvent onDeliveryArrived;
    public UnityEvent<int> onFabricChanged;
    public UnityEvent<int> onPurchaseFailed;

    private readonly List<PendingCostumeDelivery> pendingCostumeDeliveries = new List<PendingCostumeDelivery>();
    private int fabricAmount;

    public int FabricAmount => fabricAmount;
    public int PendingDeliveryCount => pendingCostumeDeliveries.Count;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadFabricAmount();
        UpdateDisplays();

        Debug.Log($"DeskPurchaseManager Awake (Instance id {this.GetInstanceID()})");
    }

    public void BuyCheapCostume() => BuyCostume(cheapCostumeBaseCost, cheapCostumeDeliveryWeeks);
    public void BuyExpensiveCostume() => BuyCostume(expensiveCostumeBaseCost, expensiveCostumeDeliveryWeeks);

    public void BuyFabricBundle()
    {
        if (!TryPay(fabricBundleCost)) return;
        AddFabric(fabricBundleAmount);
        onPurchaseMade?.Invoke();
    }

    public bool BuySpecificCostume(StorageManager.StoredClothingItem item, int cost, int deliveryWeeks)
    {
        Debug.Log($"BuySpecificCostume called for {item?.displayName ?? "null"} cost:{cost} weeks:{deliveryWeeks}");

        if (!TryPay(cost))
        {
            Debug.Log($"BuySpecificCostume: payment failed for {item?.displayName}");
            return false;
        }

        pendingCostumeDeliveries.Add(new PendingCostumeDelivery
        {
            item = item,
            weeksRemaining = deliveryWeeks
        });

        Debug.Log($"BuySpecificCostume: added pending item {item.displayName} with {deliveryWeeks} weeksRemaining. PendingCount={pendingCostumeDeliveries.Count}");

        AddDeliveryCalendarEvent(item, deliveryWeeks);
        UpdateDisplays();
        onPurchaseMade?.Invoke();
        return true;
    }

    public bool BuySpecificFabric(string displayName, string material, Color color, int cost)
    {
        if (!TryPay(cost)) return false;

        if (FabricInventoryManager.Instance != null)
            FabricInventoryManager.Instance.AddFabric(material, color);
        else
            Debug.LogWarning("No FabricInventoryManager found. Fabric purchase could not be added to inventory.");

        AddFabric(1);
        onPurchaseMade?.Invoke();
        return true;
    }

    public void AddFabric(int amount)
    {
        if (amount <= 0) return;
        fabricAmount += amount;
        SaveFabricAmount();
        UpdateDisplays();
        onFabricChanged?.Invoke(fabricAmount);
    }

    public bool CanSpendFabric(int amount) => amount <= 0 || fabricAmount >= amount;

    public bool TrySpendFabric(int amount)
    {
        if (amount <= 0) return true;
        if (!CanSpendFabric(amount)) return false;
        fabricAmount -= amount;
        SaveFabricAmount();
        UpdateDisplays();
        onFabricChanged?.Invoke(fabricAmount);
        return true;
    }

    public void AdvanceDeliveriesOneWeek()
    {
        Debug.Log("AdvanceDeliveriesOneWeek called.");
        for (int i = pendingCostumeDeliveries.Count - 1; i >= 0; i--)
        {
            pendingCostumeDeliveries[i].weeksRemaining--;
            StorageManager.StoredClothingItem current = pendingCostumeDeliveries[i].item;

            if (pendingCostumeDeliveries[i].weeksRemaining <= 0)
            {
                Debug.Log($"AdvanceDeliveriesOneWeek: delivering {current.displayName}");
                DeliverCostume(current);
                pendingCostumeDeliveries.RemoveAt(i);
                onDeliveryArrived?.Invoke();
            }
            else
            {
                Debug.Log($"AdvanceDeliveriesOneWeek: {current.displayName} now has {pendingCostumeDeliveries[i].weeksRemaining} weeks remaining");
            }
        }

        UpdateDisplays();
    }

    private void BuyCostume(int baseCost, int deliveryWeeks)
    {
        char grade = RandomGrade();
        int totalCost = baseCost + GetGradeExtraCost(grade);

        if (!TryPay(totalCost)) return;

        pendingCostumeDeliveries.Add(new PendingCostumeDelivery
        {
            item = CreateRandomCostume(grade, totalCost),
            weeksRemaining = deliveryWeeks
        });

        AddDeliveryCalendarEvent(pendingCostumeDeliveries[pendingCostumeDeliveries.Count - 1].item, deliveryWeeks);
        UpdateDisplays();
        onPurchaseMade?.Invoke();
    }

    private bool TryPay(int cost)
    {
        if (ChapterBudgetManager.Instance == null)
        {
            Debug.LogWarning("No ChapterBudgetManager found. Purchase cancelled.");
            onPurchaseFailed?.Invoke(cost);
            return false;
        }

        bool paid = ChapterBudgetManager.Instance.TrySpend(cost);
        if (!paid) onPurchaseFailed?.Invoke(cost);
        return paid;
    }

    private void DeliverCostume(StorageManager.StoredClothingItem item)
    {
        Debug.Log($"DeliverCostume: delivering {item.displayName}");

        if (StorageManager.Instance == null)
        {
            Debug.Log("DeliverCostume: StorageManager.Instance is null — creating one.");
            new GameObject("StorageManager").AddComponent<StorageManager>();
        }

        // Persist delivered item
        StorageManager.Instance.AddItemToStorage(item);
        Debug.Log($"DeliverCostume: persisted item. Storage count now = {StorageManager.Instance.GetStorageCount()}");

        // If Storage scene is open and a StorageSpawner exists, spawn the single delivered item immediately.
        StorageSpawner spawner = FindObjectOfType<StorageSpawner>();
        if (spawner != null)
        {
            spawner.SpawnSingleItem(item);
            Debug.Log("DeliverCostume: spawned delivered item into active StorageSpawner.");
        }
        else
        {
            Debug.Log("DeliverCostume: no StorageSpawner found in scene (Storage scene not open). Item will appear when Storage is opened.");
        }
    }

    private void AddDeliveryCalendarEvent(StorageManager.StoredClothingItem item, int deliveryWeeks)
    {
       if (DeskCalendarManager.Instance == null || TimelineHandler.Instance == null) return;

        int chapter = TimelineHandler.Instance.GetCurrentChapter();
        int deliveryWeek = TimelineHandler.Instance.GetCurrentWeek() + deliveryWeeks;
        int maxWeek = TimelineHandler.Instance.GetWeeksThisChapter();
        deliveryWeek = Mathf.Clamp(deliveryWeek, 1, maxWeek);
        DeskCalendarManager.Instance.AddDeliveryEvent(chapter, deliveryWeek, $"{item.displayName} delivery");
    }

    private StorageManager.StoredClothingItem CreateRandomCostume(char grade, int cost)
    {
        string piece = RandomPiece();
        return new StorageManager.StoredClothingItem
        {
            piece = piece,
            displayName = GetRandomDisplayNameByPiece(piece),
            color = Random.ColorHSV(),
            material = UsesFabricMaterial(piece) ? RandomMaterial() : "N/A",
            style = RandomStyle(),
            grade = grade,
            cost = cost
        };
    }

    private int GetGradeExtraCost(char grade)
    {
        switch (char.ToUpper(grade))
        {
            case 'A': return gradeAExtraCost;
            case 'B': return gradeBExtraCost;
            case 'C': return gradeCExtraCost;
            default: return 0;
        }
    }

    private char RandomGrade()
    {
        char[] grades = { 'A', 'B', 'C', 'D', 'F' };
        return grades[Random.Range(0, grades.Length)];
    }

    private string RandomPiece()
    {
        string[] pieces = { "top", "bottom", "hat", "shoe", "full" };
        return pieces[Random.Range(0, pieces.Length)];
    }

    private string GetRandomDisplayNameByPiece(string piece)
    {
        switch (piece.ToLower())
        {
            case "top": return PickRandom("Tank Top", "Vest Top", "Button up", "Sweater", "Blouse", "Basic shirt");
            case "bottom": return PickRandom("Jeans", "Slacks", "Shorts", "Harem Pants", "Skirt", "Tights");
            case "hat": return PickRandom("Cowboy hat", "Fedora", "Flat cap", "Beret", "Sun Hat", "Top Hat");
            case "shoe": return PickRandom("Sneakers", "Boots", "Loafers", "Sandals", "Heels", "Fancy Shoes");
            case "full": return PickRandom("Jumpsuit", "Overalls", "Dress", "Morph suit", "Robe", "Suit Set");
            default: return "Clothing Item";
        }
    }

    private string RandomMaterial() => PickRandom("Cotton", "Leather", "Wool", "Fur", "Silk");
    private string RandomStyle() => PickRandom("Ren", "1890's", "1920's", "1960's", "Modern", "Futuristic", "Fantasy");
    private string PickRandom(params string[] options) => options[Random.Range(0, options.Length)];

    private bool UsesFabricMaterial(string piece)
    {
        string lowerPiece = piece.ToLower();
        return lowerPiece != "hat" && lowerPiece != "shoe";
    }

    private void LoadFabricAmount()
    {
        fabricAmount = PersistentGameData.Instance != null ? PersistentGameData.Instance.fabricAmount : 0;
    }

    private void SaveFabricAmount()
    {
        if (PersistentGameData.Instance != null) PersistentGameData.Instance.fabricAmount = fabricAmount;
    }

    private void UpdateDisplays()
    {
        if (fabricLabel != null) fabricLabel.text = fabricAmount.ToString();
        if (pendingDeliveriesLabel != null) pendingDeliveriesLabel.text = pendingCostumeDeliveries.Count.ToString();
    }

    [System.Serializable]
    private class PendingCostumeDelivery { public StorageManager.StoredClothingItem item; public int weeksRemaining; }
}