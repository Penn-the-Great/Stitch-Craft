using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StitchingTableMenu : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown pieceDropdown;
    [SerializeField] private TMP_Dropdown typeDropdown;
    [SerializeField] private TMP_Dropdown styleDropdown;

    [SerializeField] private Transform fabricListParent;
    [SerializeField] private FabricOptionButton fabricButtonPrefab;
    [SerializeField] private float fabricButtonHeight = 48f;

    [Header("Crafted Clothing Spawn")]
    [SerializeField] private GameObject hangerPrefab;
    [SerializeField] private Canvas workspaceCanvas;
    [SerializeField] private Vector2 craftedSpawnPosition = Vector2.zero;
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
    [SerializeField] private Button createButton;
    private FabricInventoryManager.FabricStack selectedFabric;

    private void Start()
    {
        pieceDropdown.onValueChanged.AddListener(OnPieceChanged);
        createButton.onClick.AddListener(CreateClothing);

        FillPieceDropdown();
        FillStyleDropdown();
        OnPieceChanged(0);
        ConfigureFabricListLayout();
        RefreshFabricList();
        UpdateCreateButtonInteractability();
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


    private void FillPieceDropdown()
    {
        pieceDropdown.ClearOptions();
        pieceDropdown.AddOptions(new System.Collections.Generic.List<string> { "top", "bottom", "hat", "shoe", "full" });
    }

    private void FillStyleDropdown()
    {
        styleDropdown.ClearOptions();
        styleDropdown.AddOptions(new System.Collections.Generic.List<string> { "Modern", "Fantasy", "Ren", "1920's", "1960's", "Futuristic" });
    }

    private void OnPieceChanged(int index)
    {
        string piece = pieceDropdown.options[index].text;
        typeDropdown.ClearOptions();

        if (piece == "top") typeDropdown.AddOptions(new System.Collections.Generic.List<string> { "Basic shirt", "Button up", "Sweater", "Blouse" });
        if (piece == "bottom") typeDropdown.AddOptions(new System.Collections.Generic.List<string> { "Jeans", "Slacks", "Shorts", "Skirt" });
        if (piece == "hat") typeDropdown.AddOptions(new System.Collections.Generic.List<string> { "Beret", "Fedora", "Top Hat", "Sun Hat" });
        if (piece == "shoe") typeDropdown.AddOptions(new System.Collections.Generic.List<string> { "Sneakers", "Boots", "Loafers", "Heels" });
        if (piece == "full") typeDropdown.AddOptions(new System.Collections.Generic.List<string> { "Dress", "Jumpsuit", "Robe", "Suit Set" });
    }

    private void RefreshFabricList()
    {
        foreach (Transform child in fabricListParent) Destroy(child.gameObject);

        foreach (var fabric in FabricInventoryManager.Instance.Fabrics)
        {
            FabricOptionButton button = Instantiate(fabricButtonPrefab, fabricListParent);
            button.transform.localScale = Vector3.one;
            SetFabricButtonHeight(button);
            button.Setup(fabric, selectedFabric == fabric, SelectFabric);
        }
    }

    private void SelectFabric(FabricInventoryManager.FabricStack fabric)
    {
        selectedFabric = fabric;
        RefreshFabricList();
        UpdateCreateButtonInteractability();
    }

    private void UpdateCreateButtonInteractability()
    {
        if (createButton != null)
            createButton.interactable = selectedFabric != null;
    }

private void CreateClothing()
{
    if (FabricInventoryManager.Instance == null) return;
    if (selectedFabric == null) return;

    string selectedMaterial = selectedFabric.material;
    Color selectedColor = selectedFabric.color;
    selectedColor.a = 1f;

    if (!FabricInventoryManager.Instance.TryUseFabric(selectedFabric)) return;

    pendingPiece = pieceDropdown.options[pieceDropdown.value].text;
    pendingDisplayName = typeDropdown.options[typeDropdown.value].text;
    pendingStyle = styleDropdown.options[styleDropdown.value].text;
    pendingMaterial = selectedMaterial;
    pendingColor = selectedColor;
    pendingCost = 0;

    hasPendingCraft = true;
    selectedFabric = null;
    RefreshFabricList();
    UpdateCreateButtonInteractability();
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

    private void ConfigureFabricListLayout()
    {
        if (fabricListParent == null) return;

        VerticalLayoutGroup layoutGroup = fabricListParent.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
            layoutGroup = fabricListParent.gameObject.AddComponent<VerticalLayoutGroup>();

        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.spacing = 0f;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;
    }

    private void SetFabricButtonHeight(FabricOptionButton button)
    {
        LayoutElement layoutElement = button.GetComponent<LayoutElement>();
        if (layoutElement == null)
            layoutElement = button.gameObject.AddComponent<LayoutElement>();

        layoutElement.preferredHeight = fabricButtonHeight;
        layoutElement.minHeight = fabricButtonHeight;
    }

    private Canvas GetWorkspaceCanvas()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            if (!canvas.CompareTag("StorageCanvas"))
                return canvas;
        }

        return null;
    }
}
