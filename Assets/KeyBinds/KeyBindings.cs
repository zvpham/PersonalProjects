using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    
public class KeyBindings : MonoBehaviour
{
    public static KeyBindings instance;
    public GameObject keybindmanager;
    public Dictionary<PlayerActionName, List<KeyCode>> defaultActionKeyBinds = new Dictionary<PlayerActionName, List<KeyCode>>();
    public Dictionary<MenuActionName, List<KeyCode>> menuActionKeyBinds = new Dictionary<MenuActionName, List<KeyCode>>();
    public Dictionary<TestActionName, List<KeyCode>> testActionKeyBinds = new Dictionary<TestActionName, List<KeyCode>>();
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
        defaultActionKeyBinds.Add(PlayerActionName.ResetTest, new List<KeyCode>() { KeyCode.P });

        menuActionKeyBinds.Add(MenuActionName.Back, new List<KeyCode>() { KeyCode.Backspace });

        testActionKeyBinds.Add(TestActionName.RaiseElevation, new List<KeyCode>() { KeyCode.Q });
        testActionKeyBinds.Add(TestActionName.LowerElevation, new List<KeyCode>() { KeyCode.E });
        testActionKeyBinds.Add(TestActionName.InteractWithInventory, new List<KeyCode>() { KeyCode.I });
        testActionKeyBinds.Add(TestActionName.TestEarthWall, new List<KeyCode>() { KeyCode.R });
        testActionKeyBinds.Add(TestActionName.RaiseViewElevation, new List<KeyCode>() { KeyCode.Z });
        testActionKeyBinds.Add(TestActionName.LowerViewElevation, new List<KeyCode>() { KeyCode.X });
        testActionKeyBinds.Add(TestActionName.PlacePlayer, new List<KeyCode>() { KeyCode.Alpha1 });
        testActionKeyBinds.Add(TestActionName.PlaceTeam2, new List<KeyCode>() { KeyCode.Alpha2 });
        testActionKeyBinds.Add(TestActionName.PlaceTeam3, new List<KeyCode>() { KeyCode.Alpha3 });
        testActionKeyBinds.Add(TestActionName.PlaceTeam4, new List<KeyCode>() { KeyCode.Alpha4 });
        testActionKeyBinds.Add(TestActionName.RemoveUnit, new List<KeyCode>() { KeyCode.Backspace });
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
    
