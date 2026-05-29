using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class TopProperty : MonoBehaviour
{

    public string presetName;
    public string displayName;
    public Color color;
    public string material;
    public string style;
    public char grade;
    public string piece;

}

public class CustomPropertiesBehaviour : MonoBehaviour
{
    public string displayName;
    public Color color;
    public string material;
    public string style;
    public char grade;
    public string piece;


    public void ApplyProperties(TopProperty properties)
    {
        displayName = properties.displayName;
        material = properties.material;
        color = properties.color;
        style = properties.style;
        grade = properties.grade;
        piece = properties.piece;
        // Apply values to visuals/UI/etc. here
    }
}




