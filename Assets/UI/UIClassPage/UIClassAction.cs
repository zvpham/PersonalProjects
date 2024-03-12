using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIClassAction : MonoBehaviour
{
    private string actionDescrtiption;
    public TMP_Text actionName;

    public void SetActionDescription(string actionDescription)
    {
        actionDescrtiption = actionDescription;
    }

    public string GetActionDescription()
    {
        return actionDescrtiption;
    }
}
