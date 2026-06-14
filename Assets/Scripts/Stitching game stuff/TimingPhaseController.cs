using UnityEngine;

public class TimingPhaseController : MonoBehaviour
{
    [SerializeField] private StitchingMinigameManager manager;
    [SerializeField] private RectTransform movingMarker;
    [SerializeField] private RectTransform successZone;

    [SerializeField] private float speed = 300f;
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
        Vector2 position = movingMarker.anchoredPosition;
        position.x += direction * speed * Time.deltaTime;

        if(position.x > 300f)
        direction = -1;

        if (position.x < -300f)
        direction = 1;

        movingMarker.anchoredPosition = position;
    }

    public void PressTimingButton()
    {
        float markerX = movingMarker.position.x;
        float zoneLeft = successZone.position.x - successZone.rect.width / 2f;
        float zoneRight = successZone.position.x + successZone.rect.width / 2f;

        if (markerX >= zoneLeft && markerX <= zoneRight)
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
