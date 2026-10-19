using UnityEngine;
using UnityEngine.UI;


public class DeleteStorageButton : MonoBehaviour
{
    [SerializeField] private bool debugMode = false;

    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(DeleteStorage);
        }
    }

    private void DeleteStorage()
    {
        if (StorageManager.Instance != null)
        {
            StorageManager.Instance.ClearStorage();
            if (debugMode) Debug.Log("Storage deleted!");

            // Destroy all hanger objects in storage
            DraggableLerpImage[] allHangers = FindObjectsOfType<DraggableLerpImage>();
            foreach (DraggableLerpImage hanger in allHangers)
            {
                if (hanger.IsInStorage)
                {
                    Destroy(hanger.gameObject);
                }
            }
        }
    }
}