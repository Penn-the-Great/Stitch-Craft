using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChapterSelectUI : MonoBehaviour
{
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

    [Header("Objectives")]
    [SerializeField] private ChapterObjectiveManager objectiveManager;

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
        ChapterObjectiveManager manager = objectiveManager != null ? objectiveManager : ChapterObjectiveManager.Instance;
        if (manager != null)
            manager.SelectChapter(chapterNumber);
        else
            Debug.LogWarning("No ChapterObjectiveManager found when selecting a chapter.");

        if (PersistentGameData.Instance != null)
            PersistentGameData.Instance.selectedChapter = chapterNumber;

        SceneTransitionManager.Instance.TransitionToScene("Shop", LoadSceneMode.Single);
    }
}