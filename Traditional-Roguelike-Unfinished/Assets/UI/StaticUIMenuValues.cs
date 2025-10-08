using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticUIMenuValues : MonoBehaviour
{
    public string indent;
    public string selectedIcon;
    public List<string> selectionNames;
    public static StaticUIMenuValues Instance;
    public void Awake()
    {
        Instance = this; 
    }
}
