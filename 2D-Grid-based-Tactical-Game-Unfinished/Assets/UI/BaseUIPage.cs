using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class BaseUIPage : MonoBehaviour
{
    public List<BaseGameUIObject> onScreenUIObjects;
    public StaticUIMenuValues menuValues;
    public string indent;
    public string selectedIcon;
    public List<string> selectionNames;

    public RectTransform contentPanel;

    public List<BaseGameUIObject> activeUIObjects = new List<BaseGameUIObject>();
    public int previousMouseIndex;
    public int currentIndex;
    public int topIndex;
    public int bottomIndex;
    public int maxUIObjectsVisibleOnScreen;
    public virtual void SelectMenuObject(int itemIndex)
    {

    }

    public virtual void UseUI()
    {

    }

    public virtual void UseHoverUI()
    {

    }

    public virtual void UpdateBaseUIObjects()
    {

    }

    public virtual void IndexUp()
    {

    }

    public virtual void IndexDown()
    {

    }

    public virtual void MouseMoved()
    {

    }

    public virtual void ResetPage()
    {

    }

    public virtual void Start()
    {
        menuValues = StaticUIMenuValues.Instance;
        indent = menuValues.indent;
        selectedIcon = menuValues.selectedIcon;
        selectionNames = menuValues.selectionNames;
    }   
}
