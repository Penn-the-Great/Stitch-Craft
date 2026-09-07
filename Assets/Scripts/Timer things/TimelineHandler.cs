using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.SceneManagement;

public class TimelineHandler : MonoBehaviour
{
    public static TimelineHandler Instance { get; private set; }

    [Header("Core Settings")]
    public bool freePlayMode = false;

    [Header("Debug")]
    public bool debugMode = false;
    public float debugTimerSpeed = 1f;

    [Header("UI References")]
    public TMP_Text weekLabel;
    public GameObject timesUpGraphic;

    [Header("Slider Fader Transition")]
    public string nextSceneName;

    [Header("Navigation")]
    public ShopNavigation shopNavigation;

    [Header("Events")]
    public UnityEvent onPreWeekStart;
    public UnityEvent onWeekTimerStart;
    public UnityEvent onWeekEnd;
    public UnityEvent onChapterEnd;
    public UnityEvent onFreeplayEnd;

    private int chapter = 1;
    private int currentWeek = 1;
    private int weeksThisChapter = 6;
    private float weekLengthSeconds = 8 * 60f;
    private float timer = 0f;
    private float totalChapterTime = 0f;
    private bool timerRunning = false;
    private bool waitingToStartWeek = false;
    private Coroutine timesUpCoroutine;
    private bool weekEndTriggered = false;
    private float lastDebugLogTime = 0f;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        if (PersistentGameData.Instance != null)
        {
            chapter = PersistentGameData.Instance.selectedChapter;
        }

        // Auto-find ShopNavigation if not assigned
        if (shopNavigation == null)
            shopNavigation = FindObjectOfType<ShopNavigation>();

        SetWeeksForChapter();
        PrepareChapter();
    }

    void Update()
    {
        if (!timerRunning) return;
        float debugSpeed = debugMode ? (PersistentGameData.Instance?.debugTimerSpeed ?? debugTimerSpeed) : 1f;
         float deltaTime = debugMode ? Time.unscaledDeltaTime * debugSpeed : Time.unscaledDeltaTime;
        timer -= deltaTime;
        totalChapterTime += deltaTime;


    if (timer <= 0f && !weekEndTriggered)
    {
        weekEndTriggered = true;
        timer = 0f;
        if (DeskPurchaseManager.Instance != null)
            DeskPurchaseManager.Instance.AdvanceDeliveriesOneWeek();

        onWeekEnd?.Invoke();

        if (timesUpCoroutine != null)
            StopCoroutine(timesUpCoroutine);

        timesUpCoroutine = StartCoroutine(DoWeekEndAndContinue());
    }

        if (debugMode)
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                timer = 0f;
                Debug.Log("⏱️ DEBUG: Timer forced to end");
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                PrepareChapter();
                Debug.Log("🔄 DEBUG: Chapter restarted");
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                if (currentWeek < weeksThisChapter)
                {
                    currentWeek++;
                    UpdateWeekLabel();
                    RefreshWeeklySystems();
                    timer = weekLengthSeconds;
                    Debug.Log($"⏭️ DEBUG: Advanced to Week {currentWeek}");
                }
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
            currentWeek = weeksThisChapter - 1;
            UpdateWeekLabel();
            RefreshWeeklySystems();
            timer = weekLengthSeconds;
            Debug.Log($"🏁 DEBUG: Skipped to last week (Week {currentWeek})");
            }
        }
    }

public void TransitionToNextScene()
{
    if (!string.IsNullOrEmpty(nextSceneName))
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene(nextSceneName, LoadSceneMode.Single);
            Debug.Log($"🚪 Transitioning to next scene: {nextSceneName}");
        }
        else
        {
            Debug.LogError("❌ SceneTransitionManager.Instance is NULL! Make sure it exists in the scene.");
            // Fallback: Load scene directly
            SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
        }
    }
    else
    {
        Debug.LogWarning("⚠️ nextSceneName is not set in TimelineHandler!");
    }
}


    IEnumerator DoWeekEndAndContinue()
    {
        bool isLastWeek = (currentWeek >= weeksThisChapter);

        if (isLastWeek)
        {
           if (timesUpGraphic) 
        {
            timesUpGraphic.SetActive(true);
            Debug.Log("✅ Graphic activated");
        }
        else
            Debug.LogError("❌ timesUpGraphic is NULL before wait!");
        
        Debug.Log("⏳ Starting 4 second wait...");
        yield return new WaitForSeconds(4f);
        Debug.Log("✅ Wait completed");
        
        if (timesUpGraphic) 
        {
            timesUpGraphic.SetActive(false);
            Debug.Log("✅ Graphic deactivated");
        }
        else
            Debug.LogWarning("⚠️ timesUpGraphic is NULL after wait!");

            Debug.Log("📅 Chapter ended");
            timerRunning = false;
            onChapterEnd?.Invoke();
            
            if (freePlayMode)
            {
            onFreeplayEnd?.Invoke(); 
            }
                    else
        {
            // Transition to next scene after chapter ends
            TransitionToNextScene();
        }

        }
        else
        {
            currentWeek++;
            UpdateWeekLabel();
            RefreshWeeklySystems();
            timer = weekLengthSeconds;
            weekEndTriggered = false; 
            onWeekTimerStart?.Invoke();
        }
    }

    public void PrepareChapter()
    {
        SetWeeksForChapter();
        if (ChapterBudgetManager.Instance != null)
            ChapterBudgetManager.Instance.BeginChapter(chapter);

        currentWeek = 1;
        totalChapterTime = 0f;
        UpdateWeekLabel();
        RefreshWeeklySystems();
        waitingToStartWeek = true;
        timerRunning = false;
        weekEndTriggered = false;
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

    private void RefreshWeeklySystems()
    {
        if (DeskStorefrontManager.Instance != null)
            DeskStorefrontManager.Instance.GenerateWeeklyOffers(chapter, currentWeek);

        if (DeskCalendarManager.Instance != null)
            DeskCalendarManager.Instance.RefreshCalendarDisplay();
    }

    public float GetTimeLeftThisWeek() => timer;
    public int GetCurrentWeek() => currentWeek;
    public int GetCurrentChapter() => chapter;
    public int GetWeeksThisChapter() => weeksThisChapter;
    public float GetTotalChapterTime() => totalChapterTime;
}
