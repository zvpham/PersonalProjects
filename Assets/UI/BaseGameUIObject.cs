using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseGameUIObject : MonoBehaviour
{
    public bool isHeadOfGroup;
    public List<GameObject> groupHeaders;
    public List<BaseGameUIObject> groupMembers;

    public void ExpandGroup()
    {
        this.gameObject.SetActive(true);
    }

    public void CollapseGroup()
    {
        this.gameObject.SetActive(false);
    }

    public void GetBaseUIOBject(List<BaseGameUIObject> baseGameUIObjects)
    {
        if (!isHeadOfGroup)
        {
            baseGameUIObjects.Add(this);
            return;
        }

        for(int i = 0; i < groupMembers.Count; i++)
        {
            if (groupMembers[i].gameObject.activeInHierarchy)
            {
                groupMembers[i].GetBaseUIOBject(baseGameUIObjects);
            }
        }
        baseGameUIObjects.Add(this);
    }


    public List<bool> GroupsActive()
    {
        List<bool> activeGroups = new List<bool>();
        for(int i = 0; i < groupHeaders.Count; i++)
        {
                activeGroups.Add(groupHeaders[i].activeSelf);
        }
        return activeGroups;
    }
}
