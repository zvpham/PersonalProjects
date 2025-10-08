using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class SkillNodeDescriptionUI : MonoBehaviour
{
    public TextMeshProUGUI skillDescritpion;
    public SkillBranchUI skillModifierBranch;
    public SkillTreeSystem skillTreeSystem;

    public event UnityAction<SkillNodeUI> OnSkillNodeModifierClicked;
    public void ResetText()
    {
        skillDescritpion.text = "";
    }

    public void SetText(string text)
    {
        skillDescritpion.text = text;
    }

    public void SetNewSkill(SkillBranchUI skillModifierBranch)
    {
        skillModifierBranch.skillNodeSelected += SelectedSkillModifier;   
    }

    public void SelectedSkillModifier(SkillNodeUI skillNodeModifier)
    {   
        OnSkillNodeModifierClicked?.Invoke(skillNodeModifier);
    }
}
