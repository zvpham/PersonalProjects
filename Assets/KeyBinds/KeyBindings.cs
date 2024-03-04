using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    
public class KeyBindings : MonoBehaviour
{
    public static KeyBindings instance;
    public GameObject keybindmanager;
    public Dictionary<ActionName, List<KeyCode>> defaultActionKeyBinds = new Dictionary<ActionName, List<KeyCode>>();
    public Dictionary<ActionName, List<KeyCode>> actionkeyBinds = new Dictionary<ActionName, List<KeyCode>>();
    public Dictionary<DirectionName, List<KeyCode>> targetingKeyBinds = new Dictionary<DirectionName, List<KeyCode>>();
    // Start is called before the first frame update
    void Awake()
    {
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
        defaultActionKeyBinds.Add(ActionName.OpenWorldMap, new List<KeyCode>() {KeyCode.KeypadMinus});

        targetingKeyBinds.Add(DirectionName.NorthEast, new List<KeyCode> { KeyCode.Keypad9 });
        targetingKeyBinds.Add(DirectionName.North, new List<KeyCode> { KeyCode.Keypad8 });
        targetingKeyBinds.Add(DirectionName.NorthWest, new List<KeyCode> { KeyCode.Keypad7 });
        targetingKeyBinds.Add(DirectionName.West, new List<KeyCode> { KeyCode.Keypad4 });
        targetingKeyBinds.Add(DirectionName.SouthWest, new List<KeyCode> { KeyCode.Keypad1 });
        targetingKeyBinds.Add(DirectionName.South, new List<KeyCode> { KeyCode.Keypad2 });
        targetingKeyBinds.Add(DirectionName.SouthEast, new List<KeyCode> { KeyCode.Keypad3 });
        targetingKeyBinds.Add(DirectionName.East, new List<KeyCode> { KeyCode.Keypad6 });

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
    
