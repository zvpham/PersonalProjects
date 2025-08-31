using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillNode : MonoBehaviour
{
    public SkillNodeConnectionUI northConnector;
    public SkillNodeConnectionUI southConnector;
    public SkillNodeConnectionUI rightConnector;
    public SkillNodeConnectionUI leftConnector;

    public SkillNode northNode;
    public SkillNode southNode;
    public SkillNode leftNode;
    public SkillNode rightNode;

    public bool unlocked = false;

    public void Unlock()
    {
        gameObject.GetComponent<SkillNodeUI>().Unlock();

        if(northNode.unlocked)
        {
            northConnector.Unlock();
        }

        if(southNode.unlocked)
        {
            southConnector.Unlock();
        }

        if(leftNode.unlocked)
        {
            leftConnector.Unlock();
        }

        if(rightNode.unlocked)
        {
            rightConnector.Unlock();
        }
    }

    public void Lock()
    {
        gameObject.GetComponent<SkillNodeUI>().Unlock();
        northConnector.Lock();
        southConnector.Lock();
        leftConnector.Lock();
        rightConnector.Lock();

    }

    public bool CheckIfAnyConnectedNodesUnlocked()
    {
        bool foundUnlockedNode = false;
        if(northNode != null && northNode.unlocked)
        {
            foundUnlockedNode = true;
        }

        if (southNode != null && southNode.unlocked)
        {
            foundUnlockedNode = true;
        }

        if (leftNode != null && leftNode.unlocked)
        {
            foundUnlockedNode = true;
        }

        if (rightNode != null && rightNode.unlocked)
        {
            foundUnlockedNode = true;
        }
        return foundUnlockedNode;
    }
}
