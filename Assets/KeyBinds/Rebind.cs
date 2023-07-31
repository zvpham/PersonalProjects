using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rebind : MonoBehaviour
{
    // Start is called before the first frame update
    public string action;
    [SerializeField] private TMPro.TextMeshProUGUI buttontext;

    private KeyBindings keyBindings;

    private KeyCode[] values;
    private bool[] keys;
    private List<KeyCode> newKeyBinds = new List<KeyCode>();


    void Awake()
    {
        // gets all KeyCodes into a list to compare to
        values = (KeyCode[])System.Enum.GetValues(typeof(KeyCode));
        for (int i = 0; i < values.Length; i++)
        {
            switch (values[i])
            {
                case KeyCode.LeftAlt:
                    values[i] = KeyCode.None;
                    break;

                case KeyCode.LeftControl:
                    Debug.Log("LEft Control 1");
                    values[i] = KeyCode.None;
                    break;

                case KeyCode.LeftShift:
                    Debug.Log("LEft Shift 1");
                    values[i] = KeyCode.None;
                    break;
            }
            keys = new bool[values.Length];
        }
    }

    void Start()
    {
        keyBindings = KeyBindings.instance;
        enabled = false;
    }

    void Update()
    {
        for (int i = 0; i < values.Length; i++)
        {
            keys[i] = Input.GetKey((KeyCode)values[i]);
            if (Input.GetKeyDown((KeyCode)values[i]))
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    Debug.Log("LEft Control 2");
                    newKeyBinds.Add(KeyCode.LeftControl);
                }

                if (Input.GetKey(KeyCode.LeftAlt))
                {
                    newKeyBinds.Add(KeyCode.LeftAlt);
                }

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    Debug.Log("LEft Shift 2");
                    newKeyBinds.Add(KeyCode.LeftShift);
                }
                newKeyBinds.Add(values[i]);
                KeyChange(action, newKeyBinds);
                newKeyBinds.Clear();
                Debug.Log(values[i]);
            }

        }
    }

    public void OnClick()
    {
        enabled = true;
        Debug.Log("Click");
        newKeyBinds.Clear();
    }

    public void KeyChange(string action, List<KeyCode> newKeyBinds)
    {
        foreach (string key in keyBindings.defaultActionKeyBinds.Keys)
        {
            if (key == action)
            {
                if (CheckDuplicateBindings(action))
                {
                    Debug.Log("Duplicate Keybinding found");
                    return;
                }
                keyBindings.defaultActionKeyBinds[key].Clear();
                keyBindings.defaultActionKeyBinds[key].AddRange(newKeyBinds);
                changeDisplay();
                foreach(KeyCode test in  newKeyBinds)
                {
                    Debug.Log(test);
                }
            }
        }
        enabled = false;
    }

    public void changeDisplay()
    {
        var newText = string.Empty;
        if (buttontext != null)
        {
            foreach (KeyCode key in newKeyBinds)
            {
                newText += key.ToString();
                if (newKeyBinds.Count != 1)
                {
                    newText += " + ";
                }
            }
            buttontext.SetText(newText);
        }
    }

    private bool CheckDuplicateBindings(string action)
    {
        foreach (string key in keyBindings.defaultActionKeyBinds.Keys)
        {
            Debug.Log(key.ToString() );
            // check if same action
            if (key == action)
            {
                Debug.Log("break 2");
                continue;
            }

            if (keyBindings.defaultActionKeyBinds[key].Count != 0)
            {
                if (keyBindings.defaultActionKeyBinds[key].Count == newKeyBinds.Count)
                {
                    if (!IsAllSameKeyBinds(keyBindings.defaultActionKeyBinds[key], newKeyBinds))
                    {
                        Debug.Log("break 1");
                        break;
                    }
                    Debug.Log("break 3");
                    return SwapKeyBinds(action, keyBindings.defaultActionKeyBinds[key]);
                }
            }
        }
        Debug.Log("break 4");
        return false;
    }

    // compare new and previous keybinds to see if they are the same
    public bool IsAllSameKeyBinds(List<KeyCode> currentKeybinds, List<KeyCode> newKeyBinds)
    {
        for (int i = 0; i < newKeyBinds.Count; i++)
        {
            if (newKeyBinds[i] != currentKeybinds[i])
            {
                return false;
            }
        }
        return true;
    }

    public bool SwapKeyBinds(string action, List<KeyCode> currentKeyBinds)
    {
        Debug.Log("Test 124301287409128");
        /*
        while (Input.anyKeyDown)
        {
            Debug.Log("Test123");
        }
        */
        if (Input.GetKey(KeyCode.Y))
        {
            currentKeyBinds.Clear();
            currentKeyBinds.Add(KeyCode.None);
            return false;
        }
        return true;
    }

}