using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillNodeDescription : MonoBehaviour
{
    public SkillNodeDescriptionUI skillNodeDescriptionUI;

    public List<bool> skillModifiersUnlocked = new List<bool>();

    public void Start()
    {
        skillNodeDescriptionUI.OnSkillNodeModifierClicked += SelectSkillModifier;
    }
    public void SelectSkillModifier(SkillNodeUI skillNodeModifier)
    {
        
    }

    public void SetSkill(SkillSO skill)
    {
        skillModifiersUnlocked.Clear();
        for(int i = 0; i < skill.modifiers.Count; i++)
        {
            skillModifiersUnlocked.Add(false);
        }
    }

    public void LoadSKill(SkillSO skill, bool unlocked)
    {
        skillModifiersUnlocked.Clear();
        skillModifiersUnlocked.Add(unlocked);
    }
}
