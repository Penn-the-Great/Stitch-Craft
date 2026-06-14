using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class FadingCircle : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Button button;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Graphic clickableGraphic;

    [SerializeField] private Graphic graphic;

    [Header("Poof")]
    [SerializeField] private CirclePoofPiece poofPrefab;
    [SerializeField] private RectTransform poofParent; // optional; defaults to circle parent
    [SerializeField] private int poofCount = 6;
    [SerializeField] private Vector2 poofSpeedMinMax = new Vector2(120f, 260f);
    [SerializeField] private Vector2 poofSpinMinMax = new Vector2(120f, 480f);
    [SerializeField] private float poofLifetime = 0.65f;

    private float lifetime;
    private float timer;
    private Action<FadingCircle> onClicked;
    private Action<FadingCircle> onMissed;
    private bool finished = false;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (clickableGraphic == null)
            clickableGraphic = GetComponent<Graphic>();

        if (clickableGraphic == null)
            clickableGraphic = GetComponentInChildren<Graphic>();

        if (clickableGraphic != null)
            clickableGraphic.raycastTarget = true;
    }

    public void Setup(float lifetime, Action<FadingCircle> onClicked, Action<FadingCircle> onMissed)
    {
        this.lifetime = lifetime;
        this.onClicked = onClicked;
        this.onMissed = onMissed;

        timer = lifetime;
        finished = false;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(Click);
        }
    }

    private void Update()
    {
        if (finished)
            return;

        timer -= Time.deltaTime;
        canvasGroup.alpha = timer / lifetime;

        if (timer <= 0f)
        {
            finished = true;
            onMissed?.Invoke(this);
        }
    }

    private void Click()
    {
        if (finished)
            return;

        finished = true;
        canvasGroup.blocksRaycasts = false;
        SpawnPoof();
        onClicked?.Invoke(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Click();
    }

      private void SpawnPoof()
    {
        if (poofPrefab == null) return;

        RectTransform myRect = (RectTransform)transform;
        RectTransform parent = poofParent != null ? poofParent : (RectTransform)transform.parent;

        for (int i = 0; i < poofCount; i++)
        {
            CirclePoofPiece piece = Instantiate(poofPrefab, parent);
            RectTransform pieceRect = (RectTransform)piece.transform;
            pieceRect.anchoredPosition = myRect.anchoredPosition;

            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            float speed = UnityEngine.Random.Range(poofSpeedMinMax.x, poofSpeedMinMax.y);
            Vector2 vel = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;

            float spin = UnityEngine.Random.Range(poofSpinMinMax.x, poofSpinMinMax.y);
            spin *= UnityEngine.Random.value < 0.5f ? -1f : 1f;

            piece.Init(vel, spin, poofLifetime);
        }
    }
}
