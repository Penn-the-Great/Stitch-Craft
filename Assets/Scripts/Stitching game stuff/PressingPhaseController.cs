using System.Collections;
using UnityEngine;

public class PressingPhaseController : MonoBehaviour
{
    [SerializeField] private StitchingMinigameManager manager;
    [SerializeField] private RectTransform spawnArea;
    [SerializeField] private FadingCircle circlePrefab;

    [Header("Phase")]
    [SerializeField] private int circlesToSpawn = 8;
    [SerializeField] private float secondsBetweenSpawns = 0.5f;
    [SerializeField] private float circleLifetime = 2f;
    [SerializeField] private float missPenalty = 5f;

    private int resolvedCount;
    private Coroutine spawnRoutine;
    private bool isRunning;

    private void OnEnable()
    {
    }
        public void BeginPhase3()
{
    if (isRunning) return;
    isRunning = true;
    resolvedCount = 0;

    if (spawnRoutine != null) StopCoroutine(spawnRoutine);
    spawnRoutine = StartCoroutine(SpawnRoutine());
}

public void EndPhase3()
{
    isRunning = false;
    if (spawnRoutine != null) StopCoroutine(spawnRoutine);
    spawnRoutine = null;
}

    private void OnDisable()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    private IEnumerator SpawnRoutine()
    {
        for (int i = 0; i < circlesToSpawn; i++)
        {
            SpawnCircle();

            // delay before next spawn (except after last)
            if (i < circlesToSpawn - 1)
                yield return new WaitForSeconds(secondsBetweenSpawns);
        }

        spawnRoutine = null;
    }

    private void SpawnCircle()
    {
        FadingCircle circle = Instantiate(circlePrefab, spawnArea);
        RectTransform rect = circle.GetComponent<RectTransform>();

        float x = Random.Range(-spawnArea.rect.width * 0.5f, spawnArea.rect.width * 0.5f);
        float y = Random.Range(-spawnArea.rect.height * 0.5f, spawnArea.rect.height * 0.5f);
        rect.anchoredPosition = new Vector2(x, y);

        circle.Setup(circleLifetime, CircleClicked, CircleMissed);
    }

    private void CircleClicked(FadingCircle circle)
    {
        Destroy(circle.gameObject);
        ResolveCircle();
    }

    private void CircleMissed(FadingCircle circle)
    {
        Destroy(circle.gameObject);
        manager.DeductAccuracy(missPenalty);
        ResolveCircle();
    }

    private void ResolveCircle()
    {
        resolvedCount++;

        if (resolvedCount >= circlesToSpawn)
            manager.FinishMinigame();
    }


}