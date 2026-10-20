using UnityEngine;
using TMPro;
using System.Collections;
using System;
using UnityEngine.UI;

[System.Serializable]
public class DialogueLine
{
    public string Speaker;
    [TextArea(2, 5)] public string Text;
    public DialogueChoice[] Choices;
}

[System.Serializable]
public class DialogueChoice
{
    public string ButtonText;
    public DialogueLine[] Lines;
}

[System.Serializable]
public class DialogueSession
{
    public string SessionName;
    public DialogueLine[] Lines;
}

public class Dialogue : MonoBehaviour
{
    public static event Action<Dialogue, string> SessionFinished;

    [SerializeField] private GameObject DialogueBox;
    [SerializeField] private TMP_Text DialogueText;
    [SerializeField] private GameObject NextLinePrompt;
    [SerializeField] private TMP_Text SpeakerText;
    [SerializeField] private Button ChoiceButton1;
    [SerializeField] private Button ChoiceButton2;
    [SerializeField] private TMP_Text ChoiceButton1Text;
    [SerializeField] private TMP_Text ChoiceButton2Text;

    [SerializeField] private DialogueSession[] DialogueSessions;
    [SerializeField] private float TypeSpeed = 0.0f;

    private DialogueSession currentSession;
    private DialogueLine[] currentLines;
    private int lineIndex = 0;
    private int returnLineIndex = 0;
    private bool playingChoice;
    private bool isWritingLine;
    private bool skipTyping;

    private void Awake()
    {
        if (ChoiceButton1 != null)
            ChoiceButton1.onClick.AddListener(ChooseFirstOption);

        if (ChoiceButton2 != null)
            ChoiceButton2.onClick.AddListener(ChooseSecondOption);
    }

    private void OnDestroy()
    {
        if (ChoiceButton1 != null)
            ChoiceButton1.onClick.RemoveListener(ChooseFirstOption);

        if (ChoiceButton2 != null)
            ChoiceButton2.onClick.RemoveListener(ChooseSecondOption);
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        if (isWritingLine)
        {
            skipTyping = true;
        }
        else if (NextLinePrompt.activeInHierarchy)
        {
            NextLine();
        }
    }

    private IEnumerator WriteLine()
    {
        DialogueText.text = "";
        isWritingLine = true;
        skipTyping = false;

        DialogueLine line = currentLines[lineIndex];
        SpeakerText.text = line.Speaker;

        foreach (char c in line.Text)
        {
            DialogueText.text += c;

            if (skipTyping)
            {
                DialogueText.text = line.Text;
                break;
            }

            yield return new WaitForSeconds(TypeSpeed);
        }

        isWritingLine = false;

        if (line.Choices != null && line.Choices.Length > 0 && !playingChoice)
        {
            ShowChoices(line.Choices);
        }
        else
        {
            NextLinePrompt.SetActive(true);
        }
    }

    private void NextLine()
    {
        NextLinePrompt.SetActive(false);

        lineIndex++;

        if (lineIndex < currentLines.Length)
        {
            StartCoroutine(WriteLine());
        }
        else if (playingChoice)
        {
            playingChoice = false;
            currentLines = currentSession.Lines;
            lineIndex = returnLineIndex;

            if (lineIndex < currentLines.Length)
            {
                StartCoroutine(WriteLine());
            }
            else
            {
                EndDialogue();
            }
        }
        else
        {
            EndDialogue();
        }
    }

    private void ShowChoices(DialogueChoice[] choices)
    {
        NextLinePrompt.SetActive(false);

        ChoiceButton1.gameObject.SetActive(choices.Length > 0);
        ChoiceButton2.gameObject.SetActive(choices.Length > 1);

        if (choices.Length > 0)
        {
            ChoiceButton1Text.text = choices[0].ButtonText;
        }

        if (choices.Length > 1)
        {
            ChoiceButton2Text.text = choices[1].ButtonText;
        }
    }

    private void SelectChoice(int choiceIndex)
    {
        DialogueChoice[] choices = currentLines[lineIndex].Choices;

        if (choices == null || choiceIndex < 0 || choiceIndex >= choices.Length)
        {
            return;
        }

        DialogueChoice choice = choices[choiceIndex];

        if (choice.Lines == null || choice.Lines.Length == 0)
        {
            return;
        }

        ChoiceButton1.gameObject.SetActive(false);
        ChoiceButton2.gameObject.SetActive(false);

        returnLineIndex = lineIndex + 1;
        currentLines = choice.Lines;
        lineIndex = 0;
        playingChoice = true;

        StartCoroutine(WriteLine());
    }

    public void ChooseFirstOption()
    {
        SelectChoice(0);
    }

    public void ChooseSecondOption()
    {
        SelectChoice(1);
    }

    private void EndDialogue()
    {
        string finishedSessionName = currentSession.SessionName;
        ChoiceButton1.gameObject.SetActive(false);
        ChoiceButton2.gameObject.SetActive(false);
        DialogueBox.SetActive(false);
        NextLinePrompt.SetActive(false);
        SessionFinished?.Invoke(this, finishedSessionName);
    }

    private void StartDialogueSession(int sessionIndex)
    {
        if (sessionIndex < 0 || sessionIndex >= DialogueSessions.Length)
        {
            Debug.LogWarning($"Dialogue session {sessionIndex} does not exist.");
            return;
        }

        currentSession = DialogueSessions[sessionIndex];

        if (currentSession.Lines == null || currentSession.Lines.Length == 0)
        {
            Debug.LogWarning($"Dialogue session '{currentSession.SessionName}' has no lines.");
            return;
        }

        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        if (!gameObject.activeInHierarchy)
        {
            Debug.LogError($"Dialogue '{name}' is under an inactive parent and cannot start its coroutine.", this);
            return;
        }

        if (DialogueBox != null && !DialogueBox.activeSelf)
            DialogueBox.SetActive(true);

        NextLinePrompt.SetActive(false);
        ChoiceButton1.gameObject.SetActive(false);
        ChoiceButton2.gameObject.SetActive(false);

        currentLines = currentSession.Lines;
        lineIndex = 0;
        playingChoice = false;
        StartCoroutine(WriteLine());
    }

    public void StartDialogueSession(string sessionName)
    {
        for (int i = 0; i < DialogueSessions.Length; i++)
        {
            if (string.Equals(DialogueSessions[i].SessionName?.Trim(), sessionName?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                StartDialogueSession(i);
                return;
            }
        }

        Debug.LogWarning($"Dialogue session '{sessionName}' does not exist.");
    }

    public bool HasSession(string sessionName)
    {
        foreach (DialogueSession session in DialogueSessions)
        {
            if (string.Equals(session.SessionName?.Trim(), sessionName?.Trim(), StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    public void StartTutorialDialogue(string sessionName)
    {
        Debug.Log($"Dialogue '{name}': StartTutorialDialogue called for session '{sessionName}'.", this);

        if (TutorialDialogueManager.Instance == null || !TutorialDialogueManager.Instance.IsTutorialActive)
        {
            Debug.LogWarning($"Dialogue '{name}': tutorial dialogue blocked because tutorial mode is inactive or the manager is missing.", this);
            return;
        }

        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        if (!gameObject.activeInHierarchy)
        {
            Debug.LogError($"Dialogue '{name}' is under an inactive parent and cannot start its coroutine.", this);
            return;
        }

        if (DialogueBox != null && !DialogueBox.activeSelf)
            DialogueBox.SetActive(true);

        Debug.Log($"Dialogue '{name}': starting tutorial session '{sessionName}'.", this);
        StartDialogueSession(sessionName);
    }

    public void StartTutorialEndDialogue(string sessionName)
    {
        Debug.Log($"Dialogue '{name}': starting tutorial end session '{sessionName}'.", this);
        StartDialogueSession(sessionName);
    }

    public void StartDialogue()
    {
        StartDialogueSession(0);
    }

    public void StartIntroDialogue()
    {
        StartDialogueSession(0);
    }

    public void StartShopDialogue()
    {
        StartDialogueSession(1);
    }

}
