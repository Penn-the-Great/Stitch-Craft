using UnityEngine;
using UnityEngine.UI;
using TMPro; // If you use TMP
using UnityEngine.EventSystems;
using System;

public class HoverUI : MonoBehaviour
{
    public GameObject propertyPanel;
    private Vector3 targetPosition;
    private bool isShowing = false;
    private RectTransform currentRect;
    public TextMeshProUGUI pieceText, nameText, materialText, styleText, gradeText;

    [Serializable]
    public class PieceSprite
    {
        public string piece;
        public Sprite sprite;
    }

    [Header("Color Preview")]
    public Image colorImage;
    public PieceSprite[] pieceSprites;
    public Sprite defaultColorSprite;
    private Sprite originalColorSprite;

    void Awake()
    {
        if (colorImage != null)
            originalColorSprite = colorImage.sprite;
    }

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
        UpdateColorImage(tp);

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

    private void UpdateColorImage(TopProperty tp)
    {
        if (colorImage == null || tp == null) return;

        colorImage.color = tp.color;
        colorImage.sprite = GetSpriteForPiece(tp.piece);
    }

    private Sprite GetSpriteForPiece(string piece)
    {
        if (pieceSprites != null)
        {
            foreach (PieceSprite pieceSprite in pieceSprites)
            {
                if (pieceSprite == null || pieceSprite.sprite == null) continue;
                if (string.Equals(pieceSprite.piece, piece, StringComparison.OrdinalIgnoreCase))
                    return pieceSprite.sprite;
            }
        }

        return defaultColorSprite != null ? defaultColorSprite : originalColorSprite;
    }
}
