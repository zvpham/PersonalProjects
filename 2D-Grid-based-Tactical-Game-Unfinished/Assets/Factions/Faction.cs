using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Faction/Faction")]
public class Faction : ScriptableObject
{
    public Sprite factionImage;
    public FactionName factionName;
    public List<DangerLevel> dangerLevelList = new List<DangerLevel>();
    public List<MissionDangerLevel> missionDangerLevels = new List<MissionDangerLevel>();
}

public enum FactionName
{
    Bandits,
    CityState
}


