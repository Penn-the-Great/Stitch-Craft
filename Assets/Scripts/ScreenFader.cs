using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class ScreenFader2 : MonoBehaviour
{
    public Image fadeImage;
    [SerializeField] int sceneToLoad = 0;



        void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (fadeImage != null && fadeImage.canvas != null)
        {
            DontDestroyOnLoad(fadeImage.canvas.gameObject);
        }
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

    StartCoroutine(FadeInOut(1f, 2f));
}  
   
   
   
    public IEnumerator FadeIn(float duration)
    {
        Color c = fadeImage.color;
        for (float t = 0; t <duration; t += Time.deltaTime)
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
        for (float t = 0; t <duration; t += Time.deltaTime)
        {
            c.a = Mathf.Lerp(1, 0, t / duration);
            fadeImage.color = c;
            yield return null;
        }
        c.a = 0;
        fadeImage.color = c;
    }




}
