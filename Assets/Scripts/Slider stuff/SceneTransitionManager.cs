using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    public GameObject sliderFaderPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Transition to a scene (single or additive load)
    /// </summary>
    public void TransitionToScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        StartCoroutine(DoTransition(sceneName, mode));
    }

    private System.Collections.IEnumerator DoTransition(string sceneName, LoadSceneMode mode)
    {
        if (sliderFaderPrefab)
        {
            var canvas = GameObject.FindGameObjectWithTag("Slider Canvas")?.transform 
                      ?? FindObjectOfType<Canvas>()?.transform;
            
            if (canvas)
            {
                var faderObj = Instantiate(sliderFaderPrefab, canvas);
                var fader = faderObj.GetComponent<SceneSlider>();
                
                if (fader)
                {
                    fader.BeginTransition(sceneName, mode);
                    yield break;
                }
            }
        }

        SceneManager.LoadScene(sceneName, mode);
    }
}