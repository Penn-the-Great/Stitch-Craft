using UnityEngine;
using UnityEngine.UI;

public class PressingPhaseController : MonoBehaviour
{
    [SerializeField] private StitchingMinigameManager manager;
    [SerializeField] private RectTransform spawnArea;
    [SerializeField] private FadingCircle circlePrefab;

    [SerializeField] private int circlesToClick = 8;
    [SerializeField] private float circleLifetime = 2f;
    [SerializeField] private float missPenalty = 5f;
    [SerializeField] private float circleScaleMultiplier = 1.8f;

    [Header("Click Puff")]
    [SerializeField] private RectTransform puffContainer;
    [SerializeField] private Sprite puffSprite;
    [SerializeField] private int puffCount = 7;
    [SerializeField] private float puffSpreadRadius = 32f;
    [SerializeField] private float puffMoveDistance = 26f;
    [SerializeField] private float puffLifetime = 0.22f;
    [SerializeField] private Vector2 puffScaleRange = new Vector2(0.35f, 0.65f);

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
        rect.localScale = circlePrefab.transform.localScale * circleScaleMultiplier;

        circle.Setup(circleLifetime, CircleClicked, CircleMissed);
    }

    private void CircleClicked(FadingCircle circle)
    {
        SpawnPuff(circle);
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

    private void SpawnPuff(FadingCircle circle)
    {
        if (circle == null)
            return;

        RectTransform container = puffContainer != null ? puffContainer : spawnArea;
        if (container == null)
            return;

        Sprite spriteToUse = puffSprite;
        if (spriteToUse == null)
        {
            Graphic sourceGraphic = circle.GetComponent<Graphic>();
            if (sourceGraphic == null)
                sourceGraphic = circle.GetComponentInChildren<Graphic>();

            if (sourceGraphic is Image imageGraphic)
                spriteToUse = imageGraphic.sprite;
        }

        if (spriteToUse == null)
            return;

        GameObject burstObject = new GameObject("Circle Click Puff", typeof(RectTransform), typeof(UIPuffBurst));
        RectTransform burstRect = burstObject.GetComponent<RectTransform>();
        burstRect.SetParent(container, false);

        Vector3 localPosition = container.InverseTransformPoint(circle.transform.position);
        burstRect.anchoredPosition = new Vector2(localPosition.x, localPosition.y);

        UIPuffBurst burst = burstObject.GetComponent<UIPuffBurst>();
        burst.Play(spriteToUse, puffCount, puffSpreadRadius, puffMoveDistance, puffLifetime, puffScaleRange);
    }


    
}
