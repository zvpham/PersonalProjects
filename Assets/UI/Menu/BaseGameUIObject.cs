using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class BaseGameUIObject : MonoBehaviour
{
    public BaseUIPage UIPage;
    public bool isHeadOfGroup;
    public List<BaseGameUIObject> groupMembers;

    [SerializeField]
    private TMP_Text text;

    [SerializeField]
    private string originalText;
    [SerializeField]
    private int numIndents;

    private StaticUIMenuValues menuValues;

    public string indent;

    public void Awake()
    {
        if(StaticUIMenuValues.Instance != null)
        {
            menuValues = StaticUIMenuValues.Instance;
            indent = menuValues.indent;
        }
    }

    public void Start()
    {
        menuValues = StaticUIMenuValues.Instance;
        indent = menuValues.indent;
    }

    public void setAmountOfIndents(int indentAmount)
    {
        numIndents = indentAmount;
    }
    public void SetOriginalText(string newText)
    {
        originalText = newText;
    }

    public string GetOriginalText()
    {
        return originalText;
    }

    public string AddIndents(string unmodifiedText)
    {
        string indents = "";
        for(int i = 0; i < numIndents; i++)
        {
            indents += indent;
        }
        indents += unmodifiedText;
        return indents;
    }

    public void SetText(string newText)
    {
        text.text = newText;
    }

    public string GetText()
    {
        return text.text;
    }

    public void GetActiveBaseUIOBjects(List<BaseGameUIObject> baseGameUIObjects)
    {
        //Base Case - BaseGameUIObject has no GroupMembers
        baseGameUIObjects.Add(this);
        for (int i = 0; i < groupMembers.Count; i++)
        {
            if (groupMembers[i].gameObject.activeInHierarchy)
            {
                groupMembers[i].GetActiveBaseUIOBjects(baseGameUIObjects);
            }
        }
    }

    public void GetAllBaseUIOBjects(List<BaseGameUIObject> baseGameUIObjects)
    {
        //Base Case - BaseGameUIObject has no GroupMembers
        baseGameUIObjects.Add(this);
        if(!isHeadOfGroup)
        {
            return;
        }
        for (int i = 0; i < groupMembers.Count; i++)
        {
                groupMembers[i].GetAllBaseUIOBjects(baseGameUIObjects);
        }
    }

    public bool isGroupActive()
    {
        for (int i = 0; i < groupMembers.Count; i++)
        {
            if (groupMembers[i].gameObject.activeInHierarchy)
            {
                return true;
            }
        }
        return false;
    }

    public List<bool> ActiveGroups()
    {
        List<bool> activeGroups = new List<bool>(); 
        for(int i = 0;i < groupMembers.Count;i++)
        {
            activeGroups.Add(groupMembers[i].gameObject.activeInHierarchy);
        }
        return activeGroups;
    }

    public virtual void UseUI()
    {
        //Expanding And Collapsing group
        if (isHeadOfGroup)
        {
            bool isAnyGroupMemberActive = false;
            List<bool> activeGroups = ActiveGroups();
            for (int i = 0; i < activeGroups.Count; i++)
            {
                if (activeGroups[i])
                {
                    isAnyGroupMemberActive = true;
                    break;
                }
            }

            List<BaseGameUIObject> allGroupMembers = new List<BaseGameUIObject>();
            GetAllBaseUIOBjects(allGroupMembers);
            allGroupMembers.RemoveAt(0);

            if (isAnyGroupMemberActive)
            {
                for (int i = 0; i < allGroupMembers.Count; i++)
                {
                    allGroupMembers[i].gameObject.SetActive(false);
                }
            }
            else
            {
                for (int i = 0; i < allGroupMembers.Count; i++)
                {
                    allGroupMembers[i].gameObject.SetActive(true);
                }
            }
        }
    }
}
