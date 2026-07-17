using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class TopPropertySpawner : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public Canvas targetCanvas;

    [Header("Color Pools (Inspector Controlled)")]
    [SerializeField] private List<Color> allowedClothingColors = new List<Color>();
    [SerializeField] private List<Color> allowedFabricColors = new List<Color>(); // future use

    [Header("Economy")]
    [SerializeField] private bool requireBudget = true;
    [SerializeField] private int topCost = 20;
    [SerializeField] private int bottomCost = 25;
    [SerializeField] private int hatCost = 12;
    [SerializeField] private int shoeCost = 18;
    [SerializeField] private int fullOutfitCost = 45;
    [SerializeField] private int gradeAExtraCost = 20;
    [SerializeField] private int gradeBExtraCost = 12;
    [SerializeField] private int gradeCExtraCost = 6;

    private string[] topNames = { "Tank Top", "Vest Top", "Button up", "Sweater", "Blouse", "Basic shirt" };
    private string[] bottomNames = { "Jeans", "Slacks", "Shorts", "Harem Pants", "Skirt", "Tights" };
    private string[] hatNames = { "Cowboy hat", "Fedora", "Flat cap", "Beret", "Sun Hat", "Top Hat" };
    private string[] shoeNames = { "Sneakers", "Boots", "Loafers", "Sandals", "Heels", "Fancy" };
    private string[] fullNames = { "Jumpsuit", "Overall", "Dress", "Morph suit", "Robe", "Suit Set" };

    public void SpawnOnCanvas(Vector2 anchoredPosition)
    {
        GameObject obj = Instantiate(prefabToSpawn);
        obj.transform.SetParent(targetCanvas.transform, false);

        RectTransform rt = obj.GetComponent<RectTransform>();
        if (rt != null) rt.anchoredPosition = anchoredPosition;
        else Debug.LogWarning("Instantiated object does not have a RectTransform! Make sure your prefab is a UI element.");
    }

    public void SetPresetProperties(GameObject obj, string piece, string displayName, Color color, string material, string style, char grade, int cost = 0)
    {
        var tp = obj.GetComponent<TopProperty>();
        if (tp != null)
        {
            tp.piece = piece;
            tp.displayName = displayName;
            tp.color = color;
            tp.material = material;
            tp.style = style;
            tp.grade = grade;
            tp.cost = cost > 0 ? cost : CalculateCost(piece, grade);
        }
    }

    private string RandomMaterial()
    {
        string[] mats = { "Cotton", "Leather", "Wool", "Fur", "Silk" };
        return mats[Random.Range(0, mats.Length)];
    }

    private string RandomStyle()
    {
        string[] styles = { "Ren", "1890's", "1920's", "1960's", "Modern", "Futuristic", "Fantasy" };
        return styles[Random.Range(0, styles.Length)];
    }

    private char RandomGrade()
    {
        char[] grades = { 'A', 'B', 'C', 'D', 'F' };
        return grades[Random.Range(0, grades.Length)];
    }

    private string GetRandomDisplayNameByPiece(string piece)
    {
        switch (piece.ToLower())
        {
            case "top": return topNames[Random.Range(0, topNames.Length)];
            case "bottom": return bottomNames[Random.Range(0, bottomNames.Length)];
            case "hat": return hatNames[Random.Range(0, hatNames.Length)];
            case "shoe": return shoeNames[Random.Range(0, shoeNames.Length)];
            case "full": return fullNames[Random.Range(0, fullNames.Length)];
            default: return "Clothing Item";
        }
    }

    private Color PickRandomColorFromList(List<Color> pool, Color fallback)
    {
        if (pool == null || pool.Count == 0) return fallback;
        Color c = pool[Random.Range(0, pool.Count)];
        c.a = 1f;
        return c;
    }

    private Color RandomClothingColor()
    {
        return PickRandomColorFromList(allowedClothingColors, Color.white);
    }

    private Color RandomFabricColor()
    {
        return PickRandomColorFromList(allowedFabricColors, Color.gray);
    }

    public void SpawnRandomOnCanvas(string piece)
    {
        float randomX = Random.Range(-300f, 300f);
        float fixedY = 0f;

        string displayName = GetRandomDisplayNameByPiece(piece);
        Color color = RandomClothingColor();
        string material = RandomMaterial();
        string style = RandomStyle();
        char grade = RandomGrade();
        int cost = CalculateCost(piece, grade);

        if (!TryPayForSpawn(cost)) return;

        GameObject obj = InstantiateToCanvas(new Vector2(randomX, fixedY));
        var tp = obj.GetComponent<TopProperty>();
        if (tp != null)
        {
            tp.piece = piece;
            tp.displayName = displayName;
            tp.color = color;
            tp.material = (piece.ToLower() == "hat" || piece.ToLower() == "shoe") ? "N/A" : material;
            tp.style = style;
            tp.grade = grade;
            tp.cost = cost;
        }
    }

    public void SpawnWithOverrides(ClothingOverrides overrides = null)
    {
        string piece = RandomPiece();
        string displayName = GetRandomDisplayNameByPiece(piece);
        Color color = RandomClothingColor();
        string material = RandomMaterial();
        string style = RandomStyle();
        char grade = RandomGrade();

        if (overrides != null)
        {
            if (overrides.overridePiece) piece = overrides.piece;
            if (overrides.overrideDisplayName) displayName = overrides.displayName;
            if (overrides.overrideColor) color = overrides.color;
            if (overrides.overrideMaterial) material = overrides.material;
            if (overrides.overrideStyle) style = overrides.style;
            if (overrides.overrideGrade) grade = overrides.grade;
        }

        int cost = CalculateCost(piece, grade);
        if (!TryPayForSpawn(cost)) return;

        float randomX = Random.Range(-300f, 300f);
        GameObject obj = InstantiateToCanvas(new Vector2(randomX, 0f));
        SetPresetProperties(obj, piece, displayName, color, material, style, grade, cost);
    }

    public void SpawnPresetOnCanvas()
    {
        float randomX = Random.Range(-300f, 300f);
        int cost = CalculateCost("Top", 'A');
        if (!TryPayForSpawn(cost)) return;

        GameObject obj = InstantiateToCanvas(new Vector2(randomX, 0f));
        SetPresetProperties(obj, "Top", "Basic shirt", Color.white, "Cotton", "Modern", 'A', cost);
    }

    private GameObject InstantiateToCanvas(Vector2 anchoredPosition)
    {
        GameObject obj = Instantiate(prefabToSpawn);
        obj.transform.SetParent(targetCanvas.transform, false);
        var rt = obj.GetComponent<RectTransform>();
        if (rt != null) rt.anchoredPosition = anchoredPosition;
        return obj;
    }

    private string RandomPiece()
    {
        string[] pieces = { "top", "bottom", "hat", "shoe", "full" };
        return pieces[Random.Range(0, pieces.Length)];
    }

    private int CalculateCost(string piece, char grade)
    {
        int baseCost;
        switch (piece.ToLower())
        {
            case "top": baseCost = topCost; break;
            case "bottom": baseCost = bottomCost; break;
            case "hat": baseCost = hatCost; break;
            case "shoe": baseCost = shoeCost; break;
            case "full": baseCost = fullOutfitCost; break;
            default: baseCost = topCost; break;
        }

        switch (char.ToUpper(grade))
        {
            case 'A': return baseCost + gradeAExtraCost;
            case 'B': return baseCost + gradeBExtraCost;
            case 'C': return baseCost + gradeCExtraCost;
            default: return baseCost;
        }
    }

    private bool TryPayForSpawn(int cost)
    {
        if (!requireBudget || ChapterBudgetManager.Instance == null) return true;
        return ChapterBudgetManager.Instance.TrySpend(cost);
    }

    public void SpawnRandomTop() => SpawnRandomOnCanvas("top");
    public void SpawnRandomBottom() => SpawnRandomOnCanvas("bottom");
    public void SpawnRandomHat() => SpawnRandomOnCanvas("hat");
    public void SpawnRandomShoe() => SpawnRandomOnCanvas("shoe");
    public void SpawnRandomFull() => SpawnRandomOnCanvas("full");
    public void SpawnRandomAny() => SpawnRandomOnCanvas(RandomPiece());
}