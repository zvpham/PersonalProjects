using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    
public class KeyBindings : MonoBehaviour
{
    public static KeyBindings instance;
    public GameObject keybindmanager;
    public Dictionary<ActionName, List<KeyCode>> defaultActionKeyBinds = new Dictionary<ActionName, List<KeyCode>>();
    public Dictionary<ActionName, List<KeyCode>> actionkeyBinds = new Dictionary<ActionName, List<KeyCode>>();
    public Dictionary<WorldMapTravelIntputName, List<KeyCode>> worldMapTravelKeyBinds = new Dictionary<WorldMapTravelIntputName, List<KeyCode>>();
    public Dictionary<DirectionName, List<KeyCode>> targetingKeyBinds = new Dictionary<DirectionName, List<KeyCode>>();
    public Dictionary<MenuInputNames, List<KeyCode>> menuKeyBinds = new Dictionary<MenuInputNames, List<KeyCode>>();
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
        defaultActionKeyBinds.Add(ActionName.ClassMenu, new List<KeyCode>() { KeyCode.I });
        defaultActionKeyBinds.Add(ActionName.OpenWorldMap, new List<KeyCode>() {KeyCode.KeypadMinus});

        worldMapTravelKeyBinds.Add(WorldMapTravelIntputName.CloseWorldMap, new List<KeyCode>() { KeyCode.KeypadMinus });
        worldMapTravelKeyBinds.Add(WorldMapTravelIntputName.EnterTile, new List<KeyCode>() { KeyCode.KeypadPlus });


        targetingKeyBinds.Add(DirectionName.NorthEast, new List<KeyCode> { KeyCode.Keypad9 });
        targetingKeyBinds.Add(DirectionName.North, new List<KeyCode> { KeyCode.Keypad8 });
        targetingKeyBinds.Add(DirectionName.NorthWest, new List<KeyCode> { KeyCode.Keypad7 });
        targetingKeyBinds.Add(DirectionName.West, new List<KeyCode> { KeyCode.Keypad4 });
        targetingKeyBinds.Add(DirectionName.SouthWest, new List<KeyCode> { KeyCode.Keypad1 });
        targetingKeyBinds.Add(DirectionName.South, new List<KeyCode> { KeyCode.Keypad2 });
        targetingKeyBinds.Add(DirectionName.SouthEast, new List<KeyCode> { KeyCode.Keypad3 });
        targetingKeyBinds.Add(DirectionName.East, new List<KeyCode> { KeyCode.Keypad6 });

        menuKeyBinds.Add(MenuInputNames.UIItem1, new List<KeyCode> { KeyCode.A });
        menuKeyBinds.Add(MenuInputNames.UIItem2, new List<KeyCode> { KeyCode.B });
        menuKeyBinds.Add(MenuInputNames.UIItem3, new List<KeyCode> { KeyCode.C });
        menuKeyBinds.Add(MenuInputNames.UIItem4, new List<KeyCode> { KeyCode.D });
        menuKeyBinds.Add(MenuInputNames.UIItem5, new List<KeyCode> { KeyCode.E});
        menuKeyBinds.Add(MenuInputNames.UIItem6, new List<KeyCode> { KeyCode.F });
        menuKeyBinds.Add(MenuInputNames.UIItem7, new List<KeyCode> { KeyCode.G });
        menuKeyBinds.Add(MenuInputNames.UIItem8, new List<KeyCode> { KeyCode.H });
        menuKeyBinds.Add(MenuInputNames.UIItem9, new List<KeyCode> { KeyCode.I });
        menuKeyBinds.Add(MenuInputNames.UIItem10, new List<KeyCode> { KeyCode.J });
        menuKeyBinds.Add(MenuInputNames.UIItem11, new List<KeyCode> { KeyCode.K });
        menuKeyBinds.Add(MenuInputNames.UIItem12, new List<KeyCode> { KeyCode.L });
        menuKeyBinds.Add(MenuInputNames.UIItem13, new List<KeyCode> { KeyCode.M });
        menuKeyBinds.Add(MenuInputNames.UIItem14, new List<KeyCode> { KeyCode.N });
        menuKeyBinds.Add(MenuInputNames.UIItem15, new List<KeyCode> { KeyCode.O });
        menuKeyBinds.Add(MenuInputNames.UIItem16, new List<KeyCode> { KeyCode.P });
        menuKeyBinds.Add(MenuInputNames.UIItem17, new List<KeyCode> { KeyCode.Q });
        menuKeyBinds.Add(MenuInputNames.UIItem18, new List<KeyCode> { KeyCode.R });
        menuKeyBinds.Add(MenuInputNames.UIItem19, new List<KeyCode> { KeyCode.S });
        menuKeyBinds.Add(MenuInputNames.UIItem20, new List<KeyCode> { KeyCode.T });
        menuKeyBinds.Add(MenuInputNames.UIItem21, new List<KeyCode> { KeyCode.U });
        menuKeyBinds.Add(MenuInputNames.UIItem22, new List<KeyCode> { KeyCode.V });
        menuKeyBinds.Add(MenuInputNames.UIItem23, new List<KeyCode> { KeyCode.W });
        menuKeyBinds.Add(MenuInputNames.UIItem24, new List<KeyCode> { KeyCode.X });
        menuKeyBinds.Add(MenuInputNames.UIItem25, new List<KeyCode> { KeyCode.Y });
        menuKeyBinds.Add(MenuInputNames.UIItem26, new List<KeyCode> { KeyCode.Z });
        menuKeyBinds.Add(MenuInputNames.CloseMenu, new List<KeyCode> { KeyCode.Escape });
        menuKeyBinds.Add(MenuInputNames.IndexUp, new List<KeyCode> { KeyCode.UpArrow });
        menuKeyBinds.Add(MenuInputNames.IndexDown, new List<KeyCode> { KeyCode.DownArrow });
        menuKeyBinds.Add(MenuInputNames.UseUi, new List<KeyCode> { KeyCode.Return });
        menuKeyBinds.Add(MenuInputNames.HoverUI, new List<KeyCode> { KeyCode.RightShift });

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
    
