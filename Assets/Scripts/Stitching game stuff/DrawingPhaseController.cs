using UnityEngine;
using UnityEngine.UI;

public class DrawingPhaseController : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private StitchingMinigameManager manager;
    [SerializeField] private Image guideImage;
    [SerializeField] private Camera uiCamera;
    [SerializeField] private StitchingUILineRenderer uiLineRenderer;
    [SerializeField] private Image progressFill;

    [Header("Phase 1 Timing")]
    [SerializeField] private float phase1DurationSeconds = 10f;

    [Header("Countdown Visual (optional, 1 sprite per second)")]
    [SerializeField] private Image countdownImage;
    [SerializeField] private Sprite[] countdownFrames; // e.g. [10,9,8,...,1]

    [Header("Guide Sensor / Accuracy")]
    [SerializeField] private float missPenalty = 3f;
    [SerializeField] private float missCooldown = 0.25f;
    [SerializeField] private float alphaThreshold = 0.1f;
    [SerializeField] private int sensorRadiusPixels = 12;
    [SerializeField] private bool penalizeEmptySpaceInsideGuide = false;

    private bool isDrawing;
    private float nextMissTime;
    private float phase1TimeRemaining;
    private bool timingPhaseStarted;

    private void OnEnable()
    {
        isDrawing = false;
        nextMissTime = 0f;
        timingPhaseStarted = false;
        phase1TimeRemaining = phase1DurationSeconds;

        ClearDrawnLine();
        UpdateProgressFill();
        UpdateCountdownVisual();
    }

    private void Update()
    {
        TickPhaseTimer();

        if (timingPhaseStarted)
            return;

        if (Input.GetMouseButtonDown(0))
            isDrawing = true;

        if (Input.GetMouseButtonUp(0))
            isDrawing = false;

        if (!isDrawing)
            return;

        // Cosmetic line drawing (UI only)
        AddUILinePointAtMouse();

        // Optional: keep guide penalty logic
        CheckGuideSensorForPenalty();
    }

    private void TickPhaseTimer()
    {
        if (timingPhaseStarted)
            return;

        phase1TimeRemaining -= Time.deltaTime;
        if (phase1TimeRemaining < 0f)
            phase1TimeRemaining = 0f;

        UpdateProgressFill();
        UpdateCountdownVisual();

        if (phase1TimeRemaining <= 0f)
        {
            timingPhaseStarted = true;
            manager.StartTimingPhase();
        }
    }

    private void CheckGuideSensorForPenalty()
    {
        bool isInsideGuideRect;
        bool isCloseToGuideLine = IsMouseCloseToGuideLine(out isInsideGuideRect);

        if (isCloseToGuideLine)
            return;

        if (isInsideGuideRect && !penalizeEmptySpaceInsideGuide)
            return;

        if (Time.time >= nextMissTime)
        {
            manager.DeductAccuracy(missPenalty);
            nextMissTime = Time.time + missCooldown;
        }
    }

    private bool IsMouseCloseToGuideLine(out bool isInsideGuideRect)
    {
        isInsideGuideRect = false;

        if (guideImage == null)
            return false;

        RectTransform guideRect = guideImage.rectTransform;
        Vector2 localPoint;
        Camera cameraToUse = uiCamera;

        if (cameraToUse == null && guideImage.canvas != null && guideImage.canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cameraToUse = guideImage.canvas.worldCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(guideRect, Input.mousePosition, cameraToUse, out localPoint))
            return false;

        Rect rect = guideRect.rect;
        if (!rect.Contains(localPoint))
            return false;

        isInsideGuideRect = true;

        Sprite sprite = guideImage.sprite;
        if (sprite == null)
            return true;

        float normalizedX = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
        float normalizedY = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);

        Rect textureRect = sprite.textureRect;
        int textureX = Mathf.FloorToInt(textureRect.x + normalizedX * textureRect.width);
        int textureY = Mathf.FloorToInt(textureRect.y + normalizedY * textureRect.height);

        try
        {
            return HasOpaquePixelNear(sprite, textureX, textureY);
        }
        catch (UnityException)
        {
            Debug.LogWarning("Guide texture not readable. Enable Read/Write on texture import settings.");
            return false;
        }
    }

    private bool HasOpaquePixelNear(Sprite sprite, int centerX, int centerY)
    {
        Texture2D texture = sprite.texture;
        Rect textureRect = sprite.textureRect;

        int minX = Mathf.Max(Mathf.FloorToInt(textureRect.x), centerX - sensorRadiusPixels);
        int maxX = Mathf.Min(Mathf.FloorToInt(textureRect.xMax) - 1, centerX + sensorRadiusPixels);
        int minY = Mathf.Max(Mathf.FloorToInt(textureRect.y), centerY - sensorRadiusPixels);
        int maxY = Mathf.Min(Mathf.FloorToInt(textureRect.yMax) - 1, centerY + sensorRadiusPixels);
        int radiusSq = sensorRadiusPixels * sensorRadiusPixels;

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                int dx = x - centerX;
                int dy = y - centerY;

                if (dx * dx + dy * dy > radiusSq)
                    continue;

                if (texture.GetPixel(x, y).a >= alphaThreshold)
                    return true;
            }
        }

        return false;
    }

    private void AddUILinePointAtMouse()
    {
        if (uiLineRenderer == null)
            return;

        RectTransform lineRect = uiLineRenderer.rectTransform;
        Vector2 localPoint;
        Camera cameraToUse = uiCamera;

        if (cameraToUse == null)
        {
            Canvas canvas = lineRect.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                cameraToUse = canvas.worldCamera;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(lineRect, Input.mousePosition, cameraToUse, out localPoint))
            uiLineRenderer.AddPoint(localPoint);
    }

    private void ClearDrawnLine()
    {
        if (uiLineRenderer != null)
            uiLineRenderer.ClearLine();
    }

    private void UpdateProgressFill()
    {
        if (progressFill == null || phase1DurationSeconds <= 0f)
            return;

        float elapsed = phase1DurationSeconds - phase1TimeRemaining;
        progressFill.fillAmount = Mathf.Clamp01(elapsed / phase1DurationSeconds);
    }

    private void UpdateCountdownVisual()
    {
        if (countdownImage == null || countdownFrames == null || countdownFrames.Length == 0 || phase1DurationSeconds <= 0f)
            return;

        int elapsedWholeSeconds = Mathf.FloorToInt(phase1DurationSeconds - Mathf.Ceil(phase1TimeRemaining));
        int frameIndex = Mathf.Clamp(elapsedWholeSeconds, 0, countdownFrames.Length - 1);
        countdownImage.sprite = countdownFrames[frameIndex];
    }
}