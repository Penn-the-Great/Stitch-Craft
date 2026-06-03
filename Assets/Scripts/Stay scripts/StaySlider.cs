using System.Collections.Generic;
using UnityEngine;

public class Stay : MonoBehaviour
{
    public static Stay instance;

    void Awake()
    {
        // If instance exists but the gameobject was destroyed, clear it
        if (instance != null && instance.gameObject == null)
        {
            instance = null;
        }

        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
}