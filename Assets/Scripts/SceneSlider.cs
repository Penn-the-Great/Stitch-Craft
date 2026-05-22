using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneSlider : MonoBehaviour
{
    [SerializeField] private float slideDuration = 0.7f;
    [SerializeField] private string sceneToLoad; // set this when you spawn
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
                screenHeight = 1080f; // fallback
            }
        } else {
            Debug.LogError("No Slider Canvas (with tag) found!");
            screenHeight = 1080f; // fallback
        }

        // Start above the screen, regardless
        rectTransform.anchoredPosition = new Vector2(0, screenHeight);
    }

    public void BeginTransition(string nextScene)
    {
        sceneToLoad = nextScene;
        StartCoroutine(SlideAndChangeScene());
    }

    private IEnumerator SlideAndChangeScene()
    {
        // Slide down to cover screen
        yield return StartCoroutine(SlideTo(Vector2.zero, slideDuration));

        // Load scene
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneToLoad);
        while (!op.isDone)
            yield return null;

        yield return null; // wait for new scene frame

        // Slide up and off screen
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
        // S-curve ease: SmoothStep
        return t * t * (3f - 2f * t);
    }
}