using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MusicVolumeSlider : MonoBehaviour
{
    public Slider volumeSlider; // Assign in Inspector or via GetComponentInChildren

    private AudioSource musicSource;

IEnumerator FindSliderCoroutine()
{
    while (volumeSlider == null)
    {
       GameObject volumeSlider = GameObject.FindWithTag("MusicSlider");
        if (volumeSlider == null)
            yield return null; // Wait one frame
    }
   
}
void Update()
{
    StartCoroutine(FindSliderCoroutine());
}
   
    void OnEnable()
    {

      
     

        // Find the persistent music AudioSource (ideally tagged "Music")
        GameObject musicPlayer = GameObject.FindWithTag("Music");
        
        if (musicPlayer != null)
        {
            musicSource = musicPlayer.GetComponent<AudioSource>();
            // Initialize slider to current volume
            volumeSlider.value = musicSource.volume;
            // Remove old listeners to avoid stacking!
            volumeSlider.onValueChanged.RemoveAllListeners();
            // Add fresh listener
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
        else
        {
            Debug.LogWarning("Music AudioSource not found!");
        }

        volumeSlider.onValueChanged.RemoveAllListeners();
        volumeSlider.onValueChanged.AddListener(SetVolume);

        Debug.Log("Settings enabled");
Debug.Log("Slider object: " + volumeSlider);
Debug.Log("MusicPlayer found: " + musicPlayer);
Debug.Log("MusicSource assigned: " + (musicSource != null));
    }

    void SetVolume(float value)
    {
        if (musicSource != null)
            musicSource.volume = value;
    }
}