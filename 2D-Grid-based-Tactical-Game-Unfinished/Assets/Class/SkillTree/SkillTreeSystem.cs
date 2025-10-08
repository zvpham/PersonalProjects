using Inventory.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SkillTreeSystem : MonoBehaviour
{
    [SerializeField]
    public SkillBranchUI skillBranchOne;

    [SerializeField]
    public SkillBranchUI skillBranchTwo;

    [SerializeField]
    public SkillBranchUI skillBranchThree;

    [SerializeField]
    public ActionConfirmationMenu actionConfirmationMenu;

    public Unit currentUnit;

    public List<SkillNode> startNodes;

    public Dictionary<string, List<bool>> skillUnlockDictionary = new Dictionary<string, List<bool>>();

    public void Start()
    {
        skillBranchOne.skillNodeSelected += SkillNodeSelected;
        skillBranchTwo.skillNodeSelected += SkillNodeSelected;
        skillBranchThree.skillNodeSelected += SkillNodeSelected;

        skillBranchOne.ResetSkillBranch();
        skillBranchTwo.ResetSkillBranch();
        skillBranchThree.ResetSkillBranch();
    }       

    public void LoadUnitSkillTree(Unit unit)
    {
        currentUnit = unit;
        LoadSkillBranch(skillBranchOne, 1);
        LoadSkillBranch(skillBranchTwo, 2);
        LoadSkillBranch(skillBranchThree, 3);
    }

    public void LoadSkillBranch(SkillBranchUI skillBranch, int unitSkilLBranchIndex)
    {
        SkillBranch unitSkillBranch = currentUnit.skillBranches[unitSkilLBranchIndex].skillBranch;
        if(unitSkillBranch != null)
        {
            skillBranch.LoadSkillTree(unitSkillBranch);
            LoadStartNodes(skillBranch);
        }
    }

    public void LoadStartNodes(SkillBranchUI skillBranch)
    {
        for(int i = 0; i < skillBranch.startNodes.Count; i++)
        {
            startNodes.Add(skillBranch.startNodes[i].gameObject.GetComponent<SkillNode>());
        }
    }

    public void UnlockSKill(SkillNode skillNode)
    {
        if(skillNode.unlocked)
        {
            return;
        }

        if(startNodes.Contains(skillNode) || skillNode.CheckIfAnyConnectedNodesUnlocked())
        {
            skillNode.Unlock(currentUnit);
        }
    }

    // This should only be called when skill points are reset so there are no checks
    public void LockSkill(SkillNode skillNode)
    {
        skillNode.Lock(currentUnit);
    }

    public void SkillNodeSelected(SkillNodeUI skillNode)
    {
        SkillNode newSkillNode =  skillNode.gameObject.GetComponent<SkillNode>();
        if(newSkillNode.unlocked)
        {
            actionConfirmationMenu.ActivateCancelAction();

        }
        else
        {
            ActivateActionConfirmationMenu(
                () =>
                {
                    UnlockSKill(newSkillNode);
                },
                () =>
                {

                });
        }
    }

    public void ActivateActionConfirmationMenu(UnityAction confirmAction, UnityAction cancelAction)
    {
        actionConfirmationMenu.ActivateMenu(confirmAction, cancelAction);
    }

    public void ConfirmAction()
    {
        actionConfirmationMenu.ActivateConfirmAction();
    }
    public void CancelAction()
    {
        actionConfirmationMenu.ActivateCancelAction();
    }
}
