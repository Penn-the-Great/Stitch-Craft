using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CompilerManager : MonoBehaviour
{
    public static CompilerManager Instance { get; private set; }

    [System.Serializable]
    public class PieceSprite
    {
        public string piece;
        public Sprite sprite;
    }

    [System.Serializable]
    public class PieceSize
    {
        public string piece; // e.g. "Blouse", "T-shirt", or fallback keys like "top"
        public Vector2 widthHeight = new Vector2(300f, 300f); // x = width, y = height
    }

    [System.Serializable]
    public class PiecePosition
    {
        public string piece; // e.g. "Blouse", "T-shirt", or fallback keys like "top"
        public Vector2 anchoredPosition = Vector2.zero;
    }

    [Header("Drop Area")]
    public RectTransform mannequinDropArea;

    [Header("Spawn Parents")]
    public Transform mannequinRoot;
    public Canvas hangerCanvas;
    [SerializeField] private string playerCanvasTag = "WorkspaceCanvas";
    [SerializeField] private string playerCanvasName = "Player canvas";

    [Header("Prefabs")]
    public GameObject mannequinClothingPrefab;
    public GameObject hangerPrefab;

    [Header("Sprites")]
    public PieceSprite[] pieceSprites;

    [Header("Per-Variant Width/Height Overrides")]
    [SerializeField] private PieceSize[] pieceSizes;
    [SerializeField] private Vector2 defaultWidthHeight = new Vector2(300f, 300f);

    [Header("Per-Variant Position Overrides (Canvas Anchored)")]
    [SerializeField] private PiecePosition[] piecePositions;
    [SerializeField] private Vector2 defaultAnchoredPosition = Vector2.zero;

    [Header("UI")]
    public Button submitButton;
    public TMP_Text counterText;

    [Header("Submission")]
    public int requiredCostumes = 1;
    public float slideDistance = 1400f;
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float slideDuration = 0.75f;

    [Header("Return Hangers")]
    [SerializeField] private bool returnWornPiecesOnDestroy = true;
    [SerializeField] private Vector2 returnedHangerStartPosition = Vector2.zero;
    [SerializeField] private float returnedHangerSpacing = 120f;

    private Dictionary<string, GameObject> wornPieces = new Dictionary<string, GameObject>();
    private int submittedCostumes = 0;
    private bool isSliding = false;
    private bool shouldReturnWornPieces = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        FindHangerCanvasIfNeeded();
    }

    private void Start()
    {
        FindHangerCanvasIfNeeded();

        ChapterObjectiveData objective = ChapterObjectiveManager.Instance?.CurrentObjective;
        if (objective != null)
            requiredCostumes = objective.requiredActors;

        if (submitButton != null)
            submitButton.onClick.AddListener(SubmitCurrentCostume);

        RefreshSubmitButton();
        RefreshCounter();
    }

    private void OnDestroy()
    {
        if (returnWornPiecesOnDestroy && shouldReturnWornPieces)
            ReturnAllWornPiecesToHangers();

        if (Instance == this)
            Instance = null;
    }

    public bool IsPointerOverDropArea(Vector2 screenPosition, Camera eventCamera)
    {
        if (mannequinDropArea == null) return false;

        Canvas dropCanvas = mannequinDropArea.GetComponentInParent<Canvas>();
        Camera cameraToUse = eventCamera;
        if (dropCanvas != null)
        {
            cameraToUse = dropCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : eventCamera != null ? eventCamera : dropCanvas.worldCamera;
        }

        return RectTransformUtility.RectangleContainsScreenPoint(
            mannequinDropArea,
            screenPosition,
            cameraToUse
        );
    }

    public bool AddHangerToMannequin(TopProperty properties)
    {
        if (properties == null || isSliding) return false;
        if (mannequinClothingPrefab == null || mannequinRoot == null) return false;

        if (string.IsNullOrWhiteSpace(properties.piece)) return false;
        string pieceKey = NormalizePieceKey(properties.piece);

        if (wornPieces.ContainsKey(pieceKey))
            RemoveWornPiece(pieceKey);

        if (pieceKey == "full")
        {
            RemoveWornPiece("top");
            RemoveWornPiece("bottom");
        }
        else if (pieceKey == "top" || pieceKey == "bottom")
        {
            RemoveWornPiece("full");
        }

        GameObject clothingObject = Instantiate(mannequinClothingPrefab, mannequinRoot);
        clothingObject.name = properties.displayName;

        RectTransform rt = clothingObject.GetComponent<RectTransform>();
        if (rt != null)
        {
            // Position: variant first, then normalized piece fallback
            Vector2 position = GetAnchoredPositionForPiece(properties.displayName);
            if (position == defaultAnchoredPosition)
                position = GetAnchoredPositionForPiece(pieceKey);
            rt.anchoredPosition = position;

            // Size: variant first, then normalized piece fallback
            Vector2 size = GetWidthHeightForPiece(properties.displayName);
            if (size == defaultWidthHeight)
                size = GetWidthHeightForPiece(pieceKey);
            rt.sizeDelta = size; // x = width, y = height
        }

        TopProperty newProperties = clothingObject.GetComponent<TopProperty>();
        CopyProperties(properties, newProperties);

        Image image = clothingObject.GetComponent<Image>();
        if (image != null)
        {
            // Sprite: variant first, then normalized piece fallback
            Sprite sprite = GetSpriteForPiece(properties.displayName);
            if (sprite == null)
                sprite = GetSpriteForPiece(pieceKey);

            if (sprite == null)
            {
                Debug.LogWarning(
                    $"[CompilerManager] No sprite match. displayName='{properties.displayName}', piece='{properties.piece}'. " +
                    "Check pieceSprites[].piece values in inspector."
                );
            }

            image.sprite = sprite;
            image.color = properties.color;
        }

        CompilerPieceDragger dragger = clothingObject.GetComponent<CompilerPieceDragger>();
        if (dragger != null)
            dragger.Setup(this);

        wornPieces[pieceKey] = clothingObject;
        RefreshSubmitButton();
        return true;
    }

    public void ReturnPieceToHanger(GameObject clothingObject, Vector2 screenPosition, Camera eventCamera)
    {
        if (clothingObject == null || isSliding) return;
        FindHangerCanvasIfNeeded();
        if (hangerCanvas == null || hangerPrefab == null) return;

        TopProperty properties = clothingObject.GetComponent<TopProperty>();
        if (properties == null) return;

        if (string.IsNullOrWhiteSpace(properties.piece)) return;
        string pieceKey = NormalizePieceKey(properties.piece);

        wornPieces.Remove(pieceKey);

        SpawnReturnedHanger(properties, GetReturnedHangerPosition(screenPosition, eventCamera));

        Destroy(clothingObject);
        RefreshSubmitButton();
    }

    public void ReturnPieceToHanger(GameObject clothingObject)
    {
        ReturnPieceToHanger(clothingObject, Vector2.zero, null);
    }

    public void ReturnAllWornPiecesToHangers()
    {
        FindHangerCanvasIfNeeded();
        if (hangerCanvas == null || hangerPrefab == null) return;

        List<GameObject> piecesToReturn = new List<GameObject>(wornPieces.Values);
        wornPieces.Clear();

        for (int i = 0; i < piecesToReturn.Count; i++)
        {
            GameObject clothingObject = piecesToReturn[i];
            if (clothingObject == null) continue;

            TopProperty properties = clothingObject.GetComponent<TopProperty>();
            if (properties != null)
                SpawnReturnedHanger(properties, i);

            Destroy(clothingObject);
        }

        RefreshSubmitButton();
    }

    private IEnumerator SlideOutfitAway()
    {
        isSliding = true;

        Vector3 startPosition = mannequinRoot.localPosition;
        Vector3 endPosition = startPosition + Vector3.left * slideDistance;

        float time = 0f;

        while (time < slideDuration)
        {
            time += Time.deltaTime;
            float percent = time / slideDuration;
            float curvedPercent = slideCurve.Evaluate(percent);

            mannequinRoot.localPosition = Vector3.Lerp(startPosition, endPosition, curvedPercent);
            yield return null;
        }

        ClearCurrentOutfit();

        mannequinRoot.localPosition = startPosition + Vector3.right * slideDistance;

        time = 0f;

        while (time < slideDuration)
        {
            time += Time.deltaTime;
            float percent = time / slideDuration;
            float curvedPercent = slideCurve.Evaluate(percent);

            mannequinRoot.localPosition = Vector3.Lerp(
                startPosition + Vector3.right * slideDistance,
                startPosition,
                curvedPercent
            );

            yield return null;
        }

        mannequinRoot.localPosition = startPosition;
        isSliding = false;
        RefreshSubmitButton();
    }

    private bool CanSubmit()
    {
        bool hasFull = wornPieces.ContainsKey("full");
        bool hasTopAndBottom = wornPieces.ContainsKey("top") && wornPieces.ContainsKey("bottom");
        return hasFull || hasTopAndBottom;
    }

    private void SubmitCurrentCostume()
    {
        if (!CanSubmit() || isSliding) return;

        submittedCostumes++;
        RefreshCounter();

        if (submittedCostumes >= requiredCostumes)
        {
            EndChapterNow();
            return;
        }

        StartCoroutine(SlideOutfitAway());
    }

    private void RefreshSubmitButton()
    {
        if (submitButton != null)
            submitButton.interactable = CanSubmit() && !isSliding;
    }

    private void RefreshCounter()
    {
        if (counterText != null)
            counterText.text = submittedCostumes + " / " + requiredCostumes;
    }

    private void FindHangerCanvasIfNeeded()
    {
        if (hangerCanvas != null) return;

        GameObject taggedCanvas = FindGameObjectWithTagIfDefined(playerCanvasTag);
        if (taggedCanvas != null)
            hangerCanvas = taggedCanvas.GetComponent<Canvas>();

        if (hangerCanvas != null) return;

        GameObject namedCanvas = GameObject.Find(playerCanvasName);
        if (namedCanvas != null)
            hangerCanvas = namedCanvas.GetComponent<Canvas>();
    }

    private GameObject FindGameObjectWithTagIfDefined(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName)) return null;

        try
        {
            return GameObject.FindGameObjectWithTag(tagName);
        }
        catch (UnityException)
        {
            return null;
        }
    }

    private void ClearCurrentOutfit()
    {
        shouldReturnWornPieces = false;

        foreach (GameObject piece in wornPieces.Values)
        {
            if (piece != null)
                Destroy(piece);
        }

        wornPieces.Clear();
        shouldReturnWornPieces = true;
    }

    private void SpawnReturnedHanger(TopProperty properties, int returnIndex)
    {
        Vector2 position = returnedHangerStartPosition;
        position.x += returnIndex * returnedHangerSpacing;
        SpawnReturnedHanger(properties, position);
    }

    private void SpawnReturnedHanger(TopProperty properties, Vector2 anchoredPosition)
    {
        GameObject hanger = Instantiate(hangerPrefab, hangerCanvas.transform, false);

        RectTransform hangerRect = hanger.GetComponent<RectTransform>();
        if (hangerRect != null)
            hangerRect.anchoredPosition = anchoredPosition;

        TopProperty hangerProperties = hanger.GetComponent<TopProperty>();
        CopyProperties(properties, hangerProperties);
    }

    private Vector2 GetReturnedHangerPosition(Vector2 screenPosition, Camera eventCamera)
    {
        Vector2 position = returnedHangerStartPosition;
        RectTransform canvasRect = hangerCanvas.transform as RectTransform;
        if (canvasRect == null) return position;

        Camera cameraToUse = GetCanvasEventCamera(hangerCanvas, eventCamera);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            cameraToUse,
            out Vector2 localPosition))
        {
            position.x = localPosition.x;
        }

        return position;
    }

    private Camera GetCanvasEventCamera(Canvas canvas, Camera eventCamera)
    {
        if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        return eventCamera != null ? eventCamera : canvas.worldCamera;
    }

    private void RemoveWornPiece(string pieceKey)
    {
        if (!wornPieces.ContainsKey(pieceKey)) return;

        Destroy(wornPieces[pieceKey]);
        wornPieces.Remove(pieceKey);
    }

    private string NormalizePieceKey(string piece)
    {
        if (string.IsNullOrWhiteSpace(piece)) return string.Empty;

        string p = piece.Trim().ToLowerInvariant();
        if (p == "shoes") p = "shoe";
        if (p == "pants") p = "bottom";
        return p;
    }

    // Compares against pieceSprites[].piece
    private Sprite GetSpriteForPiece(string piece)
    {
        if (string.IsNullOrWhiteSpace(piece)) return null;
        if (pieceSprites == null) return null;

        string key = piece.Trim();

        foreach (PieceSprite pieceSprite in pieceSprites)
        {
            if (pieceSprite == null || pieceSprite.sprite == null) continue;
            if (string.Equals(pieceSprite.piece?.Trim(), key, System.StringComparison.OrdinalIgnoreCase))
                return pieceSprite.sprite;
        }

        return null;
    }

    // Compares against pieceSizes[].piece
    private Vector2 GetWidthHeightForPiece(string pieceName)
    {
        if (string.IsNullOrWhiteSpace(pieceName) || pieceSizes == null)
            return defaultWidthHeight;

        string key = pieceName.Trim();

        foreach (PieceSize ps in pieceSizes)
        {
            if (ps == null || string.IsNullOrWhiteSpace(ps.piece)) continue;
            if (string.Equals(ps.piece.Trim(), key, System.StringComparison.OrdinalIgnoreCase))
                return ps.widthHeight;
        }

        return defaultWidthHeight;
    }

    // Compares against piecePositions[].piece
    private Vector2 GetAnchoredPositionForPiece(string pieceName)
    {
        if (string.IsNullOrWhiteSpace(pieceName) || piecePositions == null)
            return defaultAnchoredPosition;

        string key = pieceName.Trim();

        foreach (PiecePosition pp in piecePositions)
        {
            if (pp == null || string.IsNullOrWhiteSpace(pp.piece)) continue;
            if (string.Equals(pp.piece.Trim(), key, System.StringComparison.OrdinalIgnoreCase))
                return pp.anchoredPosition;
        }

        return defaultAnchoredPosition;
    }

    private void CopyProperties(TopProperty from, TopProperty to)
    {
        if (from == null || to == null) return;

        to.piece = from.piece;
        to.displayName = from.displayName;
        to.color = from.color;
        to.material = from.material;
        to.style = from.style;
        to.grade = from.grade;
        to.cost = from.cost;
    }

    private void EndChapterNow()
    {
        shouldReturnWornPieces = false;

        if (TimelineHandler.Instance != null)
            TimelineHandler.Instance.TransitionToNextScene();
        else
            SceneManager.LoadScene("Dialogue and end");
    }
}