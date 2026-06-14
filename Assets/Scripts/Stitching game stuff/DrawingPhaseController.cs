using UnityEngine;
using UnityEngine.UI;

public class DrawingPhaseController : MonoBehaviour
{
    [SerializeField] private StitchingMinigameManager manager;
    [SerializeField] private Image guideImage;
    [SerializeField] private Camera uiCamera;
    [SerializeField] private Image progressFill;
    [SerializeField] private float requiredDrawingSeconds = 3f;
    [SerializeField] private float missPenalty = 3f;
    [SerializeField] private float missCooldown = 0.25f;
    [SerializeField] private float alphaThreshold = 0.1f;
    [SerializeField] private int sensorRadiusPixels = 12;
    [SerializeField] private bool penalizeEmptySpaceInsideGuide = false;
    [SerializeField] private StitchingUILineRenderer uiLineRenderer;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Camera drawingCamera;
    [SerializeField] private float lineZDistance = 5f;
    [SerializeField] private float minLinePointDistance = 0.05f;    
[SerializeField] private float lineWidth = 0.05f;

    private bool isDrawing = false;
    private float validDrawingTime = 0f;
    private float nextMissTime = 0f;

    private Vector3 lastLineWorldPoint;
private bool hasLastLinePoint = false;

private void OnEnable()
{
    isDrawing = false;
    validDrawingTime = 0f;
    nextMissTime = 0f;
    ClearDrawnLine();
    hasLastLinePoint = false;
    UpdateProgressFill();
}

    private void Update()
    {
if (Input.GetMouseButtonDown(0))
{
    isDrawing = true;
    hasLastLinePoint = false;
}

        if (Input.GetMouseButtonUp(0))
            isDrawing = false;

        if (!isDrawing)
        return;

        CheckGuideSensor();
    }

    private void CheckGuideSensor()
    {
        bool isInsideGuideRect;
        bool isCloseToGuideLine = IsMouseCloseToGuideLine(out isInsideGuideRect);

if (isCloseToGuideLine)
{
    

    validDrawingTime += Time.deltaTime;
    UpdateProgressFill();

    if (validDrawingTime >= requiredDrawingSeconds)
        manager.StartTimingPhase();

    return;
}

        if (isInsideGuideRect && !penalizeEmptySpaceInsideGuide)
            return;

        if (Time.time >= nextMissTime)
        {
            manager.DeductAccuracy(missPenalty);
            nextMissTime = Time.time + missCooldown;
        }

        if(isDrawing = true)
        {
            AddLinePointAtMouse();
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
            Debug.LogWarning("Drawing guide texture is not readable. Enable Read/Write on the sprite texture for dotted-line sensing.");
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
                int distanceX = x - centerX;
                int distanceY = y - centerY;

                if (distanceX * distanceX + distanceY * distanceY > radiusSquared)
                    continue;

                if (texture.GetPixel(x, y).a >= alphaThreshold)
                    return true;
            }
        }

        return false;
    }

    private void UpdateProgressFill()
    {
        if (progressFill != null)
            progressFill.fillAmount = Mathf.Clamp01(validDrawingTime / requiredDrawingSeconds);
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

    if (lineRenderer == null)
        return;

    Camera worldLineCamera = drawingCamera;

    if (worldLineCamera == null)
        worldLineCamera = Camera.main;

    if (worldLineCamera == null)
        return;

    Vector3 mousePosition = Input.mousePosition;
    mousePosition.z = lineZDistance;

    Vector3 worldPoint = worldLineCamera.ScreenToWorldPoint(mousePosition);

 if (!hasLastLinePoint)
{
    lineRenderer.startWidth = lineWidth;
    lineRenderer.endWidth = lineWidth;

    Vector3 firstPoint = worldPoint;
    Vector3 secondPoint = worldPoint + new Vector3(0.1f, 0f, 0f);

    lineRenderer.positionCount = 2;
    lineRenderer.SetPosition(0, firstPoint);
    lineRenderer.SetPosition(1, secondPoint);

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
    {
        lineRenderer.startWidth = 1f;
        lineRenderer.endWidth = 1f;
    }
}




}
