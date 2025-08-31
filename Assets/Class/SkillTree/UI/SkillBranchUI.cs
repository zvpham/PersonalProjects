using System.Collections.Generic;
using UnityEngine;
using System;

public class SkillBranchUI : MonoBehaviour
{
    public List<SkillNodeUI> skillNodes;
    public List<SkillNodeUI> startNodes;

    public event Action<SkillNodeUI> skillNodeSelected;

    public void Start()
    {
        for(int i = 0; i < skillNodes.Count; i++)
        {
            skillNodes[i].OnClick += SkillNodeSelected;
        }
    }

    public void SkillNodeSelected(SkillNodeUI skillNode)
    {
        skillNodeSelected?.Invoke(skillNode);
    }

    public void ResetSkillBranch()
    {
        for(int i = 0; i < skillNodes.Count;i++)
        {
            skillNodes[i].Lock();
        }
    }
}
