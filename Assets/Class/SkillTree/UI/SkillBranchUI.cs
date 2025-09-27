using System.Collections.Generic;
using UnityEngine;
using System;

public class SkillBranchUI : MonoBehaviour
{
    public List<SkillNodeUI> skillNodes;
    public List<SkillNodeUI> startNodes;
    public GameObject skillTreePrefab;
    public GameObject emptySkillTreePrefab;

    public event Action<SkillNodeUI> skillNodeSelected;

    public void LoadSkillTree(SkillBranch skillTree)
    {
        ResetSkillBranch();
        if(skillTree == null)
        {
            skillTreePrefab = Instantiate(emptySkillTreePrefab);
        }
        else
        {
            skillTreePrefab = Instantiate(skillTree.skillBranchPrefab);
        }
        SkillTreePrefabUI skillTreePrefabUI =  skillTreePrefab.GetComponent<SkillTreePrefabUI>();
        skillNodes = skillTreePrefabUI.skillNodes;
        startNodes = skillTreePrefabUI.startNodes;
        ResetUnlocks();

        for (int i = 0; i < skillNodes.Count; i++)
        {
            skillNodes[i].OnClick += SkillNodeSelected;
            SkillSO skillInformation = skillNodes[i].gameObject.GetComponent<SkillSO>();
            skillNodes[i].LoadImage(skillInformation.skill.skillimage);
        }
    }

    public void SkillNodeSelected(SkillNodeUI skillNode)
    {
        skillNodeSelected?.Invoke(skillNode);
    }

    public void ResetUnlocks()
    {
        for(int i = 0; i < skillNodes.Count;i++)
        {
            skillNodes[i].Lock();
        }
    }

    public void ResetSkillBranch()
    {
        Destroy(skillTreePrefab);
        skillNodes.Clear();
        startNodes.Clear();
    }

}
