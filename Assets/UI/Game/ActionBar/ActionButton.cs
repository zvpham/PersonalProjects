using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour
{
    [SerializeField]
    private TMP_Text actionName;
    [SerializeField]
    private TMP_Text coolDown;

    public ActionName action;

    public event UnityAction<ActionName> useAction;

    public void ChangeActionName(string newActionName)
    {
        actionName.text = newActionName;
    }

    public void ChangeActionCoolDown(string newCoolDown)
    {
        if (newCoolDown.Equals("0"))
        {
            coolDown.text = null;
        }
        else
        {
            coolDown.text = newCoolDown;
        }
    }

    public void OnActionPressed()
    {
        useAction?.Invoke(action); 
    }
}
