using UnityEngine;
using TMPro;

public class StitchingMinigameManager : MonoBehaviour
{
    [SerializeField] private TMP_Text accuracyText;

    [SerializeField] private GameObject drawingPhase;
    [SerializeField] private GameObject timingPhase;
    [SerializeField] private GameObject pressingPhase;

    private float accuracy = 100f;

    private void Start()
    {
        StartDrawingPhase();
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
    }

    public void FinishMinigame()
    {
        drawingPhase.SetActive(false);
        timingPhase.SetActive(false);
        pressingPhase.SetActive(false);

        char finalGrade = GetFinalGrade();
        Debug.Log($"Final grade: {finalGrade}");
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
        if(accuracy >= 80f) return 'B';
        if (accuracy >= 70f) return 'C';
        if (accuracy >= 60f) return 'D';
        return 'F';
    }
}