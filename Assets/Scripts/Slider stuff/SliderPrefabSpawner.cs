using UnityEngine;

public class SliderPrefabSpawner : MonoBehaviour
{
    public GameObject sliderFaderPrefab;
    public string nextSceneName;

    public void OnButtonClick()
    {
        var faderObj = Instantiate(sliderFaderPrefab, GameObject.FindObjectOfType<Canvas>().transform);
        var fader = faderObj.GetComponent<SceneSlider>();
        fader.BeginTransition(nextSceneName);
    }
}