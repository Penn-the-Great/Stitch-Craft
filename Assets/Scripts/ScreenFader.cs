using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class ScreenFader : MonoBehaviour
{
    public Image fadeImage;
    [SerializeField] private string sceneToFade;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (fadeImage != null && fadeImage.canvas != null)
        {
            DontDestroyOnLoad(fadeImage.canvas.gameObject);
        }
    }

    public void FadeButtonClick()
    {
        StartCoroutine(FadeInOut(1f, 2f));
    }

    public void StartFadeIn(float duration)
    {
        StartCoroutine(FadeIn(duration));
        Debug.Log("Fading");
    }

    public IEnumerator FadeAndLoadScene(float fadeDuration, bool fadeOutOnNewScene = true)
    {
        // Use unscaled delta time so fade works even when game is paused
        yield return StartCoroutine(FadeInUnscaled(fadeDuration));
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToFade);

        if (fadeOutOnNewScene)
        {
            // Reset timeScale before fading out on new scene
            Time.timeScale = 1f;
            yield return StartCoroutine(FadeOut(fadeDuration));
        }
    }

    public IEnumerator FadeInOut(float fadeDuration, float waitTime)
    {
        yield return StartCoroutine(FadeIn(fadeDuration));
        yield return new WaitForSeconds(waitTime);
        yield return StartCoroutine(FadeOut(fadeDuration));

        StartCoroutine(FadeInOut(1f, 2f));
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

    // Fade in using unscaled delta time (works when game is paused)
    public IEnumerator FadeInUnscaled(float duration)
    {
        Color c = fadeImage.color;
        for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
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
