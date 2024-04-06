using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Leveling/mainLevel")]
public  class MainLevel : ScriptableObject
{
    public int XPThreshold;
    [SerializeField]
    private int increaseCommonClassAmount;
    [SerializeField]
    private int increaseUncommonLevelAmount;
    [SerializeField]
    private int increaseRareLevelAmount;

    [SerializeField]
    private bool unlockRandomJobClass;
    [SerializeField]
    private bool unlockRandomRaceClass;

    public void LevelUp(Player player)
    {
        player.maxCommonClasses += increaseCommonClassAmount;
        player.maxUncommonClasses += increaseUncommonLevelAmount;
        player.maxRareClasses += increaseRareLevelAmount;

        if (unlockRandomJobClass)
        {
            player.AquireRandomJobClass();
        }
        if (unlockRandomRaceClass)
        {
            player.AquireRandomJobClass();
        }
    }
}
