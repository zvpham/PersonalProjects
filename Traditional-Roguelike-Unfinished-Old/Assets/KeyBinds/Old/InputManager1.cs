/*
 * using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    [SerializeField] private KeyBindings keyBindings;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this) 
        {
            Destroy(this);
        }
        DontDestroyOnLoad(this);
    }
    
    public KeyCode GetKeyForAction(KeyBindingActions KeyBindingAction)
    {
        foreach(KeyBindings.KeyBindingCheck keyBindingCheck in keyBindings.keyBindingChecks)
        {
           if(keyBindingCheck.keyBindingAction == KeyBindingAction)
            {
                return keyBindingCheck.defaultKeyBind;
            }
        }
        return KeyCode.None;
    }

    public bool GetKeyDown(KeyBindingActions action)
    {
        // detects if player is pressing button
        if (!Input.anyKey)
            return false;

        foreach (KeyBindings.KeyBindingCheck keyBindingCheck in keyBindings.keyBindingChecks)
        {
            if (keyBindingCheck.keyBindingAction == action)
            {
                if (keyBindingCheck.currentKeyBinds.Count != 0)
                {
                    for (int i = 0; i < keyBindingCheck.currentKeyBinds.Count; i++)
                    {
                        if(i == keyBindingCheck.currentKeyBinds.Count - 1)
                        {
                            return (Input.GetKeyDown(keyBindingCheck.currentKeyBinds[i]));
                        }
                        else
                        {
                            if (!Input.GetKey(keyBindingCheck.currentKeyBinds[i]))
                            {
                                return false;
                            }
                        }
                    }
                }
                else
                {
                    return Input.GetKeyDown(keyBindingCheck.defaultKeyBind);
                }
            }
        }
        return false;
    }

    public bool GetKey(KeyBindingActions action)
    {

        foreach (KeyBindings.KeyBindingCheck keyBindingCheck in keyBindings.keyBindingChecks)
        {
            //Debug.Log("6");
            if (keyBindingCheck.keyBindingAction == action)
            {
                //Debug.Log("5");
                if (keyBindingCheck.currentKeyBinds.Count != 0)
                {
                    Debug.Log("4");
                    for (int i = 0; i < keyBindingCheck.currentKeyBinds.Count; i++)
                    {
                        Debug.Log("3");
                        if (i == keyBindingCheck.currentKeyBinds.Count - 1)
                        {
                            Debug.Log("1");
                            return (Input.GetKey(keyBindingCheck.currentKeyBinds[i]));
                        }
                        else
                        {
                            if (!Input.GetKey(keyBindingCheck.currentKeyBinds[i]))
                            {
                                Debug.Log("2");
                                return false;
                            }
                        }
                    }
                }
                else
                {
                    return Input.GetKey(keyBindingCheck.defaultKeyBind);
                }
            }
        }
        return false;
    }

    public bool GetKeyUp(KeyBindingActions action)
    {
        foreach (KeyBindings.KeyBindingCheck keyBindingCheck in keyBindings.keyBindingChecks)
        {
            if (keyBindingCheck.keyBindingAction == action)
            {
                return Input.GetKeyUp(keyBindingCheck.defaultKeyBind);
            }
        }
        return false;
    }
}
*/