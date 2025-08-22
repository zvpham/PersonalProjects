using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "SkillTree/ SkillNode")]
public class SkillNodeSO : ScriptableObject
{
    [field: SerializeField]
    public Sprite itemImage { get; set; }

}
