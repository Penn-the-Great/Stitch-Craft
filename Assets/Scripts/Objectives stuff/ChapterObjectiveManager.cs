using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChapterObjectiveManager : MonoBehaviour
{
    public static ChapterObjectiveManager Instance { get; private set; }

    [Header("Objective Data")]
    [SerializeField] private ChapterObjectiveData[] chapters;
    [SerializeField] private bool useActorsMetric = true;

    [Header("Scene Loading")]
    [SerializeField] private string shopSceneName = "Shop";
    [SerializeField] private string meetingRoomSceneName = "Meeting Room";
    [SerializeField] private bool autoLoadMeetingRoomAdditive = true;

    [Header("UI Rebind")]
    [SerializeField] private string objectiveUITag = "ChapterObjectiveUI";

    public event Action OnDialogueFinished;

    private ChapterObjectiveData _current;
    private ChapterObjectiveUI _objectiveUI;
    private bool _isLoadingMeetingRoom;

    public ChapterObjectiveData CurrentObjective => _current;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        OnDialogueFinished += HandleDialogueFinished;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        OnDialogueFinished -= HandleDialogueFinished;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void SelectChapter(int chapterNumber)
    {
        _current = FindByNumber(chapterNumber);
        if (_current == null)
        {
            Debug.LogError($"No ChapterObjectiveData found for chapter {chapterNumber}");
            return;
        }

        if (autoLoadMeetingRoomAdditive)
            StartCoroutine(EnsureMeetingRoomLoadedThenRefreshUI());
        else
            TryBindUIAndRefresh();
    }

    private IEnumerator EnsureMeetingRoomLoadedThenRefreshUI()
    {
        // Wait until Shop is active/loaded (if you transition to it)
        while (!SceneManager.GetSceneByName(shopSceneName).isLoaded)
            yield return null;

        Scene meeting = SceneManager.GetSceneByName(meetingRoomSceneName);

        if (!meeting.isLoaded && !_isLoadingMeetingRoom)
        {
            _isLoadingMeetingRoom = true;
            AsyncOperation op = SceneManager.LoadSceneAsync(meetingRoomSceneName, LoadSceneMode.Additive);
            if (op == null)
            {
                Debug.LogError($"Failed to start loading additive scene '{meetingRoomSceneName}'. Check Build Settings scene name.");
                _isLoadingMeetingRoom = false;
                yield break;
            }

            while (!op.isDone)
                yield return null;

            _isLoadingMeetingRoom = false;
        }

        // allow one frame for scene objects to initialize
        yield return null;
        TryBindUIAndRefresh();
    }

    public void TryBindUIAndRefresh()
    {
        if (_objectiveUI == null)
            _objectiveUI = FindObjectiveUIByTag();

        if (_objectiveUI != null && _current != null)
            _objectiveUI.ShowObjectives(_current, useActorsMetric);
    }

    private ChapterObjectiveUI FindObjectiveUIByTag()
    {
        if (!string.IsNullOrWhiteSpace(objectiveUITag))
        {
            try
            {
                GameObject tagged = GameObject.FindGameObjectWithTag(objectiveUITag);
                if (tagged != null)
                {
                    ChapterObjectiveUI taggedUI = tagged.GetComponent<ChapterObjectiveUI>();
                    if (taggedUI != null)
                        return taggedUI;

                    Debug.LogWarning($"GameObject tagged '{objectiveUITag}' does not have ChapterObjectiveUI.");
                }
            }
            catch (UnityException)
            {
                Debug.LogWarning($"Objective UI tag '{objectiveUITag}' is not defined. Falling back to finding ChapterObjectiveUI by component.");
            }
        }

        return FindObjectOfType<ChapterObjectiveUI>(true);
    }

    private ChapterObjectiveData FindByNumber(int chapterNumber)
    {
        foreach (var c in chapters)
            if (c != null && c.chapterNumber == chapterNumber)
                return c;
        return null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _objectiveUI = null;
        TryBindUIAndRefresh();
    }

    public void NotifyDialogueFinished()
    {
        OnDialogueFinished?.Invoke();
    }

    private void HandleDialogueFinished()
    {
        Debug.Log("Dialogue finished -> start timer here.");
    }
}