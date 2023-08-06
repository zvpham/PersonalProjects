using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Template")]
public class TemplateHolder : ScriptableObject
{
    public List<Action> Actions;
}
