using UnityEngine;

public class PressingPhaseController : MonoBehaviour
{
    [SerializeField] private StitchingMinigameManager manager;
    [SerializeField] private RectTransform spawnArea;
    [SerializeField] private FadingCircle circlePrefab;

    [SerializeField] private int circlesToClick = 8;
    [SerializeField] private float circleLifetime = 2f;
    [SerializeField] private float missPenalty = 5f;

    private int circlesFinished = 0;

    private void OnEnable()
    {
        circlesFinished = 0;
        SpawnCircle();
    }

    private void SpawnCircle()
    {
        FadingCircle circle = Instantiate(circlePrefab, spawnArea);
        RectTransform rect = circle.GetComponent<RectTransform>();

        float x = Random.Range(-spawnArea.rect.width / 2f, spawnArea.rect.width / 2f);
        float y = Random.Range(-spawnArea.rect.height / 2f, spawnArea.rect.height / 2f);

        rect.anchoredPosition = new Vector2(x, y);

        circle.Setup(circleLifetime, CircleClicked, CircleMissed);
    }

    private void CircleClicked(FadingCircle circle)
    {
        Destroy(circle.gameObject);
        FinishCircle();
    }

    private void CircleMissed(FadingCircle circle)
    {
        Destroy(circle.gameObject);
        manager.DeductAccuracy(missPenalty);
        FinishCircle();
    }

    private void FinishCircle()
    {
        circlesFinished++;

        if (circlesFinished >= circlesToClick)
            manager.FinishMinigame();
        else
            SpawnCircle();
    }


    
}
