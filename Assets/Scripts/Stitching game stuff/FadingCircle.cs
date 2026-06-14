using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class FadingCircle : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Button button;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Graphic clickableGraphic;

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
        onClicked?.Invoke(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Click();
    }
}
