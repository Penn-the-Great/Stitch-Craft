using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChapterObjectiveUI : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text styleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text countText;

    [Header("Colors")]
    [SerializeField] private Image dotA;
    [SerializeField] private Image dotB;
    [SerializeField] private Image dotC;

    public void ShowObjectives(ChapterObjectiveData data, bool useActors = true)
    {
        if (data == null) return;

        styleText.text = $"Theme - {data.styleName}";
        int count = useActors ? data.requiredActors : data.requiredCompletedPieces;
        string label = useActors ? "Actors" : "Completed Pieces";
        countText.text = $"{label} Needed: {count}";
        descriptionText.text = data.objectiveDescription;

        dotA.color = data.colorA;
        dotB.color = data.colorB;
        dotC.color = data.colorC;
    }
}
