using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIClass : BaseGameUIObject
{
    public Class Class;
    public bool interactable = true;

    public override void HoverUI()
    {     
        ChooseClassMenu classMenu = (ChooseClassMenu) UIPage;
        classMenu.SetClassDescription(Class);
    }


    public override void MouseUseUI()
    {
        int index = UIPage.activeUIObjects.IndexOf(this);
        ChooseClassMenu classMenu = (ChooseClassMenu)UIPage;
        classMenu.SetClassDescription(Class);
        UIPage.currentIndex = index;
        UIPage.UpdateBaseUIObjects();
        UIPage.previousMouseIndex = index;
    }

    public void Deactivate()
    {
        interactable = false;
        gameObject.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        gameObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
        SetText(indent + AddIndents(GetOriginalText()));
    }
    public void Activate()
    {
        interactable = true;
        gameObject.GetComponent<Image>().color = new Color(1, 1, 1, .5f);
        gameObject.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
}
