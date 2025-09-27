using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SkillTreeNodeData
{
    public SkillNodeConnectionUI connector;
    public SkillTreeNode connectedNode;
}

public abstract class SkillTreeNode : MonoBehaviour
{
    public List<SkillTreeNodeData> connectedNodes;

    public bool unlocked = false;

    public virtual void Unlock(Unit unit)
    {
        gameObject.GetComponent<SkillNodeUI>().Unlock();

        for(int i = 0; i <  connectedNodes.Count; i++)
        {
            if (connectedNodes[i].connectedNode.unlocked)
            {
                connectedNodes[i].connector.Unlock();
            }
        }
    }

    public virtual void Lock(Unit unit)
    {
        gameObject.GetComponent<SkillNodeUI>().Lock();
        for(int i = 0; i < connectedNodes.Count; i++)
        {
            connectedNodes[i].connector.Lock();
        }
    }

    public bool CheckIfAnyConnectedNodesUnlocked()
    {
        for(int i = 0; i < connectedNodes.Count; i++)
        {
            if(connectedNodes[i].connectedNode.unlocked)
            {
                return true;
            }
        }
        return false;
    }
}
