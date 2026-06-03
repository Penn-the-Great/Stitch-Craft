using UnityEngine;
using UnityEngine.SceneManagement;

public class ShopNavigation : MonoBehaviour
{

  

    public void GoToDesk()
    {
        SceneTransitionManager.Instance.TransitionToScene("Desk", LoadSceneMode.Additive);
    }

    public void GoToCompiler()
    {
        SceneTransitionManager.Instance.TransitionToScene("Compiler", LoadSceneMode.Additive);
    }

    public void GoToMeeting()
    {
        SceneTransitionManager.Instance.TransitionToScene("Meeting room", LoadSceneMode.Additive);
    }

        public void GoToStorage()
    {
        SceneTransitionManager.Instance.TransitionToScene("Storage", LoadSceneMode.Additive);
    }

        public void GoToStitching()
    {
        SceneTransitionManager.Instance.TransitionToScene("Stitching table", LoadSceneMode.Additive);
    }

      public void LeaveDesk()
    {
        SceneTransitionManager.Instance.UnloadScene("Desk");
    }

    public void LeaveCompiler()
    {
        SceneTransitionManager.Instance.UnloadScene("Compiler");
    }

    public void LeaveMeeting()
    {
        SceneTransitionManager.Instance.UnloadScene("Meeting room");
    }

    public void LeaveStorage()
    {
        SceneTransitionManager.Instance.UnloadScene("Storage");
    }

    public void LeaveStitching()
    {
        SceneTransitionManager.Instance.UnloadScene("Stitching table");
    }

    public void GoToEndScene()
    {
        SceneTransitionManager.Instance.TransitionToScene("Dialogue and end", LoadSceneMode.Single);
    }

        public void GoToEndSceneFreeplay()
    {
        SceneTransitionManager.Instance.TransitionToScene("Dialogue and end", LoadSceneMode.Single);
    }
}