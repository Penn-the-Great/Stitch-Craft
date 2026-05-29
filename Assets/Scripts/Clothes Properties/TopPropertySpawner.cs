using UnityEngine;
using UnityEngine.SceneManagement; // Only needed if you want to target a specific scene

public class TopPropertySpawner : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public Canvas targetCanvas;
    private string[] topNames = { "Tank Top", "Vest Top", "Button up", "Sweater", "Blouse", "Basic shirt" };
private string[] bottomNames = { "Jeans", "Slackd", "Shorts", "Harem Pants", "Skirt", "tights" };
private string[] hatNames = { "Cowboy hat", "Fedora", "Flat cap", "Beret", "Sun Hat", "Top Hat" };
private string[] shoeNames = { "Sneakers", "Boots", "Loafers", "Sandals", "Heels", "Fancy" };
private string[] fullNames = { "Jumpsuit", "Overall", "Dress", "Morph suit", "Robe", "Suit Set" };
   
    public void SpawnOnCanvas(Vector2 anchoredPosition)
    {
    GameObject obj = Instantiate(prefabToSpawn);
    obj.transform.SetParent(targetCanvas.transform, false); // Don't keep world position

    RectTransform rt = obj.GetComponent<RectTransform>();
    if (rt != null)
    {
        rt.anchoredPosition = anchoredPosition;
    }
    else
    {
        Debug.LogWarning("Instantiated object does not have a RectTransform! Make sure your prefab is a UI element.");
    }
 }

    public void SetPresetProperties(GameObject obj, string piece, string displayName, Color color, string material, string style, char grade)
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
            
            // Optionally call tp.ApplyProperties(tp) if needed by your UI
        }
    }


    // -- Random value helpers --
    private string RandomMaterial()
    {
        string[] mats = { "Cotton", "leather", "Wool", "fur", "Silk" };
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
            case "top":
                return topNames[Random.Range(0, topNames.Length)];
            case "bottom":
                return bottomNames[Random.Range(0, bottomNames.Length)];
            case "hat":
                return hatNames[Random.Range(0, hatNames.Length)];
            case "shoe":
                return shoeNames[Random.Range(0, shoeNames.Length)];
            case "full":
                return fullNames[Random.Range(0, fullNames.Length)];
            default:
                return "Clothing Item";
        }
    }


        public void SpawnRandomOnCanvas(string piece)
    {
            // These bounds should fit your Canvas UI width (try CanvasScaler reference resolution for guidance)
    float minX = -300f;
    float maxX = 300f;
    float randomX = Random.Range(minX, maxX);

    // Fixed Y (the row of hangers)
    float fixedY = 0f;

        GameObject obj = InstantiateToCanvas(new Vector2(randomX, fixedY));

            var tp = obj.GetComponent<TopProperty>();
        if (tp != null)
        {
            tp.displayName = GetRandomDisplayNameByPiece(piece);
            tp.color = Random.ColorHSV();
            tp.material = RandomMaterial();
            tp.style = RandomStyle();
            tp.grade = RandomGrade();
           
            // Optionally call tp.ApplyProperties(tp) if needed by your UI
        }
    }

        public void SpawnPresetOnCanvas()
    {

            float minX = -300f;
    float maxX = 300f;
    float randomX = Random.Range(minX, maxX);

    // Fixed Y (the row of hangers)
    float fixedY = 0f;

        GameObject obj = InstantiateToCanvas(new Vector2(randomX, fixedY)); // change position if needed
        SetPresetProperties(obj, "Top", "Basic shirt", Color.white, "Cotton", "Modern", 'A');
    }

        private GameObject InstantiateToCanvas(Vector2 anchoredPosition)
    {
        GameObject obj = Instantiate(prefabToSpawn);
        obj.transform.SetParent(targetCanvas.transform, false);
        var rt = obj.GetComponent<RectTransform>();
        if (rt != null)
            rt.anchoredPosition = anchoredPosition;
        return obj;
    }

        // Button hook examples:
    public void SpawnRandomTop()    { SpawnRandomOnCanvas("top"); }
    public void SpawnRandomBottom() { SpawnRandomOnCanvas("bottom"); }
    public void SpawnRandomHat()    { SpawnRandomOnCanvas("hat"); }
    public void SpawnRandomShoe()   { SpawnRandomOnCanvas("shoe"); }
    public void SpawnRandomFull()   { SpawnRandomOnCanvas("full"); }
}
