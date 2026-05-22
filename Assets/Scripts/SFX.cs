using System.Collections.Generic;
using UnityEngine;
 
public class SFX : MonoBehaviour
{
    public static SFX instance;
 
    void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
}