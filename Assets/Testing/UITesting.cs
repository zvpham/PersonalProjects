using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITesting : MonoBehaviour
{
    public UIClassPage classPage;
    public Class testClass;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            classPage.AddClass(testClass);
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            classPage.UseUI();
        }
    }
}
