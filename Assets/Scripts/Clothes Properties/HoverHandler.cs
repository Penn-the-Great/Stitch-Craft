using UnityEngine;
using UnityEngine.EventSystems;

public class HoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TopProperty tp;

    void Awake()
    {
        tp = GetComponent<TopProperty>();
        Debug.Log($"[HoverHandler] Awake - tp assigned? {tp != null}");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GameObject propertyPanel = GameObject.FindGameObjectWithTag("HoverUI");
        Debug.Log("Enter");

        if (propertyPanel != null)
        {
            var hoverUI = propertyPanel.GetComponent<HoverUI>();
            if (hoverUI != null && tp != null)
            {
                hoverUI.Show(tp, GetComponent<RectTransform>());
            }
            else
            {
                Debug.LogWarning("HoverUI component or TopProperty (tp) is null.");
            }
        }
        else
        {
            Debug.LogWarning("No GameObject found with tag 'HoverUI'");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameObject propertyPanel = GameObject.FindGameObjectWithTag("HoverUI");

        if (propertyPanel != null)
        {
            var hoverUI = propertyPanel.GetComponent<HoverUI>();
            if (hoverUI != null)
            {
                hoverUI.Hide();
            }
            else
            {
                Debug.LogWarning("HoverUI component is null.");
            }
        }
        else
        {
            Debug.LogWarning("No GameObject found with tag 'HoverUI'");
        }
    }
}