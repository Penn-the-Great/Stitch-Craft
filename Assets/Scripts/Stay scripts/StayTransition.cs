using System.Collections.Generic;
using UnityEngine;

public class StayTransition : MonoBehaviour
{
    public static StayTransition instance;

    void Awake()
    {
      
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