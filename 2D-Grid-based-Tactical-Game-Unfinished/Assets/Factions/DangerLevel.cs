using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Faction/DangerLevel")]
public class DangerLevel : ScriptableObject
{
    public List<Unit> meleeHeroes = new List<Unit>();
    public List<Unit> rangedHeroes = new List<Unit>();
    public List<Unit> supportHeroes = new List<Unit>();

    public List<UnitGroup> meleeUnitGroups = new List<UnitGroup>();
    public List<UnitGroup> rangedUnitGroups = new List<UnitGroup>();
    public List<UnitGroup> supportHUnitGroups = new List<UnitGroup>();
}
