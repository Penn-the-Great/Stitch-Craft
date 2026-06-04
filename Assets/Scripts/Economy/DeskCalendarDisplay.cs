using TMPro;
using UnityEngine;

public class DeskCalendarDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text calendarText;

    private void Awake()
    {
        if (calendarText == null)
            calendarText = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (DeskCalendarManager.Instance != null)
            DeskCalendarManager.Instance.SetCalendarText(calendarText);
    }
}
