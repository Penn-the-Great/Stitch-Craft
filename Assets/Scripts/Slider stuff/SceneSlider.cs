using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneSlider : MonoBehaviour
{
    [SerializeField] private float slideDuration = 0.7f;
    [SerializeField] private string sceneToLoad;
    private LoadSceneMode loadMode = LoadSceneMode.Single;
    private RectTransform rectTransform;
    private RectTransform canvasRectTransform;
    private float screenHeight;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        GameObject sliderCanvasObj = GameObject.FindGameObjectWithTag("Slider Canvas");
        if (sliderCanvasObj != null)
        {
            canvasRectTransform = sliderCanvasObj.GetComponent<RectTransform>();
            if (canvasRectTransform != null)
            {
                screenHeight = canvasRectTransform.rect.height;
            }
            else
            {
                Debug.LogError("Slider Canvas does not have RectTransform!");
                screenHeight = 1080f;
            }
        } else {
            Debug.LogError("No Slider Canvas (with tag) found!");
            screenHeight = 1080f;
        }

        rectTransform.anchoredPosition = new Vector2(0, screenHeight);
    }

    // Original method - single scene load
    public void BeginTransition(string nextScene)
    {
        BeginTransition(nextScene, LoadSceneMode.Single);
    }

    // New overload - supports additive loading
    public void BeginTransition(string nextScene, LoadSceneMode mode)
    {
        sceneToLoad = nextScene;
        loadMode = mode;
        StartCoroutine(SlideAndChangeScene());
    }

    private IEnumerator SlideAndChangeScene()
    {
        yield return StartCoroutine(SlideTo(Vector2.zero, slideDuration));

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneToLoad, loadMode);
        while (!op.isDone)
            yield return null;

        yield return null;

        yield return StartCoroutine(SlideTo(new Vector2(0, screenHeight), slideDuration));

        Destroy(gameObject);
    }

    private IEnumerator SlideTo(Vector2 targetPosition, float duration)
    {
        Vector2 start = rectTransform.anchoredPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = EaseInOut(t);
            rectTransform.anchoredPosition = Vector2.Lerp(start, targetPosition, easedT);
            yield return null;
        }
        rectTransform.anchoredPosition = targetPosition;
    }

    private float EaseInOut(float t)
    {
        return t * t * (3f - 2f * t);
    }
}