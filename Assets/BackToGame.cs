
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class BackToGame : MonoBehaviour
{
    private InputManager inputManager;

    // Start is called before the first frame update
    void Start()
    {
        inputManager = InputManager.instance;
        Debug.Log("Hello");
    }

    // Update is called once per frame
    void Update()
    {
        if (inputManager.GetKeyDown("Back"))
        {
            Debug.Log("Hello");
            SceneManager.LoadScene("Game");
        }

        else if (inputManager.GetKeyDown("Pause"))
        {
            SceneManager.LoadScene("SettingsMenu");
        }
    }

}

