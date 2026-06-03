using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Button to cycle through sorting modes
/// </summary>
public class SortModeButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private bool debugMode = false;

    private enum SortMode
    {
        None,
        Color,
        Material,
        Style,
        Grade
    }

    private SortMode currentSortMode = SortMode.None;

    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(CycleSortMode);
        }

        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
        }

        UpdateButtonText();
    }

    private void CycleSortMode()
    {
        // Cycle to next sort mode
        currentSortMode = (SortMode)(((int)currentSortMode + 1) % 5);

        if (debugMode) Debug.Log($"Cycled to sort mode: {currentSortMode}");

        // Apply sort or exit sort mode
        if (currentSortMode == SortMode.None)
        {
            SortingManager.Instance.ExitSortMode();
            if (debugMode) Debug.Log("Exited sort mode");
        }
        else
        {
            if (!SortingManager.Instance.IsSortModeActive)
            {
                SortingManager.Instance.EnterSortMode();
            }

            // Sort by current mode
            switch (currentSortMode)
            {
                case SortMode.Color:
                    SortingManager.Instance.SortByColor();
                    break;
                case SortMode.Material:
                    SortingManager.Instance.SortByMaterial();
                    break;
                case SortMode.Style:
                    SortingManager.Instance.SortByStyle();
                    break;
                case SortMode.Grade:
                    SortingManager.Instance.SortByGrade();
                    break;
            }

            if (debugMode) Debug.Log($"Sorted by: {currentSortMode}");
        }

        UpdateButtonText();
    }

    private void UpdateButtonText()
    {
        if (buttonText == null) return;

        switch (currentSortMode)
        {
            case SortMode.None:
                buttonText.text = "Sort: Off";
                break;
            case SortMode.Color:
                buttonText.text = "Sort: Color";
                break;
            case SortMode.Material:
                buttonText.text = "Sort: Material";
                break;
            case SortMode.Style:
                buttonText.text = "Sort: Style";
                break;
            case SortMode.Grade:
                buttonText.text = "Sort: Grade";
                break;
        }
    }
}