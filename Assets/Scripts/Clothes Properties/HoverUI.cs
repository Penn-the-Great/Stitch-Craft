using UnityEngine;
using UnityEngine.UI;
using TMPro; // If you use TMP
using UnityEngine.EventSystems;

public class HoverUI : MonoBehaviour
{
    public GameObject propertyPanel;
    private Vector3 targetPosition;
    private bool isShowing = false;

    void Update()
    {
        if (isShowing)
        {
            // Interpolate panel's position toward the target position (smooth)
            propertyPanel.transform.position = Vector3.Lerp(
                propertyPanel.transform.position,
                targetPosition,
                Time.deltaTime * 12f // 12 = speed, make higher for faster response
            );
        }
    }

    public void Show(TopProperty tp, RectTransform hangerRect)
    {
        // ... fill in text/colors ...

        Vector3[] corners = new Vector3[4];
        hangerRect.GetWorldCorners(corners);
        Vector3 topCenter = (corners[1] + corners[2]) / 2f;
        topCenter.y += 40f; // Offset above (tweak as needed)

        targetPosition = topCenter;
        isShowing = true;
        // Enable if using pointer alpha/canvas group etc.
        propertyPanel.SetActive(true);
    }

    public void Hide()
    {
        isShowing = false;
        propertyPanel.transform.position = new Vector2(-10000, -10000);
        // Or hide with alpha/canvas group as discussed previously
    }
}