using UnityEngine;

public class TimingPhaseController : MonoBehaviour
{
    [SerializeField] private StitchingMinigameManager manager;
    [SerializeField] private RectTransform movingMarker;
    [SerializeField] private RectTransform successZone;

    [SerializeField] private float speed = 300f;
    [SerializeField] private float missPenalty = 10f;
    [SerializeField] private int requiredHits = 3;

    private const float MarkerLimit = 147f;
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
        Vector2 position = movingMarker.anchoredPosition;
        position.y += direction * speed * Time.deltaTime;

        if (position.y > MarkerLimit)
        {
            position.y = MarkerLimit;   // clamp
            direction = -1;
        }
        else if (position.y < -MarkerLimit)
        {
            position.y = -MarkerLimit;  // clamp
            direction = 1;
        }

        movingMarker.anchoredPosition = position;
    }

    private void ChangeSuccess()
    {
        Vector2 position = successZone.anchoredPosition;
        position.y = Random.Range(-132f, 132f);
        successZone.anchoredPosition = position;
    }

    public void PressTimingButton()
    {
        // Use anchoredPosition for BOTH so they're in same coordinate space
        float markerY = movingMarker.anchoredPosition.y;
        float zoneCenterY = successZone.anchoredPosition.y;
        float halfZoneHeight = successZone.rect.height * 0.5f;

        float zoneBottom = zoneCenterY - halfZoneHeight;
        float zoneTop = zoneCenterY + halfZoneHeight;

        bool isHit = markerY >= zoneBottom && markerY <= zoneTop;

        if (isHit)
        {
            hits++;
            ChangeSuccess();

            if (hits >= requiredHits)
                manager.StartPressingPhase();
        }
        else
        {
            manager.DeductAccuracy(missPenalty);
        }
    }
}