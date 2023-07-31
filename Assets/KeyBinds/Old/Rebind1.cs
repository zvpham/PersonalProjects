/*
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rebind : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private KeyBindings keyBindings;
    [SerializeField] private KeyBindingActions action;
    [SerializeField] private TMPro.TextMeshProUGUI buttontext;

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
                    values[i] = KeyCode.None;
                    break;

                case KeyCode.LeftShift:
                    values[i] = KeyCode.None;
                    break;
            }
            keys = new bool[values.Length];
        }
    }

    void Start()
    {
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
                        newKeyBinds.Add(KeyCode.LeftControl);
                    }

                    if (Input.GetKey(KeyCode.LeftAlt))
                    {
                        newKeyBinds.Add(KeyCode.LeftAlt);
                    }

                    if (Input.GetKey(KeyCode.LeftShift))
                    {
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
        newKeyBinds.Clear();
    }

    public void KeyChange(KeyBindingActions action, List<KeyCode> newKeyBinds)
    {
        foreach (KeyBindings.KeyBindingCheck keyBindingCheck in keyBindings.keyBindingChecks)
        {
            if (keyBindingCheck.keyBindingAction == action)
            {
                if (CheckDuplicateBindings(action))
                {
                    Debug.Log("Duplicate Keybinding found");
                    return;
                }
                keyBindingCheck.currentKeyBinds.Clear();
                keyBindingCheck.currentKeyBinds.AddRange(newKeyBinds);
                changeDisplay();
            }
        }
        enabled = false;
    }

    public void changeDisplay()
    {
        var newText = string.Empty;
        if(buttontext != null)
        {
            foreach(KeyCode key in newKeyBinds)
            {
                newText += key.ToString();
                if(newKeyBinds.Count != 1)
                {
                    newText += " + ";
                }
            }
            buttontext.SetText(newText);
        }
    }

    private bool CheckDuplicateBindings(KeyBindingActions action)
    {

        foreach(KeyBindings.KeyBindingCheck keyBindingCheck in keyBindings.keyBindingChecks)
        {

            // check if same action
            if(keyBindingCheck.keyBindingAction == action)
            {
                continue;
            }

            if(keyBindingCheck.currentKeyBinds.Count != 0)
            {
                if(keyBindingCheck.currentKeyBinds.Count == newKeyBinds.Count)
                {
                    if(!IsAllSameKeyBinds(keyBindingCheck.currentKeyBinds, newKeyBinds))
                    {
                        Debug.Log("break 1");
                        break;
                    }
                    return SwapKeyBinds(action, keyBindingCheck);
                }
            }
            else
            {   
                if (newKeyBinds.Count == 1 && newKeyBinds[0] == keyBindingCheck.defaultKeyBind)
                {
                    return SwapKeyBinds(action, keyBindingCheck);
                }
            }
        }

        return false;
    }

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

    public bool SwapKeyBinds(KeyBindingActions action, KeyBindings.KeyBindingCheck keyBindingCheck)
    {
        Debug.Log("Test 124301287409128");
        while (Input.anyKeyDown)
        {
            Debug.Log("Test123");
        }
        if (Input.GetKey(KeyCode.Y))
        {
            keyBindingCheck.currentKeyBinds.Clear();
            keyBindingCheck.currentKeyBinds.Add(KeyCode.None);
            return false;
        }
        return true;
    }

}

*/
