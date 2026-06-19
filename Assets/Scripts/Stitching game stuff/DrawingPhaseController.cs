using UnityEngine;
using UnityEngine.UI;

public class DrawingPhaseController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StitchingMinigameManager manager;
    [SerializeField] private Image guideImage;
    [SerializeField] private Camera uiCamera;
    [SerializeField] private Image progressFill;
    [SerializeField] private StitchingUILineRenderer uiLineRenderer;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Camera drawingCamera;

    [Header("Scoring / Guide")]
    [SerializeField] private float missPenalty = 3f;
    [SerializeField] private float missCooldown = 0.25f;
    [SerializeField] private float alphaThreshold = 0.1f;
    [SerializeField] private int sensorRadiusPixels = 12;
    [SerializeField] private bool penalizeEmptySpaceInsideGuide = false;

    [Header("Line")]
    [SerializeField] private float lineZDistance = 5f;
    [SerializeField] private float minLinePointDistance = 0.05f;
    [SerializeField] private float lineWidth = 0.05f;

    [Header("Phase 1 Timer")]
    [SerializeField] private float phase1DurationSeconds = 10f;

    [Header("Countdown Visual (1 frame per second)")]
    [SerializeField] private Image countdownImage;         // UI Image to show countdown frames
    [SerializeField] private Sprite[] countdownFrames;     // index 0 = "10", index 9 = "1" (or however you arrange)

    private bool isDrawing = false;
    private float nextMissTime = 0f;

    private Vector3 lastLineWorldPoint;
    private bool hasLastLinePoint = false;

    private float phase1TimeRemaining;
    private bool timingPhaseStarted;

    private void OnEnable()
    {
        isDrawing = false;
        nextMissTime = 0f;
        hasLastLinePoint = false;
        timingPhaseStarted = false;

        phase1TimeRemaining = phase1DurationSeconds;

        ClearDrawnLine();
        UpdateProgressFill();
        UpdateCountdownVisual();
    }

    private void Update()
    {
        // 1) Timer always runs, regardless of drawing
        TickPhase1Timer();

        if (timingPhaseStarted)
            return;

        // 2) Drawing input (cosmetic or gameplay) is separate
        if (Input.GetMouseButtonDown(0))
        {
            isDrawing = true;
            hasLastLinePoint = false;
        }

        if (Input.GetMouseButtonUp(0))
            isDrawing = false;

        if (!isDrawing)
            return;

        // Optional: keep scoring logic based on guide
        CheckGuideSensor();

        // Cosmetic line always draws while holding mouse
        AddLinePointAtMouse();
    }

    private void TickPhase1Timer()
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

    private void CheckGuideSensor()
    {
        bool isInsideGuideRect;
        bool isCloseToGuideLine = IsMouseCloseToGuideLine(out isInsideGuideRect);

        // No longer controls phase transition timing
        // Keep only accuracy penalties, if desired.
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
        if (guideImage == null) return false;

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
            Debug.LogWarning("Drawing guide texture is not readable. Enable Read/Write on the sprite texture.");
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

        int radiusSquared = sensorRadiusPixels * sensorRadiusPixels;

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                int dx = x - centerX;
                int dy = y - centerY;
                if (dx * dx + dy * dy > radiusSquared) continue;

                if (texture.GetPixel(x, y).a >= alphaThreshold)
                    return true;
            }
        }

        return false;
    }

    private void UpdateProgressFill()
    {
        if (progressFill != null && phase1DurationSeconds > 0f)
        {
            float elapsed = phase1DurationSeconds - phase1TimeRemaining;
            progressFill.fillAmount = Mathf.Clamp01(elapsed / phase1DurationSeconds);
        }
    }

    private void UpdateCountdownVisual()
    {
        if (countdownImage == null || countdownFrames == null || countdownFrames.Length == 0)
            return;

        // Example for 10s timer:
        // remaining 10..9.001 -> frame 0
        // remaining 9..8.001  -> frame 1
        // ...
        // remaining 1..0      -> frame 9
        int elapsedWholeSeconds = Mathf.FloorToInt(phase1DurationSeconds - Mathf.Ceil(phase1TimeRemaining));
        int frameIndex = Mathf.Clamp(elapsedWholeSeconds, 0, countdownFrames.Length - 1);
        countdownImage.sprite = countdownFrames[frameIndex];
    }

    private void AddLinePointAtMouse()
    {
        if (uiLineRenderer != null)
        {
            RectTransform lineRect = uiLineRenderer.rectTransform;
            Vector2 localPoint;
            Camera cameraToUse = uiCamera;

            if (cameraToUse == null && lineRect.GetComponentInParent<Canvas>() != null)
            {
                Canvas canvas = lineRect.GetComponentInParent<Canvas>();
                if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                    cameraToUse = canvas.worldCamera;
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(lineRect, Input.mousePosition, cameraToUse, out localPoint))
                uiLineRenderer.AddPoint(localPoint);

            return;
        }

        if (lineRenderer == null) return;

        Camera cam = drawingCamera != null ? drawingCamera : Camera.main;
        if (cam == null) return;

        Vector3 mp = Input.mousePosition;
        mp.z = lineZDistance;
        Vector3 worldPoint = cam.ScreenToWorldPoint(mp);

        if (!hasLastLinePoint)
        {
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;

            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, worldPoint);
            lineRenderer.SetPosition(1, worldPoint + new Vector3(0.1f, 0f, 0f));

            lastLineWorldPoint = worldPoint;
            hasLastLinePoint = true;
            return;
        }

        if (Vector3.Distance(worldPoint, lastLineWorldPoint) < minLinePointDistance)
            return;

        int nextIndex = lineRenderer.positionCount;
        lineRenderer.positionCount = nextIndex + 1;
        lineRenderer.SetPosition(nextIndex, worldPoint);
        lastLineWorldPoint = worldPoint;
    }

    private void ClearDrawnLine()
    {
        if (uiLineRenderer != null)
            uiLineRenderer.ClearLine();

        if (lineRenderer != null)
            lineRenderer.positionCount = 0;
    }
}