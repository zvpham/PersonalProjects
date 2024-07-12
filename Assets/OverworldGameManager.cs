using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OverworldGameManager : MonoBehaviour, IDataPersistence
{
    public DataPersistenceManager dataPersistenceManager;
    public InventorySystem inventorySystem;
    public CharacterSystem characterSystem;
    public MissionSelectSystem missionSelectSystem;
    public ResourceManager resourceManager;

    public void Start()
    {
        dataPersistenceManager = DataPersistenceManager.Instance;
        if (!dataPersistenceManager.disableDataPersistence)
        {
            dataPersistenceManager.LoadGame();
        }
        else
        {
            inventorySystem.LoadInitialItems();
            characterSystem.LoadInitialUnits();
            missionSelectSystem.LoadInitialMissions();
        }
    }
    public void LoadData(WorldMapData mapData = null)
    {
        if(mapData == null)
        {
            inventorySystem.LoadInitialItems();
            characterSystem.LoadInitialUnits();
            missionSelectSystem.LoadInitialMissions();
            return;
        }

        if (mapData.newGame)
        {
            mapData.newGame = false;
            inventorySystem.LoadInitialItems();
            characterSystem.LoadInitialUnits();
            missionSelectSystem.LoadInitialMissions();
        }

        // Loading Item Data
        for(int i = 0; i < mapData.playerItemIndexes.Count; i++)
        {
            inventorySystem.AddItem(resourceManager.allItems[mapData.playerItemIndexes[i]], mapData.playerItemQuantities[i]);
        }

        // Loading Unit Data
        for(int i  = 0; i < mapData.playerHeroes.Count; i++)
        {
            Unit newHero = null;
            if(!mapData.playerHeroes[i].isEmpty)
            {
                unitLoadoutData hero = mapData.playerHeroes[i];
                newHero = Instantiate(resourceManager.heroes[0]);
                newHero.unitClass = resourceManager.job[hero.jobIndex];

                for (int j = 0; j < hero.skillTree1Branch1Unlocks.Count; j++)
                {
                    if (hero.skillTree1Branch1Unlocks[j])
                    {
                        newHero.unitClass.skillTree1.branch1.BranchSkills[j].UnlockSkill(newHero);
                    }
                }

                for (int j = 0; j < hero.skillTree1Branch2Unlocks.Count; j++)
                {
                    if (hero.skillTree1Branch2Unlocks[j])
                    {
                        newHero.unitClass.skillTree1.branch2.BranchSkills[j].UnlockSkill(newHero);
                    }
                }

                for (int j = 0; j < hero.skillTree2Branch1Unlocks.Count; j++)
                {
                    if (hero.skillTree2Branch1Unlocks[j])
                    {
                        newHero.unitClass.skillTree2.branch1.BranchSkills[j].UnlockSkill(newHero);
                    }
                }

                for (int j = 0; j < hero.skillTree2Branch2Unlocks.Count; j++)
                {
                    if (hero.skillTree2Branch2Unlocks[j])
                    {
                        newHero.unitClass.skillTree2.branch2.BranchSkills[j].UnlockSkill(newHero);
                    }
                }
            }
            characterSystem.SetHero(i, newHero);
        }
    }

    public void SaveData(WorldMapData mapData)
    {
        mapData.newGame = false;

        // Saving Item Data
        List<InventoryItem> items = inventorySystem.inventoryData.inventoryItems;
        List<int> itemIndexes = new List<int>();
        List<int> itemQuantities = new List<int>();
        for (int i = 0; i < items.Count; i++)
        {
            itemIndexes.Add(resourceManager.allItems.IndexOf((EquipableItemSO) items[i].item));
            itemQuantities.Add(items[i].quantity);
        }

        // Saving CharacterData
        List<Unit> playerHeroes = characterSystem.companyData.heroes;
        List<unitLoadoutData> heroes =  new List<unitLoadoutData>();
        for(int i = 0; i < playerHeroes.Count; i++)
        {
            if (playerHeroes[i] == null)
            {
                heroes.Add(new unitLoadoutData());
            }
            else
            {
                unitLoadoutData tempHero =  new unitLoadoutData();
                tempHero.jobIndex = resourceManager.job.IndexOf(playerHeroes[i].unitClass);
                tempHero.skillTree1Branch1Unlocks = playerHeroes[i].skillTreeOneBranchOne;
                tempHero.skillTree1Branch2Unlocks = playerHeroes[i].skillTreeOneBranchTwo;
                tempHero.skillTree2Branch1Unlocks = playerHeroes[i].skillTreeTwoBranchOne;
                tempHero.skillTree2Branch2Unlocks = playerHeroes[i].skillTreeTwoBranchTwo;

                tempHero.helmetIndex = resourceManager.allItems.IndexOf(playerHeroes[i].helmet);
                tempHero.armorIndex = resourceManager.allItems.IndexOf(playerHeroes[i].helmet);
                tempHero.bootsIndex = resourceManager.allItems.IndexOf(playerHeroes[i].legs);
                tempHero.mainHandIndex = resourceManager.allItems.IndexOf(playerHeroes[i].mainHand);
                tempHero.offHandIndex = resourceManager.allItems.IndexOf(playerHeroes[i].offHand);
                tempHero.item1Index = resourceManager.allItems.IndexOf(playerHeroes[i].Item1);
                tempHero.item2Index = resourceManager.allItems.IndexOf(playerHeroes[i].Item2);
                tempHero.item3Index = resourceManager.allItems.IndexOf(playerHeroes[i].Item3);
                tempHero.item4Index = resourceManager.allItems.IndexOf(playerHeroes[i].Item4);
            }
        }
    }

    public void OpenCombatScene()
    {
        //dataPersistenceManager.SaveGame();
        SceneManager.LoadSceneAsync("Combat");
    }

    public void MakeAutoSave()
    {
        dataPersistenceManager.SaveGame(dataPersistenceManager.autoSaveID);
    }
}
