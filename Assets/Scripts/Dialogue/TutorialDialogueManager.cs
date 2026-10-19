using System;
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
    public UnityEvent onTutorialCompleted;

    private bool[] completed;
    private bool tutorialActive;

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
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        Dialogue.SessionFinished += OnSessionFinished;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Dialogue.SessionFinished -= OnSessionFinished;
    }

    public void StartTutorial()
    {
        tutorialActive = true;
        completed = new bool[requirements.Length];
        CheckScene(SceneManager.GetActiveScene());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (tutorialActive)
            CheckScene(scene);
    }

    private void CheckScene(Scene scene)
    {
        for (int i = 0; i < requirements.Length; i++)
        {
            TutorialDialogueRequirement requirement = requirements[i];
            if (completed[i] || requirement.SceneName != scene.name)
                continue;

            Dialogue[] dialogues = FindObjectsOfType<Dialogue>(true);
            foreach (Dialogue dialogue in dialogues)
            {
                if (dialogue.gameObject.scene == scene)
                {
                    dialogue.StartDialogueSession(requirement.SessionName);
                    return;
                }
            }
        }
    }

    private void OnSessionFinished(Dialogue dialogue, string sessionName)
    {
        if (!tutorialActive)
            return;

        for (int i = 0; i < requirements.Length; i++)
        {
            TutorialDialogueRequirement requirement = requirements[i];
            if (!completed[i] && dialogue.gameObject.scene.name == requirement.SceneName && sessionName == requirement.SessionName)
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
        onTutorialCompleted?.Invoke();
    }
}