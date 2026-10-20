using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[Serializable]
public class TutorialDialogueRequirement
{
    public string SceneName;
    public string SessionName;
}

public class TutorialDialogueManager : MonoBehaviour
{
    public static TutorialDialogueManager Instance { get; private set; }

    [SerializeField] private TutorialDialogueRequirement[] requirements;
    [SerializeField] private Dialogue dialogueSource;
    [SerializeField] private string tutorialEndSessionName;
    [SerializeField] private string postTutorialSceneName;
    [SerializeField] private string postTutorialSessionName;
    public UnityEvent onTutorialCompleted;

    private bool[] completed;
    private bool[] started;
    private bool tutorialActive;
    private bool postTutorialDialogueTriggered;

    public bool IsTutorialActive => tutorialActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        completed = new bool[requirements.Length];
        started = new bool[requirements.Length];

        if (dialogueSource == null)
            dialogueSource = FindObjectOfType<Dialogue>(true);

        if (dialogueSource == null)
            Debug.LogError("TutorialDialogueManager: no persistent Dialogue source was found.", this);
        Debug.Log($"TutorialDialogueManager: initialized on '{gameObject.name}'.", this);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        Dialogue.SessionFinished += OnSessionFinished;
        Debug.Log("TutorialDialogueManager: subscribed to scene and dialogue events.", this);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Dialogue.SessionFinished -= OnSessionFinished;
        Debug.Log("TutorialDialogueManager: unsubscribed from scene and dialogue events.", this);
    }
    public void StartTutorial()
    {
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        if (!gameObject.activeInHierarchy)
        {
            Debug.LogError($"TutorialDialogueManager: '{name}' is under an inactive parent and cannot start its coroutine.", this);
            return;
        }

        Debug.Log($"TutorialDialogueManager: StartTutorial called with {requirements.Length} requirements.", this);
        tutorialActive = true;
        completed = new bool[requirements.Length];
        started = new bool[requirements.Length];
        StartCoroutine(CheckLoadedScenesWhenReady());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"TutorialDialogueManager: scene loaded '{scene.name}' ({mode}).", this);

        if (tutorialActive)
        {
            StartCoroutine(CheckLoadedScenesWhenReady());
            return;
        }

        TryStartPostTutorialDialogue(scene);
    }

    private void TryStartPostTutorialDialogue(Scene scene)
    {
        if (postTutorialDialogueTriggered || string.IsNullOrWhiteSpace(postTutorialSceneName) || string.IsNullOrWhiteSpace(postTutorialSessionName))
            return;

        if (!NamesMatch(scene.name, postTutorialSceneName))
            return;

        if (dialogueSource == null || !dialogueSource.HasSession(postTutorialSessionName))
        {
            Debug.LogWarning($"TutorialDialogueManager: post-tutorial session '{postTutorialSessionName}' was not found on the Dialogue Source.", this);
            return;
        }

        postTutorialDialogueTriggered = true;
        Debug.Log($"TutorialDialogueManager: starting one-time post-tutorial session '{postTutorialSessionName}' in '{scene.name}'.", dialogueSource);
        dialogueSource.StartDialogueSession(postTutorialSessionName);
    }

    private IEnumerator CheckLoadedScenesWhenReady()
    {
        for (int attempt = 0; attempt < 30 && tutorialActive; attempt++)
        {
            Debug.Log($"TutorialDialogueManager: scanning {SceneManager.sceneCount} loaded scene(s), attempt {attempt + 1}.", this);

            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
                CheckScene(SceneManager.GetSceneAt(sceneIndex));

            yield return null;
        }
    }

    private void CheckScene(Scene scene)
    {
        if (!scene.isLoaded)
            return;

        for (int i = 0; i < requirements.Length; i++)
        {
            TutorialDialogueRequirement requirement = requirements[i];
            if (completed[i] || started[i] || !NamesMatch(requirement.SceneName, scene.name))
                continue;

            Debug.Log($"TutorialDialogueManager: checking requirement '{requirement.SceneName}' -> '{requirement.SessionName}' in scene '{scene.name}'.", this);

            if (dialogueSource == null || !dialogueSource.HasSession(requirement.SessionName))
            {
                Debug.LogWarning($"TutorialDialogueManager: persistent Dialogue source does not contain session '{requirement.SessionName}'.", this);
                continue;
            }

            started[i] = true;
            Debug.Log($"TutorialDialogueManager: starting '{requirement.SessionName}' for scene '{scene.name}'.", dialogueSource);
            dialogueSource.StartTutorialDialogue(requirement.SessionName);
            return;
        }
    }

    private bool NamesMatch(string configuredName, string actualName)
    {
        return string.Equals(configuredName?.Trim(), actualName?.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private void OnSessionFinished(Dialogue dialogue, string sessionName)
    {
        if (!tutorialActive)
            return;

        for (int i = 0; i < requirements.Length; i++)
        {
            TutorialDialogueRequirement requirement = requirements[i];
            if (started[i] && !completed[i] && NamesMatch(sessionName, requirement.SessionName))
            {
                completed[i] = true;
                CheckTutorialComplete();
                return;
            }
        }
    }

    private void CheckTutorialComplete()
    {
        foreach (bool requirementCompleted in completed)
        {
            if (!requirementCompleted)
                return;
        }

        tutorialActive = false;
        Debug.Log("TutorialDialogueManager: all tutorial dialogues finished. Invoking onTutorialCompleted.", this);

        if (dialogueSource != null && !string.IsNullOrWhiteSpace(tutorialEndSessionName))
        {
            if (dialogueSource.HasSession(tutorialEndSessionName))
            {
                Debug.Log($"TutorialDialogueManager: starting end session '{tutorialEndSessionName}'.", dialogueSource);
                dialogueSource.StartTutorialEndDialogue(tutorialEndSessionName);
            }
            else
            {
                Debug.LogWarning($"TutorialDialogueManager: end session '{tutorialEndSessionName}' was not found on the Dialogue Source.", this);
            }
        }

        onTutorialCompleted?.Invoke();
    }
}