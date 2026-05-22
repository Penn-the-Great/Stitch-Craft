using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class ScreenFader : MonoBehaviour
{
    public Image fadeImage;
    [SerializeField] int sceneToLoad = 0;
    private Coroutine fadeLoopCoroutine;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (fadeImage != null && fadeImage.canvas != null)
        {
            DontDestroyOnLoad(fadeImage.canvas.gameObject);
        }
    }

    // Call this method to fade and load a scene
    public void FadeAndLoadScene(float duration)
    {
        if (fadeLoopCoroutine != null)
        {
            StopCoroutine(fadeLoopCoroutine);
        }
        StartCoroutine(FadeAndLoadSceneCoroutine(duration));
        Debug.Log($"Fading and loading scene: {sceneToLoad}");
    }

    // Coroutine that handles fading and scene loading
    private IEnumerator FadeAndLoadSceneCoroutine(float duration)
    {
        // Fade IN before loading
        yield return StartCoroutine(FadeIn(duration));
        
        // Load the scene
        SceneManager.LoadScene(sceneToLoad);
        
        // IMPORTANT: The coroutine continues after LoadScene because of DontDestroyOnLoad
        // Now fade OUT on the new scene
        yield return StartCoroutine(FadeOut(duration));
    }

    void FadeAndLoad(float duration)
    {
        StartCoroutine(FadeIn(2f));
        Debug.Log("Fading");
    }

    public IEnumerator FadeInOut(float fadeDuration, float waitTime)
    {
        yield return StartCoroutine(FadeIn(fadeDuration));
        yield return new WaitForSeconds(waitTime);
        yield return StartCoroutine(FadeOut(fadeDuration));

        fadeLoopCoroutine = StartCoroutine(FadeInOut(1f, 2f));
    }

    public IEnumerator FadeIn(float duration)
    {
        Color c = fadeImage.color;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            c.a = Mathf.Lerp(0, 1, t / duration);
            fadeImage.color = c;
            yield return null;
        }
        c.a = 1;
        fadeImage.color = c;
    }

    public IEnumerator FadeOut(float duration)
    {
        Color c = fadeImage.color;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            c.a = Mathf.Lerp(1, 0, t / duration);
            fadeImage.color = c;
            yield return null;
        }
        c.a = 0;
        fadeImage.color = c;
    }
}