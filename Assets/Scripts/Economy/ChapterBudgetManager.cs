using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ChapterBudgetManager : MonoBehaviour
{
    public static ChapterBudgetManager Instance { get; private set; }

    [Header("Chapter Budgets")]
    [SerializeField] private int[] chapterBudgets = { 120, 140, 160, 180, 200, 225, 250, 275, 300, 350 };
    [SerializeField] private int freePlayBudget = 9999;

    [Header("UI")]
    [SerializeField] private TMP_Text budgetLabel;
    [SerializeField] private bool autoFindBudgetLabel = true;
    [SerializeField] private string budgetLabelTag = "BudgetLabel";
    [SerializeField] private string budgetPrefix = "$";
    [SerializeField] private string budgetSuffix = "";
    [SerializeField] private string budgetFormat = "{0}{1}{2}";

    [Header("Events")]
    public UnityEvent<int> onBudgetChanged;
    public UnityEvent<int> onNotEnoughBudget;

    private int currentChapter = 1;
    private int startingBudget;
    private int remainingBudget;

    public int CurrentChapter => currentChapter;
    public int StartingBudget => startingBudget;
    public int RemainingBudget => remainingBudget;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        ReconnectBudgetLabel();
        int chapter = PersistentGameData.Instance != null ? PersistentGameData.Instance.selectedChapter : 1;
        BeginChapter(chapter);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ReconnectBudgetLabel();
        UpdateBudgetDisplay();
    }

    public void BeginChapter(int chapter)
    {
        currentChapter = chapter;
        startingBudget = GetBudgetForChapter(chapter);
        remainingBudget = startingBudget;
        UpdateBudgetDisplay();
        onBudgetChanged?.Invoke(remainingBudget);
    }

    public bool CanSpend(int amount)
    {
        return amount <= 0 || remainingBudget >= amount;
    }

    public bool TrySpend(int amount)
    {
        if (amount <= 0)
            return true;

        if (!CanSpend(amount))
        {
            onNotEnoughBudget?.Invoke(amount);
            return false;
        }

        remainingBudget -= amount;
        UpdateBudgetDisplay();
        onBudgetChanged?.Invoke(remainingBudget);
        return true;
    }

    public void Refund(int amount)
    {
        if (amount <= 0)
            return;

        remainingBudget = Mathf.Min(remainingBudget + amount, startingBudget);
        UpdateBudgetDisplay();
        onBudgetChanged?.Invoke(remainingBudget);
    }

    private int GetBudgetForChapter(int chapter)
    {
        if (chapter == 11)
            return freePlayBudget;

        int index = Mathf.Clamp(chapter - 1, 0, chapterBudgets.Length - 1);
        return chapterBudgets[index];
    }

    private void UpdateBudgetDisplay()
    {
        if (budgetLabel == null)
            ReconnectBudgetLabel();

        if (budgetLabel != null)
            budgetLabel.text = string.Format(budgetFormat, budgetPrefix, remainingBudget, budgetSuffix);
    }

    public void ReconnectBudgetLabel()
    {
        if (!autoFindBudgetLabel || string.IsNullOrEmpty(budgetLabelTag))
            return;

        GameObject labelObject = null;

        try
        {
            labelObject = GameObject.FindWithTag(budgetLabelTag);
        }
        catch (UnityException)
        {
            Debug.LogWarning($"Budget label tag '{budgetLabelTag}' does not exist yet. Add this tag in Unity or turn off auto finding.");
        }

        if (labelObject == null)
            return;

        budgetLabel = labelObject.GetComponent<TMP_Text>();

        if (budgetLabel == null)
            budgetLabel = labelObject.GetComponentInChildren<TMP_Text>();
    }
}
