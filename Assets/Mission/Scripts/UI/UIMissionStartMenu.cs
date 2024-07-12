using Inventory.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMissionStartMenu : MonoBehaviour
{
    public UICharacterMenu currentHeroes;
    public void AddHero(Unit unit)
    {
        if(unit != null)
        {
            currentHeroes.AddProfile();
            currentHeroes.UpdateData(currentHeroes.listOfUIProfiles.Count, unit.unitProfile);
        }
        else
        {
            currentHeroes.AddEmptySpace();
        }
    }
}
