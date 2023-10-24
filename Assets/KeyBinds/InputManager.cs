using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    private KeyBindings keyBindings;
    public GameObject inputManager;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
            Destroy(inputManager);

        }
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        keyBindings = KeyBindings.instance;
    }

    public List<KeyCode> GetKeyForAction(ActionName KeyBindingAction)
    {
        foreach (ActionName key in keyBindings.defaultActionKeyBinds.Keys)
        {
            if (key == KeyBindingAction)
            {
                return keyBindings.defaultActionKeyBinds[key];
            }
        }
        KeyCode[] input = { KeyCode.None };
        List<KeyCode> temp = new List<KeyCode>(input);
        return  temp;
    }

    public bool GetKeyDown(ActionName action)
    {
        // detects if player is pressing button
        if (!Input.anyKey)
            return false;

        foreach (ActionName key in keyBindings.defaultActionKeyBinds.Keys)
        {
            if (key == action)
            {
                for (int i = 0; i < keyBindings.defaultActionKeyBinds[key].Count; i++)
                {
                    if (i == keyBindings.defaultActionKeyBinds[key].Count - 1)
                    {
                        return (Input.GetKeyDown(keyBindings.defaultActionKeyBinds[key][i]));
                    }
                    else
                    {
                        if (!Input.GetKey(keyBindings.defaultActionKeyBinds[key][i]))
                        {
                            return false;
                        }
                    }
                }
            }
        }

        foreach (ActionName key in keyBindings.actionkeyBinds.Keys)
        {
            if (key.Equals(action))
            {
                for (int i = 0; i < keyBindings.actionkeyBinds[key].Count; i++)
                {
                    if (i == keyBindings.actionkeyBinds[key].Count - 1)
                    {

                        return (Input.GetKeyDown(keyBindings.actionkeyBinds[key][i]));
                    }
                    else
                    {
                        if (!Input.GetKey(keyBindings.actionkeyBinds[key][i]))
                        {
                            return false;
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool GetKeyDownTargeting(DirectionName direction)
    {
        // detects if player is pressing button
        if (!Input.anyKey)
            return false;

        foreach (DirectionName key in keyBindings.targetingKeyBinds.Keys)
        {
            if (key == direction)
            {
                for (int i = 0; i < keyBindings.targetingKeyBinds[key].Count; i++)
                {
                    if (i == keyBindings.targetingKeyBinds[key].Count - 1)
                    {
                        return (Input.GetKeyDown(keyBindings.targetingKeyBinds[key][i]));
                    }
                    else
                    {
                        if (!Input.GetKey(keyBindings.targetingKeyBinds[key][i]))
                        {
                            return false;
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool GetKey(ActionName action)
    {
        // detects if player is pressing button
        if (!Input.anyKey)
            return false;

        foreach (ActionName key in keyBindings.defaultActionKeyBinds.Keys)
        {
            if (key.Equals(action))
            {
                for (int i = 0; i < keyBindings.defaultActionKeyBinds[key].Count; i++)
                {
                    if (i == keyBindings.defaultActionKeyBinds[key].Count - 1)
                    {

                        return (Input.GetKey(keyBindings.defaultActionKeyBinds[key][i]));
                    }
                    else
                    {
                        if (!Input.GetKey(keyBindings.defaultActionKeyBinds[key][i]))
                        {
                            return false;
                        }
                    }
                }
            }
        }

        foreach (ActionName key in keyBindings.defaultActionKeyBinds.Keys)
        {
            if (key == action)
            {
                for (int i = 0; i < keyBindings.defaultActionKeyBinds[key].Count; i++)
                {
                    if (i == keyBindings.defaultActionKeyBinds[key].Count - 1)
                    {
                        return (Input.GetKey(keyBindings.defaultActionKeyBinds[key][i]));
                    }
                    else
                    {
                        if (!Input.GetKey(keyBindings.defaultActionKeyBinds[key][i]))
                        {
                            return false;
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool GetKeyUp(ActionName action)
    {
        // detects if player is pressing button
        if (!Input.anyKey)
            return false;

        foreach (ActionName key in keyBindings.defaultActionKeyBinds.Keys)
        {
            if (key.Equals(action))
            {
                for (int i = 0; i < keyBindings.defaultActionKeyBinds[key].Count; i++)
                {
                    if (i == keyBindings.defaultActionKeyBinds[key].Count - 1)
                    {

                        return (Input.GetKeyUp(keyBindings.defaultActionKeyBinds[key][i]));
                    }
                    else
                    {
                        if (!Input.GetKey(keyBindings.defaultActionKeyBinds[key][i]))
                        {
                            return false;
                        }
                    }
                }
            }
        }

        foreach (ActionName key in keyBindings.defaultActionKeyBinds.Keys)
        {
            if (key == action)
            {
                for (int i = 0; i < keyBindings.defaultActionKeyBinds[key].Count; i++)
                {
                    if (i == keyBindings.defaultActionKeyBinds[key].Count - 1)
                    {
                        return (Input.GetKeyUp(keyBindings.defaultActionKeyBinds[key][i]));
                    }
                    else
                    {
                        if (!Input.GetKey(keyBindings.defaultActionKeyBinds[key][i]))
                        {
                            return false;
                        }
                    }
                }
            }
        }
        return false;
    }
}