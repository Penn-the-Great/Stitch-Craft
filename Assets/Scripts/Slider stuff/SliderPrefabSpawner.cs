using UnityEngine;

public class SliderPrefabSpawner : MonoBehaviour
{
    public GameObject sliderFaderPrefab;
    public string nextSceneName;

    public void OnButtonClick()
    {
        if (sliderFaderPrefab == null)
        {
            Debug.LogError("SliderPrefabSpawner: slider fader prefab is not assigned.");
            return;
        }

        Canvas canvas = GameObject.FindGameObjectWithTag("Slider Canvas")?.GetComponent<Canvas>();
        if (canvas == null)
            canvas = FindObjectOfType<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("SliderPrefabSpawner: no Canvas is available for the transition.");
            return;
        }

        var faderObj = Instantiate(sliderFaderPrefab, canvas.transform, false);
        var fader = faderObj.GetComponent<SceneSlider>();
        if (fader == null)
        {
            Debug.LogError("SliderPrefabSpawner: the fader prefab has no SceneSlider component.");
            Destroy(faderObj);
            return;
        }

        fader.BeginTransition(nextSceneName);
    }
}