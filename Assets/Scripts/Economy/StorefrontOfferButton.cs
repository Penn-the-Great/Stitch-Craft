using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StorefrontOfferButton : MonoBehaviour
{
    public enum OfferType
    {
        Clothing,
        Fabric
    }

    [SerializeField] private OfferType offerType;
    [SerializeField] private int offerIndex;
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text offerText;
    [SerializeField] private Image colorImage;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private string emptyText = "No offer";

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void BuyOffer()
    {
        if (DeskStorefrontManager.Instance == null)
            return;

        if (offerType == OfferType.Clothing)
            DeskStorefrontManager.Instance.BuyClothingOffer(offerIndex);
        else
            DeskStorefrontManager.Instance.BuyFabricOffer(offerIndex);
    }

    public void Refresh()
    {
        if (DeskStorefrontManager.Instance == null)
        {
            SetDisplay(emptyText, false);
            return;
        }

        if (offerType == OfferType.Clothing)
            RefreshClothingOffer();
        else
            RefreshFabricOffer();
    }

    private void RefreshClothingOffer()
    {
        DeskStorefrontManager.ClothingOffer offer = DeskStorefrontManager.Instance.GetClothingOffer(offerIndex);
        if (offer == null)
        {
            SetDisplay(emptyText, false);
            return;
        }

        if (colorImage != null)
        {
            colorImage.gameObject.SetActive(true);
            colorImage.color = offer.color;
        }

        string text = $"{offer.displayName}\n{offer.piece} | Grade {offer.grade}\n{offer.material} | {offer.style}\n${offer.cost} | {offer.deliveryWeeks} week delivery";
        SetDisplay(text, !offer.purchased);
    }

    private void RefreshFabricOffer()
    {
        DeskStorefrontManager.FabricOffer offer = DeskStorefrontManager.Instance.GetFabricOffer(offerIndex);
        if (offer == null)
        {
            SetDisplay(emptyText, false);
            return;
        }

        string text = $"{offer.displayName}\n{offer.amount} fabric\n${offer.cost}";
        if (colorImage != null)
            colorImage.gameObject.SetActive(false);

        SetDisplay(text, !offer.purchased);
    }

    private void SetDisplay(string text, bool canBuy)
    {
        if (offerText != null)
            offerText.text = text;

        if (colorImage != null && text == emptyText)
            colorImage.gameObject.SetActive(false);

        if (button != null)
            button.interactable = canBuy;

        if (canvasGroup != null)
            canvasGroup.alpha = canBuy ? 1f : 0.45f;
    }
}
