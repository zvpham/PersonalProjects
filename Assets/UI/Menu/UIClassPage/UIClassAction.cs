using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIClassAction : BaseGameUIObject
{
    private string actionDescrtiption;

    public void SetActionDescription(string actionDescription)
    {
        actionDescrtiption = actionDescription;
    }

    public string GetActionDescription()
    {
        return actionDescrtiption;
    }

    public override void UseUI()
    {
        UIClassPage uIClassPage = (UIClassPage)UIPage;
        uIClassPage.HandleChangeDescription(actionDescrtiption);
    }
}
