
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Test : MonoBehaviour
{

    private InputManager inputManager;

    // Start is called before the first frame update
    void Start()
    {
        inputManager = InputManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
       if(inputManager.GetKeyDown(ActionName.Back))
        {
        }

        if (inputManager.GetKeyDown(ActionName.Pause)) 
        {
            SceneManager.LoadScene("SettingsMenu");
        }

    }
}
