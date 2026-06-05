using System.Collections.Generic;
using UnityEngine;

public class DeskStorefrontManager : MonoBehaviour
{
    public static DeskStorefrontManager Instance { get; private set; }

    [Header("Weekly Offer Counts")]
    [SerializeField] private int clothingOfferCount = 5;
    [SerializeField] private int fabricOfferCount = 5;
    [SerializeField] private string[] clothingPiecesByOfferIndex = { "top", "bottom", "hat", "shoe", "full" };

    [Header("Clothing Price Settings")]
    [SerializeField] private int fastDeliveryExtraCost = 25;
    [SerializeField] private int slowDeliveryWeeks = 3;
    [SerializeField] private int fastDeliveryWeeks = 1;
    [SerializeField] private int gradeAExtraCost = 30;
    [SerializeField] private int gradeBExtraCost = 18;
    [SerializeField] private int gradeCExtraCost = 8;

    [Header("Fabric Price Settings")]
    [SerializeField] private int minFabricAmount = 3;
    [SerializeField] private int maxFabricAmount = 9;
    [SerializeField] private int fabricCostPerUnit = 4;

    [Header("Chapter Requirement Weighting")]
    [SerializeField] private ChapterRequirement[] chapterRequirements = new ChapterRequirement[0];
    [SerializeField] private float requiredAttributeChance = 0.7f;

    private readonly List<ClothingOffer> clothingOffers = new List<ClothingOffer>();
    private readonly List<FabricOffer> fabricOffers = new List<FabricOffer>();
    private int generatedChapter = -1;
    private int generatedWeek = -1;

    public int ClothingOfferCount => clothingOffers.Count;
    public int FabricOfferCount => fabricOffers.Count;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SyncToCurrentWeek();
    }

    public void SyncToCurrentWeek()
    {
        int chapter = TimelineHandler.Instance != null ? TimelineHandler.Instance.GetCurrentChapter() : 1;
        int week = TimelineHandler.Instance != null ? TimelineHandler.Instance.GetCurrentWeek() : 1;
        GenerateWeeklyOffers(chapter, week);
    }

    public void GenerateWeeklyOffers(int chapter, int week)
    {
        if (generatedChapter == chapter && generatedWeek == week && clothingOffers.Count > 0 && fabricOffers.Count > 0)
            return;

        generatedChapter = chapter;
        generatedWeek = week;
        clothingOffers.Clear();
        fabricOffers.Clear();

        for (int i = 0; i < clothingOfferCount; i++)
            clothingOffers.Add(CreateClothingOffer(i));

        for (int i = 0; i < fabricOfferCount; i++)
            fabricOffers.Add(CreateFabricOffer());

        RefreshOfferButtons();
    }

    public ClothingOffer GetClothingOffer(int index)
    {
        if (index < 0 || index >= clothingOffers.Count)
            return null;

        return clothingOffers[index];
    }

    public FabricOffer GetFabricOffer(int index)
    {
        if (index < 0 || index >= fabricOffers.Count)
            return null;

        return fabricOffers[index];
    }

    public void BuyClothingOffer(int index)
    {
        ClothingOffer offer = GetClothingOffer(index);
        if (offer == null || offer.purchased || DeskPurchaseManager.Instance == null)
                 return;

        bool purchased = DeskPurchaseManager.Instance.BuySpecificCostume(
            offer.ToStoredClothingItem(),
            offer.cost,
            offer.deliveryWeeks
        );

        if (!purchased)
            return;

        offer.purchased = true;
        RefreshOfferButtons();
    }

    public void BuyFabricOffer(int index)
    {
        FabricOffer offer = GetFabricOffer(index);
        if (offer == null || offer.purchased || DeskPurchaseManager.Instance == null)
            return;

        bool purchased = DeskPurchaseManager.Instance.BuySpecificFabric(
            offer.displayName,
            offer.material,
            offer.color,
            offer.cost
        );

        if (!purchased)
            return;

        offer.purchased = true;
        RefreshOfferButtons();
    }

    private ClothingOffer CreateClothingOffer(int offerIndex)
    {
        ChapterRequirement requirement = GetRequirementForChapter(generatedChapter);
        string piece = GetPieceForOfferIndex(offerIndex);
        char grade = PickWeightedGrade(requirement);
        bool fastDelivery = Random.value > 0.5f;
        int deliveryWeeks = fastDelivery ? fastDeliveryWeeks : slowDeliveryWeeks;
        int cost = GetPieceBaseCost(piece) + GetGradeExtraCost(grade);

        if (fastDelivery)
            cost += fastDeliveryExtraCost;

        return new ClothingOffer
        {
            piece = piece,
            displayName = GetRandomDisplayNameByPiece(piece),
            color = PickWeightedColor(requirement),
            material = UsesFabricMaterial(piece) ? PickWeightedMaterial(requirement) : "N/A",
            style = PickWeightedStyle(requirement),
            grade = grade,
            cost = cost,
            deliveryWeeks = deliveryWeeks
        };
    }

    private FabricOffer CreateFabricOffer()
    {
        string material = RandomMaterial();
        Color color = Random.ColorHSV();
        color.a = 1f;
        int cost = Random.Range(minFabricAmount, maxFabricAmount + 1) * fabricCostPerUnit;

        return new FabricOffer
        {
            displayName = $"{material} fabric",
            material = material,
            color = color,
            cost = cost
        };
    }

    private int GetPieceBaseCost(string piece)
    {
        switch (piece.ToLower())
        {
            case "top":
                return 20;
            case "bottom":
                return 25;
            case "hat":
                return 12;
            case "shoe":
                return 18;
            case "full":
                return 45;
            default:
                return 20;
        }
    }

    private int GetGradeExtraCost(char grade)
    {
        switch (char.ToUpper(grade))
        {
            case 'A':
                return gradeAExtraCost;
            case 'B':
                return gradeBExtraCost;
            case 'C':
                return gradeCExtraCost;
            default:
                return 0;
        }
    }

    private char RandomGrade()
    {
        char[] grades = { 'A', 'B', 'C', 'D', 'F' };
        return grades[Random.Range(0, grades.Length)];
    }

    private char PickWeightedGrade(ChapterRequirement requirement)
    {
        if (requirement != null && requirement.useRequiredGrade && Random.value <= requiredAttributeChance)
            return requirement.requiredGrade;

        return RandomGrade();
    }

    private Color PickWeightedColor(ChapterRequirement requirement)
    {
        if (requirement != null && requirement.useRequiredColor && Random.value <= requiredAttributeChance)
            return requirement.requiredColor;

        return Random.ColorHSV();
    }

    private string PickWeightedMaterial(ChapterRequirement requirement)
    {
        if (requirement != null && requirement.useRequiredMaterial && Random.value <= requiredAttributeChance)
            return requirement.requiredMaterial;

        return RandomMaterial();
    }

    private string PickWeightedStyle(ChapterRequirement requirement)
    {
        if (requirement != null && requirement.useRequiredStyle && Random.value <= requiredAttributeChance)
            return requirement.requiredStyle;

        return RandomStyle();
    }

    private string RandomPiece()
    {
        string[] pieces = { "top", "bottom", "hat", "shoe", "full" };
        return pieces[Random.Range(0, pieces.Length)];
    }

    private string GetPieceForOfferIndex(int offerIndex)
    {
        if (offerIndex >= 0 && offerIndex < clothingPiecesByOfferIndex.Length && !string.IsNullOrEmpty(clothingPiecesByOfferIndex[offerIndex]))
            return clothingPiecesByOfferIndex[offerIndex];

        return RandomPiece();
    }

    private ChapterRequirement GetRequirementForChapter(int chapter)
    {
        if (chapterRequirements == null)
            return null;

        for (int i = 0; i < chapterRequirements.Length; i++)
        {
            if (chapterRequirements[i] != null && chapterRequirements[i].chapter == chapter)
                return chapterRequirements[i];
        }

        return null;
    }

    private string GetRandomDisplayNameByPiece(string piece)
    {
        switch (piece.ToLower())
        {
            case "top":
                return PickRandom("Tank Top", "Vest Top", "Button up", "Sweater", "Blouse", "Basic shirt");
            case "bottom":
                return PickRandom("Jeans", "Slacks", "Shorts", "Harem Pants", "Skirt", "Tights");
            case "hat":
                return PickRandom("Cowboy hat", "Fedora", "Flat cap", "Beret", "Sun Hat", "Top Hat");
            case "shoe":
                return PickRandom("Sneakers", "Boots", "Loafers", "Sandals", "Heels", "Fancy Shoes");
            case "full":
                return PickRandom("Jumpsuit", "Overalls", "Dress", "Morph suit", "Robe", "Suit Set");
            default:
                return "Clothing Item";
        }
    }

    private string RandomMaterial()
    {
        return PickRandom("Cotton", "Leather", "Wool", "Fur", "Silk");
    }

    private string RandomStyle()
    {
        return PickRandom("Ren", "1890's", "1920's", "1960's", "Modern", "Futuristic", "Fantasy");
    }

    private string PickRandom(params string[] options)
    {
        return options[Random.Range(0, options.Length)];
    }

    private bool UsesFabricMaterial(string piece)
    {
        string lowerPiece = piece.ToLower();
        return lowerPiece != "hat" && lowerPiece != "shoe";
    }

    private void RefreshOfferButtons()
    {
        StorefrontOfferButton[] buttons = FindObjectsOfType<StorefrontOfferButton>();
        foreach (StorefrontOfferButton button in buttons)
            button.Refresh();
    }

    [System.Serializable]
    public class ClothingOffer
    {
        public string piece;
        public string displayName;
        public Color color;
        public string material;
        public string style;
        public char grade;
        public int cost;
        public int deliveryWeeks;
        public bool purchased;

        public StorageManager.StoredClothingItem ToStoredClothingItem()
        {
            return new StorageManager.StoredClothingItem
            {
                piece = piece,
                displayName = displayName,
                color = color,
                material = material,
                style = style,
                grade = grade,
                cost = cost
            };
        }
    }

     [System.Serializable]
    public class FabricOffer
    {
        public string displayName;
        public string material;
        public Color color;
        public int cost;
        public bool purchased;
    }

    [System.Serializable]
    public class ChapterRequirement
    {
        public int chapter = 1;
        public bool useRequiredMaterial;
        public string requiredMaterial;
        public bool useRequiredStyle;
        public string requiredStyle;
        public bool useRequiredGrade;
        public char requiredGrade = 'A';
        public bool useRequiredColor;
        public Color requiredColor = Color.white;
    }
}






