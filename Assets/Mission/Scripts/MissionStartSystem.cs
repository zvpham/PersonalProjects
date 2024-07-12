using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionStartSystem : MonoBehaviour
{
    [SerializeField]
    public OverWorldMenu overWorldMenu;

    [SerializeField]
    public UIMissionStartMenu missionStartMenu;

    [SerializeField]
    public CharacterSystem characterSystem;

    [SerializeField]
    public CharacterSystem missionStartCharacterSystem;

    public void OpenMenu()
    {
        Debug.Log("Hello");
        missionStartMenu.gameObject.SetActive(true);
        overWorldMenu.ChangeMenu += CloseMenu;

        LoadCharacters();
    }

    public void CloseMenu()
    {
        missionStartMenu.gameObject.SetActive(false);
        overWorldMenu.ChangeMenu -= CloseMenu;
    }

    public void LoadCharacters()
    {
        CharacterMenuSO companyData = characterSystem.companyData;
        for(int i = 0; i < companyData.heroes.Count; i++)
        {
            missionStartMenu.AddHero(companyData.heroes[i]);
        }
    }
}
