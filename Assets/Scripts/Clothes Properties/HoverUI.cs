using UnityEngine;
using UnityEngine.UI;
using TMPro; // If you use TMP
using UnityEngine.EventSystems;

public class HoverUI : MonoBehaviour
{
    public GameObject propertyPanel;
    private Vector3 targetPosition;
    private bool isShowing = false;
    private RectTransform currentRect;
        public TextMeshProUGUI pieceText, nameText, materialText, styleText, gradeText;
    public Image colorImage;

    void Update()
    {
    if (isShowing && currentRect != null)
    {
        UpdateTargetPosition(currentRect); // Always update target position
        propertyPanel.transform.position = Vector3.Lerp(
            propertyPanel.transform.position, targetPosition, Time.deltaTime * 12f);
    }
    }

    public void Show(TopProperty tp, RectTransform hangerRect)
    {
    
        pieceText.text = $"Type: {tp.piece}";
        nameText.text     = $"Name: {tp.displayName}";
        materialText.text = $"Material: {tp.material}";
        styleText.text    = $"Style: {tp.style}";
        gradeText.text    = $"Grade: {tp.grade}";
        colorImage.color  = tp.color;

    currentRect = hangerRect;
    UpdateTargetPosition(currentRect);
    propertyPanel.transform.position = targetPosition; // Instantly snap
    isShowing = true;
    propertyPanel.SetActive(true);
    }

    void UpdateTargetPosition(RectTransform hangerRect)
{
    Vector3[] corners = new Vector3[4];
    hangerRect.GetWorldCorners(corners);
    targetPosition = (corners[1] + corners[2]) / 2f;
    targetPosition.y += 40f;
}

    public void Hide()
    {
     isShowing = false;
    propertyPanel.transform.position = new Vector2(-10000, -10000);

    currentRect = null;
    }
}