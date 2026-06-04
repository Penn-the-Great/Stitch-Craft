using UnityEngine;

public class DeskPurchaseButtons : MonoBehaviour
{
    public void BuyCheapCostume()
    {
        if (DeskPurchaseManager.Instance != null)
            DeskPurchaseManager.Instance.BuyCheapCostume();
    }

    public void BuyExpensiveCostume()
    {
        if (DeskPurchaseManager.Instance != null)
            DeskPurchaseManager.Instance.BuyExpensiveCostume();
    }

    public void BuyFabricBundle()
    {
        if (DeskPurchaseManager.Instance != null)
            DeskPurchaseManager.Instance.BuyFabricBundle();
    }
}
