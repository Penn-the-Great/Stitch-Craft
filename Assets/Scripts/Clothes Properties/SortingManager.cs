using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class SortingManager : MonoBehaviour
{
    public static SortingManager Instance { get; private set; }

    [SerializeField] private Canvas storageCanvas;
    [SerializeField] private float repositionLerpSpeed = 5f;
    [SerializeField] private bool debugMode = false;

    private bool isSortModeActive = false;
    private List<DraggableLerpImage> sortedHangers = new List<DraggableLerpImage>();
    private Dictionary<DraggableLerpImage, Vector2> targetPositions = new Dictionary<DraggableLerpImage, Vector2>();
    private bool isLerping = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Update()
    {
        if (isLerping)
        {
            UpdateLerp();
        }
    }

    public void EnterSortMode()
    {
        isSortModeActive = true;
        if (debugMode) Debug.Log("Entered sort mode");
    }

    public void ExitSortMode()
    {
        isSortModeActive = false;
        RandomizeOrder();
        if (debugMode) Debug.Log("Exited sort mode - randomized");
    }

    public void SortByColor()
    {
        SortBy(SortType.Color);
    }

    public void SortByMaterial()
    {
        SortBy(SortType.Material);
    }

    public void SortByStyle()
    {
        SortBy(SortType.Style);
    }

    public void SortByGrade()
    {
        SortBy(SortType.Grade);
    }

    private void SortBy(SortType sortType)
    {
       
        GetStorageHangers();

        if (sortedHangers.Count == 0)
        {
            if (debugMode) Debug.LogWarning("No hangers in storage to sort");
            return;
        }

     
        switch (sortType)
        {
            case SortType.Color:
                sortedHangers = sortedHangers.OrderBy(h => GetColorOrder(h.GetComponent<TopProperty>().color)).ToList();
                if (debugMode) Debug.Log("Sorted by Color (Rainbow Order)");
                break;

            case SortType.Material:
                sortedHangers = sortedHangers.OrderBy(h => h.GetComponent<TopProperty>().material).ToList();
                if (debugMode) Debug.Log("Sorted by Material");
                break;

            case SortType.Style:
                sortedHangers = sortedHangers.OrderBy(h => h.GetComponent<TopProperty>().style).ToList();
                if (debugMode) Debug.Log("Sorted by Style");
                break;

            case SortType.Grade:
                sortedHangers = sortedHangers.OrderBy(h => h.GetComponent<TopProperty>().grade).ToList();
                if (debugMode) Debug.Log("Sorted by Grade");
                break;
        }

       
        CalculateTargetPositions();
        isLerping = true;
    }

    private void RandomizeOrder()
    {
        GetStorageHangers();

        if (sortedHangers.Count == 0) return;

       
        for (int i = sortedHangers.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            var temp = sortedHangers[i];
            sortedHangers[i] = sortedHangers[randomIndex];
            sortedHangers[randomIndex] = temp;
        }

        if (debugMode) Debug.Log("Randomized hanger order");

 
        CalculateTargetPositions();
        isLerping = true;
    }

    private void GetStorageHangers()
    {
        sortedHangers.Clear();


        DraggableLerpImage[] allHangers = storageCanvas.GetComponentsInChildren<DraggableLerpImage>();
        foreach (DraggableLerpImage hanger in allHangers)
        {
            if (hanger.IsInStorage)
            {
                sortedHangers.Add(hanger);
            }
        }
    }

    private void CalculateTargetPositions()
    {
        targetPositions.Clear();

        float xOffset = -300f;
        float xStep = sortedHangers.Count > 0 ? 600f / (sortedHangers.Count + 1) : 0;

        for (int i = 0; i < sortedHangers.Count; i++)
        {
            Vector2 targetPos = new Vector2(xOffset + (i + 1) * xStep, sortedHangers[i].GetComponent<RectTransform>().anchoredPosition.y);
            targetPositions[sortedHangers[i]] = targetPos;
        }
    }

    private void UpdateLerp()
    {
        bool allFinished = true;

        foreach (var kvp in targetPositions)
        {
            DraggableLerpImage hanger = kvp.Key;
            Vector2 targetPos = kvp.Value;

            if (hanger == null) continue;

            RectTransform rt = hanger.GetComponent<RectTransform>();
            if (rt == null) continue;

            Vector2 currentPos = rt.anchoredPosition;


            if (Vector2.Distance(currentPos, targetPos) > 1f)
            {
                allFinished = false;
                Vector2 newPos = Vector2.Lerp(currentPos, targetPos, Time.deltaTime * repositionLerpSpeed);
                rt.anchoredPosition = newPos;
            }
            else
            {
              
                rt.anchoredPosition = targetPos;
            }
        }

       
        if (allFinished)
        {
            isLerping = false;
            if (debugMode) Debug.Log("Lerp complete");
        }
    }

    private int GetColorOrder(Color color)
    {
       
        Color.RGBToHSV(color, out float hue, out float saturation, out float value);
        
        return Mathf.RoundToInt(hue * 360f);
    }

    public bool IsSortModeActive => isSortModeActive;

    private enum SortType
    {
        Color,
        Material,
        Style,
        Grade
    }
}