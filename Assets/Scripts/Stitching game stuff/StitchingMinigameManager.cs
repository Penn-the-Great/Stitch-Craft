using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class StitchingMinigameManager : MonoBehaviour
{
    [SerializeField] private TMP_Text accuracyText;

    [SerializeField] private GameObject drawingPhase;
    [SerializeField] private GameObject timingPhase;
    [SerializeField] private GameObject pressingPhase;
    [SerializeField] private PressingPhaseController pressingPhaseController;
    [SerializeField] private DrawingPhaseController drawingPhaseController;

    [Header("Optional")]
    [SerializeField] private GameObject startPanel; // panel with your "Start" button
    [SerializeField] private GameObject minigameRoot; // optional parent object for all minigame UI

    [Header("End Transition")]
    [SerializeField] private bool returnToShopOnFinish = true;
    [SerializeField] private string shopSceneName = "Shop";
    [SerializeField] private LoadSceneMode returnLoadMode = LoadSceneMode.Single;

    private float accuracy = 100f;
    private bool hasStarted = false;
    [SerializeField] private StitchingTableMenu stitchingTableMenu;
    private const string CraftPendingKey = "Craft_Pending";
private const string CraftGradeKey = "Craft_Grade";


    private void Awake()
    {
        // Keep minigame inactive/idle at load
        drawingPhase.SetActive(false);
        timingPhase.SetActive(false);
        pressingPhase.SetActive(false);

        if (minigameRoot != null) minigameRoot.SetActive(true); // UI visible but idle (or false if you want hidden)
        if (startPanel != null) startPanel.SetActive(true);

        UpdateAccuracyText();
    }

public void FinishMinigame()
{
    char finalGrade = GetFinalGrade();
    Debug.Log($"Final grade: {finalGrade}");

    FindAnyObjectByType<StitchingTableMenu>().ApplyFinalGradeAndSpawn(finalGrade);


if (SceneTransitionManager.Instance != null)
    SceneTransitionManager.Instance.UnloadScene("Stitching table");
else
    SceneManager.UnloadSceneAsync("Stitching table");
}

    /// <summary>
    /// Hook this to your UI Button OnClick()
    /// </summary>
    public void OnStartButtonPressed()
    {
        if (hasStarted) return;

        hasStarted = true;
        accuracy = 100f;
        UpdateAccuracyText();

        if (startPanel != null) startPanel.SetActive(false);

        StartMinigame();
    }

    public void StartMinigame()
    {
        StartDrawingPhase();
            drawingPhaseController.BeginDrawingPhase(); 
        UpdateAccuracyText();
    }

    public void StartDrawingPhase()
    {
        drawingPhase.SetActive(true);
        timingPhase.SetActive(false);
        pressingPhase.SetActive(false);
    }

    public void StartTimingPhase()
    {
        drawingPhase.SetActive(false);
        timingPhase.SetActive(true);
        pressingPhase.SetActive(false);
    }

    public void StartPressingPhase()
    {
        drawingPhase.SetActive(false);
        timingPhase.SetActive(false);
        pressingPhase.SetActive(true);

        pressingPhaseController.BeginPhase3();
    }

    public void DeductAccuracy(float amount)
    {
        accuracy -= amount;
        accuracy = Mathf.Clamp(accuracy, 0f, 100f);
        UpdateAccuracyText();
    }

    private void UpdateAccuracyText()
    {
        if (accuracyText != null)
            accuracyText.text = $"Accuracy: {accuracy:0}%";
    }

    public char GetFinalGrade()
    {
        if (accuracy >= 90f) return 'A';
        if (accuracy >= 80f) return 'B';
        if (accuracy >= 70f) return 'C';
        if (accuracy >= 60f) return 'D';
        return 'F';
    }
}