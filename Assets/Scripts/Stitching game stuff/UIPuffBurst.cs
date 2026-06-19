using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPuffBurst : MonoBehaviour
{
    private struct PuffPiece
    {
        public RectTransform rect;
        public CanvasGroup group;
        public Vector2 startPosition;
        public Vector2 direction;
        public float startScale;
    }

    private readonly List<PuffPiece> pieces = new List<PuffPiece>();
    private float lifetime;
    private float moveDistance;
    private float timer;

    public void Play(Sprite sprite, int count, float spreadRadius, float moveDistance, float lifetime, Vector2 scaleRange)
    {
        this.lifetime = Mathf.Max(0.01f, lifetime);
        this.moveDistance = Mathf.Max(0f, moveDistance);
        timer = 0f;

        int clampedCount = Mathf.Max(1, count);
        for (int i = 0; i < clampedCount; i++)
        {
            GameObject pieceObject = new GameObject($"Puff {i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
            RectTransform pieceRect = pieceObject.GetComponent<RectTransform>();
            pieceRect.SetParent(transform, false);

            Image pieceImage = pieceObject.GetComponent<Image>();
            pieceImage.sprite = sprite;
            pieceImage.preserveAspect = true;
            pieceImage.raycastTarget = false;

            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 startOffset = direction * Random.Range(0f, Mathf.Max(0f, spreadRadius));
            float startScale = Random.Range(scaleRange.x, scaleRange.y);
            pieceRect.anchoredPosition = startOffset;
            pieceRect.localScale = Vector3.one * startScale;
            pieceRect.sizeDelta = new Vector2(48f, 48f);

            pieces.Add(new PuffPiece
            {
                rect = pieceRect,
                group = pieceObject.GetComponent<CanvasGroup>(),
                startPosition = startOffset,
                direction = direction,
                startScale = startScale
            });
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / lifetime);
        float alpha = 1f - progress;

        for (int i = 0; i < pieces.Count; i++)
        {
            PuffPiece piece = pieces[i];
            if (piece.rect == null)
                continue;

            piece.rect.anchoredPosition = piece.startPosition + piece.direction * moveDistance * progress;
            piece.rect.localScale = Vector3.one * Mathf.Lerp(piece.startScale, piece.startScale * 1.4f, progress);

            if (piece.group != null)
                piece.group.alpha = alpha;
        }

        if (progress >= 1f)
            Destroy(gameObject);
    }
}
