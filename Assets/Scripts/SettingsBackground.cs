using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DiagonalGridGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    public GameObject cellPrefab;
    public int gridWidth = 5;
    public int gridHeight = 5;

    [Header("UI Settings")]
    public RectTransform uiParent;
    public string canvasTag = "SettingsCanvas";
    public Vector2 cellSize = new Vector2(32f, 32f);
    public bool clipToParentBounds = true;

    [Header("Icon Sprites")]
    public Sprite iconSprite1;
    public Sprite iconSprite2;
    public Sprite iconSprite3;
    public Sprite iconSprite4;

    [Header("Spacing Settings")]
    [Tooltip("Horizontal spacing between columns")]
    public float spacingX = 40.0f;
    [Tooltip("Vertical spacing between rows")]
    public float spacingY = 24.0f;

    [Header("Motion")]
    public float scrollSpeed = 18.0f;

    private readonly List<RectTransform> generatedCells = new List<RectTransform>();
    private readonly List<RotatingCell> rotatingCells = new List<RotatingCell>();
    private bool refreshPending;
    private bool isRefreshing;
    private RectTransform cachedParentRect;

    private void Start()
    {
        RequestRefresh();
    }

    private void OnEnable()
    {
        RequestRefresh();
    }

    private void OnValidate()
    {
        gridWidth = Mathf.Max(1, gridWidth);
        gridHeight = Mathf.Max(1, gridHeight);

        RequestRefresh();
    }

    private void Update()
    {
        if (!Application.isPlaying)
            return;

        if (refreshPending)
            RefreshGrid();
        
        for (int i = 0; i < rotatingCells.Count; i++)
        {
            RotatingCell rotator = rotatingCells[i];
            if (rotator != null && rotator.rect != null)
                rotator.rect.Rotate(0, 0, rotator.rotationSpeed * Time.deltaTime);
        }
    }

    private void RequestRefresh()
    {
        refreshPending = true;
    }

    private void RefreshGrid()
    {
        if (isRefreshing)
            return;

        isRefreshing = true;
        refreshPending = false;

        if (cellPrefab == null)
        {
            Debug.LogError("Please assign a Cell Prefab in the Inspector.");
            isRefreshing = false;
            return;
        }

        RectTransform parentRect = transform as RectTransform;
        RectTransform canvasRect = null;

        if (uiParent != null)
            canvasRect = uiParent;
        else
        {
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null)
                canvasRect = parentCanvas.GetComponent<RectTransform>();

            if (canvasRect == null)
            {
                GameObject canvasObject = FindCanvasObjectByTag(canvasTag);
                if (canvasObject != null)
                    canvasRect = canvasObject.GetComponent<RectTransform>();
            }
        }

        if (parentRect == null)
        {
            Debug.LogError("Please attach this script to a UI GameObject with a RectTransform.");
            isRefreshing = false;
            return;
        }

        if (canvasRect != null)
        {
            parentRect.anchorMin = new Vector2(0.5f, 0.5f);
            parentRect.anchorMax = new Vector2(0.5f, 0.5f);
            parentRect.pivot = new Vector2(0.5f, 0.5f);
            parentRect.sizeDelta = canvasRect.rect.size;
            parentRect.anchoredPosition = Vector2.zero;
        }

        if (clipToParentBounds && parentRect.GetComponent<RectMask2D>() == null)
            parentRect.gameObject.AddComponent<RectMask2D>();

        cachedParentRect = parentRect;
        CacheGeneratedCells(parentRect);

        int columns = gridWidth;
        int rows = gridHeight;
        int desiredCount = columns * rows;

        while (generatedCells.Count < desiredCount)
        {
            GameObject newCell = Instantiate(cellPrefab, parentRect);

            RectTransform rect = newCell.GetComponent<RectTransform>();
            if (rect == null)
                rect = newCell.AddComponent<RectTransform>();

            Image image = newCell.GetComponent<Image>();
            if (image == null)
                image = newCell.AddComponent<Image>();

            image.raycastTarget = false;
            Sprite randomSprite = GetRandomIconSprite();
            if (randomSprite != null)
                image.sprite = randomSprite;

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = cellSize;
            rect.localScale = Vector3.one;

            RotatingCell rotator = newCell.AddComponent<RotatingCell>();
            rotator.rect = rect;
            rotatingCells.Add(rotator);

            generatedCells.Add(rect);
        }

        for (int i = generatedCells.Count - 1; i >= desiredCount; i--)
        {
            RectTransform cell = generatedCells[i];
            if (cell != null)
            {
                if (Application.isPlaying)
                    Destroy(cell.gameObject);
                else
                    DestroyImmediate(cell.gameObject);
            }

            if (i < rotatingCells.Count)
                rotatingCells.RemoveAt(i);
            generatedCells.RemoveAt(i);
        }

        float totalWidth = (columns - 1) * spacingX;
        float totalHeight = (rows - 1) * spacingY;

        int index = 0;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                float posX = x * spacingX - totalWidth * 0.5f;
                float posY = y * -spacingY + totalHeight * 0.5f;
                
                float rowStagger = (y % 2 == 0) ? spacingX * 0.5f : 0f;
                posX += rowStagger;

                RectTransform rect = generatedCells[index];
                Image image = rect.GetComponent<Image>();

                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = cellSize;
                rect.anchoredPosition = new Vector2(posX, posY);
                rect.localScale = Vector3.one;

                RotatingCell rotator = rect.GetComponent<RotatingCell>();
                if (rotator != null)
                    rotator.rowIndex = y;

                rect.name = $"Cell_{x}_{y}";
                index++;
            }
        }

        CacheAndRandomizeRotations();
        isRefreshing = false;
    }

    private void CacheGeneratedCells(RectTransform parentRect)
    {
        generatedCells.Clear();

        for (int i = 0; i < parentRect.childCount; i++)
        {
            RectTransform child = parentRect.GetChild(i) as RectTransform;
            if (child == null)
                continue;

            if (!child.name.StartsWith("Cell_"))
                continue;

            generatedCells.Add(child);
        }
    }

    private GameObject FindCanvasObjectByTag(string tag)
    {
        GameObject activeTaggedObject = GameObject.FindWithTag(tag);
        if (activeTaggedObject != null)
            return activeTaggedObject;

        Canvas[] canvases = Resources.FindObjectsOfTypeAll<Canvas>();
        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas canvas = canvases[i];
            if (canvas == null || canvas.gameObject == null)
                continue;

            if (!canvas.gameObject.scene.IsValid())
                continue;

            if (canvas.CompareTag(tag))
                return canvas.gameObject;
        }

        return null;
    }

    private Sprite GetRandomIconSprite()
    {
        Sprite[] sprites = new Sprite[] { iconSprite1, iconSprite2, iconSprite3, iconSprite4 };

        int validCount = 0;
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] != null)
                validCount++;
        }

        if (validCount == 0)
            return null;

        int choice = Random.Range(0, validCount);
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] == null)
                continue;

            if (choice == 0)
                return sprites[i];

            choice--;
        }

        return null;
    }

    private void CacheAndRandomizeRotations()
    {
        rotatingCells.Clear();
        
        for (int i = 0; i < generatedCells.Count; i++)
        {
            RectTransform cell = generatedCells[i];
            if (cell == null)
                continue;
            
            RotatingCell rotator = cell.GetComponent<RotatingCell>();
            if (rotator == null)
                rotator = cell.gameObject.AddComponent<RotatingCell>();
            
            rotator.rect = cell;
            float speed = Random.Range(15f, 90f);
            rotator.rotationSpeed = Random.value > 0.5f ? speed : -speed;
            
            rotatingCells.Add(rotator);
        }
    }

    private class RotatingCell : MonoBehaviour
    {
        public RectTransform rect;
        public int rowIndex;
        public float rotationSpeed;
    }
}
