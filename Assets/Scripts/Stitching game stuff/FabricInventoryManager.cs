using UnityEngine;
using System.Collections.Generic;

public class FabricInventoryManager : MonoBehaviour
{
    public static FabricInventoryManager Instance { get; private set; }

    [System.Serializable]
    public class FabricStack
    {
        public string material;
        public Color color;
    }

    [SerializeField] private List<FabricStack> fabrics = new List<FabricStack>();
    public List<FabricStack> Fabrics => fabrics;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddFabric(string material, Color color)
    {
        color.a = 1f;
        fabrics.Add(new FabricStack { material = material, color = color });
    }

    public bool TryUseFabric(FabricStack fabric)
    {
        if (fabric == null || !fabrics.Contains(fabric))
        return false;

        fabrics.Remove(fabric);

        return true;
    }
}