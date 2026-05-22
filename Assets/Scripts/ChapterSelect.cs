using UnityEngine;

public class ChapterSelect : MonoBehaviour
{
    // The variable you want to change
    public int Chapter = 0;

    // This public function can be called by the button
    public void SetChapter(int newValue)
    {
        Chapter = newValue;
        Debug.Log("Variable set to: " + Chapter);
    }
}