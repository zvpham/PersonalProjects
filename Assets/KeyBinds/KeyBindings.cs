using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    
public class KeyBindings : MonoBehaviour
{

    private InputManager inputManager;
    public static KeyBindings instance;
    public GameObject keybindmanager;
    public Dictionary<string, List<KeyCode>> defaultActionKeyBinds = new Dictionary<string, List<KeyCode>>();
    public Dictionary<string, List<KeyCode>> actionkeyBinds = new Dictionary<string, List<KeyCode>>();
    // Start is called before the first frame update
    void Awake()
    {
        /*
       defaultActionKeyBinds.Add("Jump", List<KeyCode>{KeyCode.});
       defaultActionKeyBinds.Add("Interact", List<KeyCode>{KeyCode.});
       defaultActionKeyBinds.Add("Test", List<KeyCode>{KeyCode.});
        */
       defaultActionKeyBinds.Add("Move_Northeast", new List<KeyCode>() { KeyCode.Keypad9 });
       defaultActionKeyBinds.Add("Move_North", new List<KeyCode>() { KeyCode.Keypad8 });
       defaultActionKeyBinds.Add("Move_Northwest", new List<KeyCode>() { KeyCode.Keypad7 });
       defaultActionKeyBinds.Add("Move_West", new List<KeyCode>() { KeyCode.Keypad4 });
       defaultActionKeyBinds.Add("Move_Southwest", new List<KeyCode>() { KeyCode.Keypad1 });
       defaultActionKeyBinds.Add("Move_South", new List<KeyCode>() { KeyCode.Keypad2 });
       defaultActionKeyBinds.Add("Move_Southeast", new List<KeyCode>() { KeyCode.Keypad3 });
       defaultActionKeyBinds.Add("Move_East", new List<KeyCode>() { KeyCode.Keypad6 });
       defaultActionKeyBinds.Add("Pause", new List<KeyCode>() { KeyCode.Escape });
       defaultActionKeyBinds.Add("Wait", new List<KeyCode>() { KeyCode.Keypad5 });
       defaultActionKeyBinds.Add("Back", new List<KeyCode>() { KeyCode.Backspace });
       defaultActionKeyBinds.Add("InventoryMenu", new List<KeyCode>() { KeyCode.I });

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
    
