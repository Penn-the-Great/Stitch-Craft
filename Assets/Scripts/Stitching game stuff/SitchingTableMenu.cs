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

    [SerializeField] private Button createButton; // Button that creates clothing.
    [SerializeField] private int fabricCost = 1; // How much fabric one clothing item costs.

    private FabricInventoryManager.FabricStack selectedFabric; // Currently selected fabric.

    private void Start() // Runs when menu opens.
    {
        pieceDropdown.onValueChanged.AddListener(OnPieceChanged); // Update type menu when piece changes.
        createButton.onClick.AddListener(CreateClothing); // Run CreateClothing when clicked.

        FillPieceDropdown(); // Add piece options.
        FillStyleDropdown(); // Add style options.
        OnPieceChanged(0); // Fill type options for first piece.
        RefreshFabricList(); // Show owned fabrics.
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
            button.Setup(fabric, selectedFabric == fabric, SelectFabric); // Give it fabric data and click behavior.
        }
    }

    private void SelectFabric(FabricInventoryManager.FabricStack fabric) // Runs when fabric is clicked.
    {
        selectedFabric = fabric; // Remember selected fabric.
        RefreshFabricList(); // Refresh checkmarks.
    }

    private void CreateClothing() // Creates final clothing item.
    {
        if (!FabricInventoryManager.Instance.TryUseFabric(selectedFabric)) return; // Spend fabric or fail.

        StorageManager.StoredClothingItem item = new StorageManager.StoredClothingItem(); // Make clothing data.
        item.piece = pieceDropdown.options[pieceDropdown.value].text; // Set piece.
        item.displayName = typeDropdown.options[typeDropdown.value].text; // Set clothing type/name.
        item.style = styleDropdown.options[styleDropdown.value].text; // Set style.
        item.material = selectedFabric.material; // Fabric decides material.
        item.color = selectedFabric.color; // Fabric decides color.
        item.grade = 'C'; // Temporary until minigame determines grade.
        item.cost = 0; // Crafted item did not cost money directly.

        StorageManager.Instance.AddItemToStorage(item); // Add result to storage.
        selectedFabric = null; // Clear selection.
        RefreshFabricList(); // Refresh fabric list after spending.
    }
}