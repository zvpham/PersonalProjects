using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameMenu : MonoBehaviour
{
    [SerializeField]
    private UIClassPage classPage;
    [SerializeField]
    private InputManager inputManager;
    private Dictionary<MenuInputNames, List<KeyCode>> menuKeybinds;

    public void Start()
    {
        inputManager = InputManager.instance;
        menuKeybinds = inputManager.GetMenuKeybinds();
    }

    // Update is called once per frame
    void Update()
    {
        int index = 0;
        foreach (MenuInputNames key in menuKeybinds.Keys)
        {
            if (inputManager.GetKeyDownMenu(key))
            {
                classPage.SelectMenuObject(index);
                break;
            }
            index += 1;
        }
    }
}