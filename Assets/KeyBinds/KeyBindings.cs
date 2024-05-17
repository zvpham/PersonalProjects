using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    
public class KeyBindings : MonoBehaviour
{
    public static KeyBindings instance;
    public GameObject keybindmanager;
    public Dictionary<PlayerActionName, List<KeyCode>> defaultActionKeyBinds = new Dictionary<PlayerActionName, List<KeyCode>>();
    // Start is called before the first frame update
    void Awake()
    {
        defaultActionKeyBinds.Add(PlayerActionName.ActionOne, new List<KeyCode>() { KeyCode.Alpha1 });
        defaultActionKeyBinds.Add(PlayerActionName.ActionTwo, new List<KeyCode>() { KeyCode.Alpha2 });
        defaultActionKeyBinds.Add(PlayerActionName.ActionThree, new List<KeyCode>() { KeyCode.Alpha3 });
        defaultActionKeyBinds.Add(PlayerActionName.ActionFour, new List<KeyCode>() { KeyCode.Alpha4 });
        defaultActionKeyBinds.Add(PlayerActionName.SwitchUnits, new List<KeyCode>() { KeyCode.Tab });


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
    
