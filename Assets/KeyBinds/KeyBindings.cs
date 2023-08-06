using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    
public class KeyBindings : MonoBehaviour
{

    private InputManager inputManager;
    public static KeyBindings instance;
    public GameObject keybindmanager;
    public Dictionary<ActionName, List<KeyCode>> defaultActionKeyBinds = new Dictionary<ActionName, List<KeyCode>>();
    public Dictionary<ActionName, List<KeyCode>> actionkeyBinds = new Dictionary<ActionName, List<KeyCode>>();
    // Start is called before the first frame update
    void Awake()
    {
        /*
       defaultActionKeyBinds.Add("Jump", List<KeyCode>{KeyCode.});
       defaultActionKeyBinds.Add("Interact", List<KeyCode>{KeyCode.});
       defaultActionKeyBinds.Add("Test", List<KeyCode>{KeyCode.});
        */
       defaultActionKeyBinds.Add(ActionName.MoveNorthEast, new List<KeyCode>() { KeyCode.Keypad9 });
       defaultActionKeyBinds.Add(ActionName.MoveNorth, new List<KeyCode>() { KeyCode.UpArrow });
       defaultActionKeyBinds.Add(ActionName.MoveNorthWest, new List<KeyCode>() { KeyCode.Keypad7 });
       defaultActionKeyBinds.Add(ActionName.MoveWest, new List<KeyCode>() { KeyCode.LeftArrow });
       defaultActionKeyBinds.Add(ActionName.MoveSouthWest, new List<KeyCode>() { KeyCode.Keypad1 });
       defaultActionKeyBinds.Add(ActionName.MoveSouth, new List<KeyCode>() { KeyCode.DownArrow });
       defaultActionKeyBinds.Add(ActionName.MoveSouthEast, new List<KeyCode>() { KeyCode.Keypad3 });
       defaultActionKeyBinds.Add(ActionName.MoveEast, new List<KeyCode>() { KeyCode.RightArrow });
       defaultActionKeyBinds.Add(ActionName.Pause, new List<KeyCode>() { KeyCode.Escape });
       defaultActionKeyBinds.Add(ActionName.Wait, new List<KeyCode>() { KeyCode.W });
       defaultActionKeyBinds.Add(ActionName.Back, new List<KeyCode>() { KeyCode.Backspace });
       defaultActionKeyBinds.Add(ActionName.InventoryMenu, new List<KeyCode>() { KeyCode.I });

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
            Destroy(keybindmanager);
        }
        DontDestroyOnLoad(this);
    }   
}
    
