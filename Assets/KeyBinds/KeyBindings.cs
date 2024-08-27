using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    
public class KeyBindings : MonoBehaviour
{
    public static KeyBindings instance;
    public GameObject keybindmanager;
    public Dictionary<PlayerActionName, List<KeyCode>> defaultActionKeyBinds = new Dictionary<PlayerActionName, List<KeyCode>>();
    public Dictionary<MenuActionName, List<KeyCode>> menuActionKeyBinds = new Dictionary<MenuActionName, List<KeyCode>>();
    // Start is called before the first frame update
    void Awake()
    {
        defaultActionKeyBinds.Add(PlayerActionName.ActionOne, new List<KeyCode>() { KeyCode.Alpha1 });
        defaultActionKeyBinds.Add(PlayerActionName.ActionTwo, new List<KeyCode>() { KeyCode.Alpha2 });
        defaultActionKeyBinds.Add(PlayerActionName.ActionThree, new List<KeyCode>() { KeyCode.Alpha3 });
        defaultActionKeyBinds.Add(PlayerActionName.ActionFour, new List<KeyCode>() { KeyCode.Alpha4 });
        defaultActionKeyBinds.Add(PlayerActionName.SwitchUnits, new List<KeyCode>() { KeyCode.Tab });
        defaultActionKeyBinds.Add(PlayerActionName.ConfirmAction, new List<KeyCode>() { KeyCode.Return });
        defaultActionKeyBinds.Add(PlayerActionName.CancelAction, new List<KeyCode>() { KeyCode.Escape });
        defaultActionKeyBinds.Add(PlayerActionName.EndTurn, new List<KeyCode>() { KeyCode.F });
        defaultActionKeyBinds.Add(PlayerActionName.NextItemAction, new List<KeyCode>() { KeyCode.E });
        defaultActionKeyBinds.Add(PlayerActionName.PreviousItemAction, new List<KeyCode>() { KeyCode.Q });

        menuActionKeyBinds.Add(MenuActionName.Back, new List<KeyCode>() { KeyCode.Backspace });
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
    
