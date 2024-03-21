using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class BaseUIPage : MonoBehaviour
{
    public GameMenu gameMenu;
    public List<BaseGameUIObject> onScreenUIObjects;
    public StaticUIMenuValues menuValues;
    public string indent;
    public string selectedIcon;
    public List<string> selectionNames;
    public virtual void SelectMenuObject(int itemIndex)
    {

    }

    public virtual void UseUI()
    {

    }

    public virtual void IndexUp()
    {

    }

    public virtual void IndexDown()
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
