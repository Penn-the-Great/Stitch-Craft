using TMPro; // Gives access to TMP_Dropdown and TMP_Text.
using UnityEngine; // Unity basics.
using UnityEngine.UI; // Gives access to Button, Image, Transform.

public class StitchingTableMenu : MonoBehaviour // Controls the stitching table creation menu.
{
    [SerializeField] private TMP_Dropdown pieceDropdown; // Top, bottom, hat, shoe, full.
    [SerializeField] private TMP_Dropdown typeDropdown; // Basic shirt, sweater, jeans, etc.
    [SerializeField] private TMP_Dropdown styleDropdown; // Modern, fantasy, 1920s, etc.

    [SerializeField] private Transform fabricListParent; // Where fabric buttons spawn.
    [SerializeField] private FabricOptionButton fabricButtonPrefab; // Button prefab for one fabric.
    [SerializeField] private float fabricButtonHeight = 48f; // Height for each fabric row.

    [Header("Crafted Clothing Spawn")]
    [SerializeField] private GameObject hangerPrefab; // Hanger prefab to spawn after crafting.
    [SerializeField] private Canvas workspaceCanvas; // Canvas where crafted clothing should appear.
    [SerializeField] private Vector2 craftedSpawnPosition = Vector2.zero; // Spawn position on the workspace canvas.
    [SerializeField] private StitchingMinigameManager minigameManager;

private CanvasGroup workspaceCanvasGroup;
private float previousWorkspaceAlpha = 1f;
private bool previousInteractable = true;
private bool previousBlocksRaycasts = true;

    private string pendingPiece;
private string pendingDisplayName;
private string pendingStyle;
private string pendingMaterial;
private Color pendingColor;
private int pendingCost;
private bool hasPendingCraft;

    private TopProperty pendingCraftedItem;
    [SerializeField] private Button createButton; // Button that creates clothing.
    private FabricInventoryManager.FabricStack selectedFabric; // Currently selected fabric.

    private void Start() // Runs when menu opens.
    {
        pieceDropdown.onValueChanged.AddListener(OnPieceChanged); // Update type menu when piece changes.
        createButton.onClick.AddListener(CreateClothing); // Run CreateClothing when clicked.

        FillPieceDropdown(); // Add piece options.
        FillStyleDropdown(); // Add style options.
        OnPieceChanged(0); // Fill type options for first piece.
        ConfigureFabricListLayout(); // Make fabric buttons stack from the top with no gaps.
        RefreshFabricList(); // Show owned fabrics.
    }

    private void OnEnable()
{
    CacheWorkspaceCanvasGroup();
    SetWorkspaceCanvasVisible(false);
}

private void OnDisable()
{
    SetWorkspaceCanvasVisible(true);
}

private void CacheWorkspaceCanvasGroup()
{
    if (workspaceCanvas == null)
        workspaceCanvas = GetWorkspaceCanvas();

    if (workspaceCanvas == null) return;

    workspaceCanvasGroup = workspaceCanvas.GetComponent<CanvasGroup>();
    if (workspaceCanvasGroup == null)
        workspaceCanvasGroup = workspaceCanvas.gameObject.AddComponent<CanvasGroup>();
}

private void SetWorkspaceCanvasVisible(bool visible)
{
    if (workspaceCanvas == null)
        workspaceCanvas = GetWorkspaceCanvas();

    if (workspaceCanvas == null) return;

    if (workspaceCanvasGroup == null)
        workspaceCanvasGroup = workspaceCanvas.GetComponent<CanvasGroup>() ?? workspaceCanvas.gameObject.AddComponent<CanvasGroup>();

    if (!visible)
    {
        previousWorkspaceAlpha = workspaceCanvasGroup.alpha;
        previousInteractable = workspaceCanvasGroup.interactable;
        previousBlocksRaycasts = workspaceCanvasGroup.blocksRaycasts;

        workspaceCanvasGroup.alpha = 0f;
        workspaceCanvasGroup.interactable = false;
        workspaceCanvasGroup.blocksRaycasts = false;
    }
    else
    {
        workspaceCanvasGroup.alpha = previousWorkspaceAlpha <= 0f ? 1f : previousWorkspaceAlpha;
        workspaceCanvasGroup.interactable = previousInteractable;
        workspaceCanvasGroup.blocksRaycasts = previousBlocksRaycasts;
    }
}


    private void FillPieceDropdown() // Adds piece choices.
    {
        pieceDropdown.ClearOptions(); // Remove old options.
        pieceDropdown.AddOptions(new System.Collections.Generic.List<string> { "top", "bottom", "hat", "shoe", "full" }); // Add new ones.
    }

    private void FillStyleDropdown() // Adds style choices.
    {
        styleDropdown.ClearOptions(); // Remove old styles.
        styleDropdown.AddOptions(new System.Collections.Generic.List<string> { "Modern", "Fantasy", "Ren", "1920's", "1960's", "Futuristic" }); // Add styles.
    }

    private void OnPieceChanged(int index) // Runs when player chooses top/bottom/etc.
    {
        string piece = pieceDropdown.options[index].text; // Get selected piece text.
        typeDropdown.ClearOptions(); // Clear old clothing types.

        if (piece == "top") typeDropdown.AddOptions(new System.Collections.Generic.List<string> { "Basic shirt", "Button up", "Sweater", "Blouse" }); // Top types.
        if (piece == "bottom") typeDropdown.AddOptions(new System.Collections.Generic.List<string> { "Jeans", "Slacks", "Shorts", "Skirt" }); // Bottom types.
        if (piece == "hat") typeDropdown.AddOptions(new System.Collections.Generic.List<string> { "Beret", "Fedora", "Top Hat", "Sun Hat" }); // Hat types.
        if (piece == "shoe") typeDropdown.AddOptions(new System.Collections.Generic.List<string> { "Sneakers", "Boots", "Loafers", "Heels" }); // Shoe types.
        if (piece == "full") typeDropdown.AddOptions(new System.Collections.Generic.List<string> { "Dress", "Jumpsuit", "Robe", "Suit Set" }); // Full outfit types.
    }

    private void RefreshFabricList() // Rebuilds fabric buttons.
    {
        foreach (Transform child in fabricListParent) Destroy(child.gameObject); // Delete old buttons.

        foreach (var fabric in FabricInventoryManager.Instance.Fabrics) // Loop through owned fabrics.
        {
            FabricOptionButton button = Instantiate(fabricButtonPrefab, fabricListParent); // Spawn a fabric button.
            button.transform.localScale = Vector3.one; // Keep prefab scale normal after spawning.
            SetFabricButtonHeight(button); // Keep each row a predictable height.
            button.Setup(fabric, selectedFabric == fabric, SelectFabric); // Give it fabric data and click behavior.
        }
    }

    private void SelectFabric(FabricInventoryManager.FabricStack fabric) // Runs when fabric is clicked.
    {
        selectedFabric = fabric; // Remember selected fabric.
        RefreshFabricList(); // Refresh checkmarks.
    }

private void CreateClothing()
{
    if (FabricInventoryManager.Instance == null) return;
    if (selectedFabric == null) return;

    string selectedMaterial = selectedFabric.material;
    Color selectedColor = selectedFabric.color;
    selectedColor.a = 1f;

    // Spend ONCE
    if (!FabricInventoryManager.Instance.TryUseFabric(selectedFabric)) return;

    // Save pending craft data
    pendingPiece = pieceDropdown.options[pieceDropdown.value].text;
    pendingDisplayName = typeDropdown.options[typeDropdown.value].text;
    pendingStyle = styleDropdown.options[styleDropdown.value].text;
    pendingMaterial = selectedMaterial;
    pendingColor = selectedColor;
    pendingCost = 0;

    hasPendingCraft = true;
    selectedFabric = null;
    RefreshFabricList();
}

public void ApplyFinalGradeAndSpawn(char finalGrade)
{
    if (!hasPendingCraft) return;
    if (hangerPrefab == null) return;

    if (workspaceCanvas == null)
        workspaceCanvas = GetWorkspaceCanvas();

    if (workspaceCanvas == null) return;

    GameObject craftedHanger = Instantiate(hangerPrefab, workspaceCanvas.transform);
    RectTransform craftedRect = craftedHanger.GetComponent<RectTransform>();
    if (craftedRect != null)
        craftedRect.anchoredPosition = craftedSpawnPosition;

    TopProperty item = craftedHanger.GetComponent<TopProperty>();
    if (item != null)
    {
        item.piece = pendingPiece;
        item.displayName = pendingDisplayName;
        item.style = pendingStyle;
        item.material = pendingMaterial;
        item.color = pendingColor;
        item.grade = finalGrade;
        item.cost = pendingCost;
    }

    hasPendingCraft = false;
}

    private void ConfigureFabricListLayout() // Sets layout behavior without resizing the parent transform.
    {
        if (fabricListParent == null) return; // Stop if no parent was assigned.

        VerticalLayoutGroup layoutGroup = fabricListParent.GetComponent<VerticalLayoutGroup>(); // Look for layout.
        if (layoutGroup == null)
            layoutGroup = fabricListParent.gameObject.AddComponent<VerticalLayoutGroup>(); // Add one if missing.

        layoutGroup.childAlignment = TextAnchor.UpperCenter; // Stack from the top.
        layoutGroup.spacing = 0f; // No gaps between rows.
        layoutGroup.childControlWidth = false; // Do not change child width.
        layoutGroup.childControlHeight = false; // Use the row height from each button's LayoutElement.
        layoutGroup.childForceExpandWidth = false; // Do not stretch children horizontally.
        layoutGroup.childForceExpandHeight = false; // Do not stretch children vertically.
    }

    private void SetFabricButtonHeight(FabricOptionButton button) // Gives each fabric row a consistent height.
    {
        LayoutElement layoutElement = button.GetComponent<LayoutElement>(); // Look for row layout settings.
        if (layoutElement == null)
            layoutElement = button.gameObject.AddComponent<LayoutElement>(); // Add settings if missing.

        layoutElement.preferredHeight = fabricButtonHeight; // Use the height from the Inspector.
        layoutElement.minHeight = fabricButtonHeight; // Prevent rows from collapsing smaller.
    }

    private Canvas GetWorkspaceCanvas() // Finds a non-storage canvas in the active scene.
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>(); // Get all canvases.
        foreach (Canvas canvas in canvases) // Check each canvas.
        {
            if (!canvas.CompareTag("StorageCanvas")) // Skip the storage canvas.
                return canvas; // Use the first normal workspace canvas.
        }

        return null; // No workspace canvas found.
    }
}
