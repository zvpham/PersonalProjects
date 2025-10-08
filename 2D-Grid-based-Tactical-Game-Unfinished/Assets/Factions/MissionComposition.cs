using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Faction/MissionComposition")]
public class MissionComposition : ScriptableObject
{
    public List<EnemyComposition> enemyCompositions;
    public List<EnemyComposition> allyCompositions;
}
