using UnityEngine;

public class AudioManager : MonoBehaviour {
    public static AudioManager Instance;
    private AudioSource source;

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persistent across scenes
            source = GetComponent<AudioSource>();
        } else {
            Destroy(gameObject);
        }
    }

    public void PlayButtonSFX(AudioClip clip) {
        source.PlayOneShot(clip);
    }
}