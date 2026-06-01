using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System.Collections; // Note: For coroutine support

public class TimelineHandler : MonoBehaviour
{
    [Header("Core Settings")]
    public int chapter = 1;
    public bool freePlayMode = false;

    [Header("UI References")]
    public TMP_Text weekLabel;
    public GameObject timesUpGraphic;

    [Header("Slider Fader Transition")]
    public GameObject sliderFaderPrefab; // assign as in your SliderPrefabSpawner
    public string nextSceneName;         // set as in your spawner

    [Header("Events")]
    public UnityEvent onPreWeekStart;    // show dialogue before timer
    public UnityEvent onWeekTimerStart;
    public UnityEvent onWeekEnd;
    public UnityEvent onChapterEnd;

    private int currentWeek = 1;
    private int weeksThisChapter = 6;
    private float weekLengthSeconds = 8 * 60f;
    private float timer = 0f;
    private bool timerRunning = false;
    private bool waitingToStartWeek = false;

    void Start()
    {
        SetWeeksForChapter();
        PrepareChapter();
    }

    void Update()
    {
        if (!timerRunning) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = 0f;
            timerRunning = false;

            // Week (timer) finished
            onWeekEnd?.Invoke();

            // --- TIME'S UP LOGIC ---
            if (timesUpGraphic) timesUpGraphic.SetActive(true);
            StartCoroutine(DoTimesUpAndMaybeTransition());
        }
    }

    IEnumerator DoTimesUpAndMaybeTransition()
    {
        // Show times up for 2 seconds (or adjust duration)
        yield return new WaitForSeconds(2f);

        if (timesUpGraphic) timesUpGraphic.SetActive(false);

        // If end of chapter, transition scenes; otherwise, next week logic
        if (currentWeek < weeksThisChapter)
        {
            currentWeek++;
            UpdateWeekLabel();
            waitingToStartWeek = true;
        }
        else
        {
            onChapterEnd?.Invoke();
            if (!freePlayMode)
            {
                // Scene transition using prefab & nextSceneName
                if (sliderFaderPrefab && !string.IsNullOrEmpty(nextSceneName))
                {
                    var canvas = FindObjectOfType<Canvas>();
                    var faderObj = Instantiate(sliderFaderPrefab, canvas ? canvas.transform : null);
                    var fader = faderObj.GetComponent<SceneSlider>();
                    fader.BeginTransition(nextSceneName);
                }
                // If you want to also increase the chapter before transition:
                // chapter++;
                // PrepareChapter(); // Optional: if returning to menu to replay
            }
        }
    }

    public void PrepareChapter()
    {
        SetWeeksForChapter();
        currentWeek = 1;
        UpdateWeekLabel();
        waitingToStartWeek = true;
        timerRunning = false;
        onPreWeekStart?.Invoke();
    }

    public void StartWeekTimer()
    {
        timer = weekLengthSeconds;
        timerRunning = true;
        waitingToStartWeek = false;
        onWeekTimerStart?.Invoke();
    }

    public void RestartFreePlayTimer()
    {
        if (freePlayMode)
            PrepareChapter();
    }

    private void SetWeeksForChapter()
    {
        weeksThisChapter = (chapter == 10) ? 3 : 6;
    }

    private void UpdateWeekLabel()
    {
        if (weekLabel)
            weekLabel.text = $"Week {currentWeek}";
    }

    public float GetTimeLeftThisWeek() => timer;
    public int GetCurrentWeek() => currentWeek;
}