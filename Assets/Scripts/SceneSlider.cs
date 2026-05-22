using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneSlider : MonoBehaviour
{
    [SerializeField] private float slideDuration = 0.7f;
    [SerializeField] private string sceneToLoad; // set this when you spawn
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        // Start above the screen
        rectTransform.anchoredPosition = new Vector2(0, rectTransform.rect.height);
    }

    public void BeginTransition(string nextScene)
    {
        sceneToLoad = nextScene;
        StartCoroutine(SlideAndChangeScene());
    }

    private IEnumerator SlideAndChangeScene()
    {
        // Slide down
        yield return StartCoroutine(SlideTo(Vector2.zero, slideDuration));

        // Load scene
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneToLoad);
        while (!op.isDone)
            yield return null;

        // Wait one frame for safety
        yield return null;

        // Slide up
        yield return StartCoroutine(SlideTo(new Vector2(0, rectTransform.rect.height), slideDuration));

        // Destroy fader
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
        float easedT = EaseInOut(t); // <--- the magic
        rectTransform.anchoredPosition = Vector2.Lerp(start, targetPosition, easedT);
        yield return null;
        }
        rectTransform.anchoredPosition = targetPosition;
    }

    private float EaseInOut(float t)
{
    // S-curve ease: SmoothStep
    return t * t * (3f - 2f * t);
    // Or use: return 0.5f - 0.5f * Mathf.Cos(Mathf.PI * t); // Sine
}
}