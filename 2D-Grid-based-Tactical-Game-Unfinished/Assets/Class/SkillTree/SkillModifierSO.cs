using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Class/SkllModifier")]
public class SkillModifierSO : ScriptableObject
{
    public Sprite modifierImage;
    public SkillModifierParameters skillModifierParameter;
    public string skillModifierDescriptionKey;
    public int value;
}
