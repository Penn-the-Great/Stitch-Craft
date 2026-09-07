using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Storefront button: shows color swatch and icon for clothing pieces.
/// Make sure iconImage and pieceIcons are assigned in the inspector.
/// </summary>
public class StorefrontOfferButton : MonoBehaviour
{
    public enum OfferType { Clothing, Fabric }

    [SerializeField] private OfferType offerType;
    [SerializeField] private int offerIndex;
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text offerText;
    [SerializeField] private Image colorImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private Sprite[] pieceIcons; // order: top, bottom, hat, shoe, full
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private string emptyText = "No offer";

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable() => Refresh();

    public void BuyOffer()
    {
        if (DeskStorefrontManager.Instance == null) return;
        if (offerType == OfferType.Clothing) DeskStorefrontManager.Instance.BuyClothingOffer(offerIndex);
        else DeskStorefrontManager.Instance.BuyFabricOffer(offerIndex);
    }

    public void Refresh()
    {
        if (DeskStorefrontManager.Instance == null) { SetDisplay(emptyText, false); return; }
        if (offerType == OfferType.Clothing) RefreshClothingOffer(); else RefreshFabricOffer();
    }

    private void RefreshClothingOffer()
    {
        var offer = DeskStorefrontManager.Instance.GetClothingOffer(offerIndex);
        if (offer == null) { SetDisplay(emptyText, false); return; }

        if (colorImage != null)
        {
            Color c = offer.color;
            c.a = 1f;
            colorImage.color = c;
            colorImage.gameObject.SetActive(true);
        }

        if (iconImage != null)
        {
            Sprite s = GetSpriteForPiece(offer.piece);
            if (s != null) { iconImage.sprite = s; iconImage.gameObject.SetActive(true); }
            else iconImage.gameObject.SetActive(false);
        }

        string text = $"{offer.displayName}\n{offer.piece} | Grade {offer.grade}\n{offer.material} | {offer.style}\n${offer.cost} | {offer.deliveryWeeks} week delivery";
        SetDisplay(text, !offer.purchased);
    }

    private void RefreshFabricOffer()
    {
        var offer = DeskStorefrontManager.Instance.GetFabricOffer(offerIndex);
        if (offer == null) { SetDisplay(emptyText, false); return; }

        if (colorImage != null)
        {
            Color c = offer.color;
            c.a = 1f;
            colorImage.color = c;
            colorImage.gameObject.SetActive(true);
        }

        if (iconImage != null) iconImage.gameObject.SetActive(false);
        SetDisplay($"{offer.displayName}\n${offer.cost}", !offer.purchased);
    }

    private void SetDisplay(string text, bool canBuy)
    {
        if (offerText != null) offerText.text = text;
        if (colorImage != null && text == emptyText) colorImage.gameObject.SetActive(false);
        if (button != null) button.interactable = canBuy;
        if (canvasGroup != null) canvasGroup.alpha = canBuy ? 1f : 0.45f;
    }

    private Sprite GetSpriteForPiece(string piece)
    {
        if (pieceIcons == null || pieceIcons.Length == 0) return null;
        switch ((piece ?? "").ToLower())
        {
            case "top": return pieceIcons.Length > 0 ? pieceIcons[0] : null;
            case "bottom": return pieceIcons.Length > 1 ? pieceIcons[1] : null;
            case "hat": return pieceIcons.Length > 2 ? pieceIcons[2] : null;
            case "shoe": return pieceIcons.Length > 3 ? pieceIcons[3] : null;
            case "full": return pieceIcons.Length > 4 ? pieceIcons[4] : null;
            default: return null;
        }
    }
}