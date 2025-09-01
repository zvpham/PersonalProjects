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

    public List<SkillNode> startNodes;

    public void Start()
    {
        skillBranchOne.skillNodeSelected += SkillNodeSelected;
        skillBranchTwo.skillNodeSelected += SkillNodeSelected;
        skillBranchThree.skillNodeSelected += SkillNodeSelected;

        skillBranchOne.ResetSkillBranch();
        skillBranchTwo.ResetSkillBranch();
        skillBranchThree.ResetSkillBranch();
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
            skillNode.Unlock();
        }
    }

    // This should only be called when skill points are reset so there are no checks
    public void LockSkill(SkillNode skillNode)
    {
        skillNode.Lock();
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
