using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class DeskCalendarManager : MonoBehaviour
{
    public static DeskCalendarManager Instance { get; private set; }

    [SerializeField] private TMP_Text calendarText;
    [SerializeField] private string emptyWeekText = "No events";

    private readonly List<CalendarEvent> calendarEvents = new List<CalendarEvent>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        RefreshCalendarDisplay();
    }

    public void SetCalendarText(TMP_Text newCalendarText)
    {
        calendarText = newCalendarText;
        RefreshCalendarDisplay();
    }

    public void AddDeliveryEvent(int chapter, int week, string eventName)
    {
        AddEvent(chapter, week, eventName);
    }

    public void AddFittingEvent(int chapter, int week, string eventName)
    {
        AddEvent(chapter, week, eventName);
    }

    public void AddEvent(int chapter, int week, string eventName)
    {
        calendarEvents.Add(new CalendarEvent
        {
            chapter = chapter,
            week = week,
            eventName = eventName
        });

        RefreshCalendarDisplay();
    }

    public void RefreshCalendarDisplay()
    {
        if (calendarText == null)
            return;

        int chapter = TimelineHandler.Instance != null ? TimelineHandler.Instance.GetCurrentChapter() : 1;
        int totalWeeks = TimelineHandler.Instance != null ? TimelineHandler.Instance.GetWeeksThisChapter() : 6;
        StringBuilder builder = new StringBuilder();

        for (int week = 1; week <= totalWeeks; week++)
        {
            builder.AppendLine($"Week {week}");

            bool hasEvent = false;
            for (int i = 0; i < calendarEvents.Count; i++)
            {
                CalendarEvent calendarEvent = calendarEvents[i];
                if (calendarEvent.chapter != chapter || calendarEvent.week != week)
                    continue;

                builder.AppendLine($"- {calendarEvent.eventName}");
                hasEvent = true;
            }

            if (!hasEvent)
                builder.AppendLine($"- {emptyWeekText}");
        }

        calendarText.text = builder.ToString();
    }

    [System.Serializable]
    private class CalendarEvent
    {
        public int chapter;
        public int week;
        public string eventName;
    }
}
