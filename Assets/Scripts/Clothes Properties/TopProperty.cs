using UnityEngine;

[System.Serializable]
public class TopProperty : MonoBehaviour
{
    [Header("Identity")]
    public string presetName;
    public string displayName;
    public string piece;

    [Header("Visual / Style")]
    public Color color = Color.white;
    public string material;
    public string style;

    [Header("Quality / Economy")]
    public char grade;
    public int cost;

    [Header("Debug / Objective Matching")]
    [SerializeField] private bool cacheDetectedColorFamily = true;
    [SerializeField] private ColorFamily detectedFamily = ColorFamily.White;

    public ColorFamily DetectedFamily => detectedFamily;

    private void Awake()
    {
        if (cacheDetectedColorFamily)
            detectedFamily = ColorFamilyUtil.GetFamily(color);
    }

    private void OnValidate()
    {
        if (cacheDetectedColorFamily)
            detectedFamily = ColorFamilyUtil.GetFamily(color);
    }

    public void SetColor(Color newColor)
    {
        color = newColor;
        if (cacheDetectedColorFamily)
            detectedFamily = ColorFamilyUtil.GetFamily(color);
    }

    public bool MatchesObjectiveFamilies(ChapterObjectiveData objective)
    {
        if (objective == null) return false;

        ColorFamily family = ColorFamilyUtil.GetFamily(color);
        return family == objective.familyA ||
               family == objective.familyB ||
               family == objective.familyC;
    }
}

public class CustomPropertiesBehaviour : MonoBehaviour
{
    [Header("Mirrored Properties")]
    public string displayName;
    public Color color;
    public string material;
    public string style;
    public char grade;
    public string piece;
    public int cost;

    [Header("Debug / Objective Matching")]
    [SerializeField] private bool cacheDetectedColorFamily = true;
    [SerializeField] private ColorFamily detectedFamily = ColorFamily.White;

    public ColorFamily DetectedFamily => detectedFamily;

    public void ApplyProperties(TopProperty properties)
    {
        if (properties == null) return;

        displayName = properties.displayName;
        material = properties.material;
        color = properties.color;
        style = properties.style;
        grade = properties.grade;
        piece = properties.piece;
        cost = properties.cost;

        if (cacheDetectedColorFamily)
            detectedFamily = ColorFamilyUtil.GetFamily(color);

        // Apply values to visuals/UI/etc. here
    }
}