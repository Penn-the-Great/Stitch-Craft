using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FabricOptionButton : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private Image colorSwatch;
    [SerializeField] private GameObject checkmark;
    [SerializeField] private Button button;

    private FabricInventoryManager.FabricStack fabric;
    private Action<FabricInventoryManager.FabricStack> onClicked;

    public void Setup(FabricInventoryManager.FabricStack fabric, bool isSelected, Action<FabricInventoryManager.FabricStack> onClicked)
    {
        this.fabric = fabric;
        this.onClicked = onClicked;

        if (label != null)
            label.text = fabric.material;

        if (colorSwatch != null)
        {
            Color visibleColor = fabric.color;
            visibleColor.a = 1f;
            colorSwatch.color = visibleColor;
            colorSwatch.gameObject.SetActive(true);
        }

        if (checkmark != null)
            checkmark.SetActive(isSelected);

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => this.onClicked?.Invoke(this.fabric));
        }
    }
}