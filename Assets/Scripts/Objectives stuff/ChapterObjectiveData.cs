using UnityEngine;

[CreateAssetMenu(menuName = "StitchCraft/Chapter Objective Data")]
public class ChapterObjectiveData : ScriptableObject
{
    public int chapterNumber;
    public string chapterId;
    public string styleName;
    public int requiredActors = 1;
    public int requiredCompletedPieces = 1;

    [Header("Display Dots (visual only)")]
    public Color colorA = Color.white;
    public Color colorB = Color.white;
    public Color colorC = Color.white;

    [Header("Accepted Families")]
    public ColorFamily familyA = ColorFamily.Red;
    public ColorFamily familyB = ColorFamily.Blue;
    public ColorFamily familyC = ColorFamily.Yellow;

    [TextArea] public string objectiveDescription;
}