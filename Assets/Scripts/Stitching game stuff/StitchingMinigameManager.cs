using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class StitchingMinigameManager : MonoBehaviour
{
    [SerializeField] private TMP_Text accuracyText;

    [SerializeField] private GameObject drawingPhase;   // phase 1
    [SerializeField] private GameObject timingPhase;    // phase 2
    [SerializeField] private GameObject pressingPhase;  // phase 3
    [SerializeField] private PressingPhaseController pressingPhaseController;
    [SerializeField] private DrawingPhaseController drawingPhaseController;
    [SerializeField] private CosmeticLineDrawer cosmeticLineDrawer;

    [Header("Optional")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject minigameRoot;

    [Header("Transitions")]
    [SerializeField] private float transitionDuration = 0.55f;
    [SerializeField] private AnimationCurve transitionCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 2f),
        new Keyframe(1f, 1f, 2f, 0f)
    );

    private RectTransform drawingRect;
    private RectTransform timingRect;
    private RectTransform pressingRect;

    private Vector2 drawingCenter;
    private Vector2 timingCenter;
    private Vector2 pressingCenter;

    private float accuracy = 100f;
    private bool hasStarted = false;
    private bool isTransitioning = false;

    [SerializeField] private StitchingTableMenu stitchingTableMenu;

    private void Awake()
    {
        drawingRect = drawingPhase.GetComponent<RectTransform>();
        timingRect = timingPhase.GetComponent<RectTransform>();
        pressingRect = pressingPhase.GetComponent<RectTransform>();

        drawingCenter = drawingRect.anchoredPosition;
        timingCenter = timingRect.anchoredPosition;
        pressingCenter = pressingRect.anchoredPosition;

        drawingPhase.SetActive(false);
        timingPhase.SetActive(false);
        pressingPhase.SetActive(false);

        if (minigameRoot != null) minigameRoot.SetActive(true);
        if (startPanel != null) startPanel.SetActive(true);

        UpdateAccuracyText();
    }

    public void OnStartButtonPressed()
    {
        if (hasStarted || isTransitioning) return;

        hasStarted = true;
        accuracy = 100f;
        UpdateAccuracyText();

        if (startPanel != null) startPanel.SetActive(false);

        StartCoroutine(EnterPhase1FromTop());
    }

    private IEnumerator EnterPhase1FromTop()
    {
        isTransitioning = true;

        drawingPhase.SetActive(true);
        timingPhase.SetActive(false);
        pressingPhase.SetActive(false);

        float h = GetScreenHeightInCanvasUnits(drawingRect);
        drawingRect.anchoredPosition = drawingCenter + Vector2.up * h;

        yield return SlideRect(drawingRect, drawingRect.anchoredPosition, drawingCenter, transitionDuration);

        drawingPhaseController.BeginDrawingPhase();
        isTransitioning = false;
    }

    public void StartTimingPhase()
    {
        if (!gameObject.activeInHierarchy || isTransitioning) return;
        StartCoroutine(TransitionPhase1ToPhase2());
    }

    private IEnumerator TransitionPhase1ToPhase2()
    {
        isTransitioning = true;

        cosmeticLineDrawer.ClearLine();

        drawingPhase.SetActive(true);
        timingPhase.SetActive(true);
        pressingPhase.SetActive(false);

        float w = GetScreenWidthInCanvasUnits(timingRect);

        timingRect.anchoredPosition = timingCenter + Vector2.right * w; // start right
        Vector2 drawingOutLeft = drawingCenter + Vector2.left * w;      // pushed left

        Coroutine inCo = StartCoroutine(SlideRect(timingRect, timingRect.anchoredPosition, timingCenter, transitionDuration));
        Coroutine outCo = StartCoroutine(SlideRect(drawingRect, drawingRect.anchoredPosition, drawingOutLeft, transitionDuration));
        yield return inCo;
        yield return outCo;

        drawingPhase.SetActive(false);
        drawingRect.anchoredPosition = drawingCenter; // reset for replay

        isTransitioning = false;
    }

    public void StartPressingPhase()
    {
        if (!gameObject.activeInHierarchy || isTransitioning) return;
        StartCoroutine(TransitionPhase2ToPhase3());
    }

    private IEnumerator TransitionPhase2ToPhase3()
    {
        isTransitioning = true;

        timingPhase.SetActive(true);
        pressingPhase.SetActive(true);
        drawingPhase.SetActive(false);

        float h = GetScreenHeightInCanvasUnits(pressingRect);

        pressingRect.anchoredPosition = pressingCenter + Vector2.down * h; // start below
        Vector2 timingOutUp = timingCenter + Vector2.up * h;               // pushed up

        Coroutine inCo = StartCoroutine(SlideRect(pressingRect, pressingRect.anchoredPosition, pressingCenter, transitionDuration));
        Coroutine outCo = StartCoroutine(SlideRect(timingRect, timingRect.anchoredPosition, timingOutUp, transitionDuration));
        yield return inCo;
        yield return outCo;

        timingPhase.SetActive(false);
        timingRect.anchoredPosition = timingCenter; // reset for replay

        pressingPhaseController.BeginPhase3();
        isTransitioning = false;
    }

    private IEnumerator SlideRect(RectTransform rt, Vector2 from, Vector2 to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float n = Mathf.Clamp01(t / duration);
            float eased = transitionCurve.Evaluate(n);
            rt.anchoredPosition = Vector2.LerpUnclamped(from, to, eased);
            yield return null;
        }
        rt.anchoredPosition = to;
    }

    private float GetScreenWidthInCanvasUnits(RectTransform rt)
    {
        Canvas c = rt.GetComponentInParent<Canvas>();
        RectTransform canvasRt = c != null ? c.GetComponent<RectTransform>() : null;
        return canvasRt != null ? canvasRt.rect.width : Screen.width;
    }

    private float GetScreenHeightInCanvasUnits(RectTransform rt)
    {
        Canvas c = rt.GetComponentInParent<Canvas>();
        RectTransform canvasRt = c != null ? c.GetComponent<RectTransform>() : null;
        return canvasRt != null ? canvasRt.rect.height : Screen.height;
    }

    public void FinishMinigame()
    {
        char finalGrade = GetFinalGrade();
        Debug.Log($"Final grade: {finalGrade}");

        if (stitchingTableMenu != null)
            stitchingTableMenu.ApplyFinalGradeAndSpawn(finalGrade);

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.UnloadScene("Stitching table");
        else
            SceneManager.UnloadSceneAsync("Stitching table");
    }

    public void DeductAccuracy(float amount)
    {
        accuracy = Mathf.Clamp(accuracy - amount, 0f, 100f);
        UpdateAccuracyText();
    }

    private void UpdateAccuracyText()
    {
        if (accuracyText != null) accuracyText.text = $"Accuracy: {accuracy:0}%";
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