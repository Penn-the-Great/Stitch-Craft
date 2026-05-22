using UnityEngine;

public class SliderPrefabSpawner : MonoBehaviour
{
    public GameObject sliderFaderPrefab; // assign in Inspector
    public string nextSceneName;         // set in Inspector (e.g., "MainMenu")

    public void OnButtonClick()
    {
        var faderObj = Instantiate(sliderFaderPrefab, GameObject.FindObjectOfType<Canvas>().transform);
        var fader = faderObj.GetComponent<SceneSlider>();
        fader.BeginTransition(nextSceneName);
    }
}