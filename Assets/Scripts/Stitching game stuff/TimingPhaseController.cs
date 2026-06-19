using UnityEngine;

public class TimingPhaseController : MonoBehaviour
{
    [SerializeField] private StitchingMinigameManager manager;
    [SerializeField] private RectTransform movingMarker;
    [SerializeField] private RectTransform successZone;
    [SerializeField] private RectTransform movementBounds;

    [SerializeField] private float speed = 300f;
    [SerializeField] private float movementPadding = 16f;
    [SerializeField] private float missPenalty = 10f;
    [SerializeField] private int requiredHits = 3;

    private int direction = 1;
    private int hits = 0;

    private void OnEnable()
    {
        hits = 0;
        direction = 1;
    }

    private void Update()
    {
        MoveMarker();

        if (Input.GetKeyDown(KeyCode.Space))
            PressTimingButton();
    }

    private void MoveMarker()
    {
        if (movingMarker == null)
            return;

        Vector2 position = movingMarker.anchoredPosition;
        position.y += direction * speed * Time.deltaTime;

        GetVerticalMovementRange(out float minY, out float maxY);

        if (position.y >= maxY)
        {
            position.y = maxY;
            direction = -1;
        }

        if (position.y <= minY)
        {
            position.y = minY;
            direction = 1;
        }

        movingMarker.anchoredPosition = position;
    }

    private void GetVerticalMovementRange(out float minY, out float maxY)
    {
        RectTransform bounds = movementBounds != null ? movementBounds : movingMarker.parent as RectTransform;
        if (bounds == null)
        {
            minY = -300f;
            maxY = 300f;
            return;
        }

        float halfBoundsHeight = bounds.rect.height * 0.5f;
        float halfMarkerHeight = movingMarker.rect.height * 0.5f;
        float effectivePadding = Mathf.Max(0f, movementPadding);

        minY = -halfBoundsHeight + halfMarkerHeight + effectivePadding;
        maxY = halfBoundsHeight - halfMarkerHeight - effectivePadding;

        if (minY > maxY)
        {
            float centerY = (minY + maxY) * 0.5f;
            minY = centerY;
            maxY = centerY;
        }
    }

    public void PressTimingButton()
    {
        if (movingMarker == null || successZone == null)
            return;

        float markerTop = movingMarker.anchoredPosition.y + movingMarker.rect.height * 0.5f;
        float markerBottom = movingMarker.anchoredPosition.y - movingMarker.rect.height * 0.5f;

        float zoneTop = successZone.anchoredPosition.y + successZone.rect.height * 0.5f;
        float zoneBottom = successZone.anchoredPosition.y - successZone.rect.height * 0.5f;

        if (markerTop >= zoneBottom && markerBottom <= zoneTop)
        {
            hits++;

            if (hits >= requiredHits)
                manager.StartPressingPhase();
        }
        else
        {
            manager.DeductAccuracy(missPenalty);
        }
    }
}
