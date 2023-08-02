using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptManager : MonoBehaviour
{
    public static ScriptManager instance;

    public List<MonoBehaviour> running = new List<MonoBehaviour>();
    public List<MonoBehaviour> onHold = new List<MonoBehaviour>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
