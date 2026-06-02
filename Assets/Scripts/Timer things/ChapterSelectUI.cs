using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChapterSelectUI : MonoBehaviour
{
    // Create buttons for each chapter (1-10)
    public Button chapter1Button;
    public Button chapter2Button;
    public Button chapter3Button;
    public Button chapter4Button;
    public Button chapter5Button;
    public Button chapter6Button;
    public Button chapter7Button;
    public Button chapter8Button;
    public Button chapter9Button;
    public Button chapter10Button;
    public Button FreePlayButton;
    
    // ... etc for all 10 chapters

    void Start()
    {
        chapter1Button.onClick.AddListener(() => SelectChapter(1));
        chapter2Button.onClick.AddListener(() => SelectChapter(2));
        chapter3Button.onClick.AddListener(() => SelectChapter(3));
        chapter4Button.onClick.AddListener(() => SelectChapter(4));
        chapter5Button.onClick.AddListener(() => SelectChapter(5));
        chapter6Button.onClick.AddListener(() => SelectChapter(6));
        chapter7Button.onClick.AddListener(() => SelectChapter(7));
        chapter8Button.onClick.AddListener(() => SelectChapter(8));
        chapter9Button.onClick.AddListener(() => SelectChapter(9));
        chapter10Button.onClick.AddListener(() => SelectChapter(10));
        FreePlayButton.onClick.AddListener(() => SelectChapter(11)); 
    }


    public void SelectChapter(int chapterNumber)
    {
        if (PersistentGameData.Instance != null)
        {
            PersistentGameData.Instance.selectedChapter = chapterNumber;
        }

        SceneTransitionManager.Instance.TransitionToScene("Shop", LoadSceneMode.Single);
    }
    
}