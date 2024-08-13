using Inventory.Model;
using JetBrains.Annotations;
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
    public MissionStartSystem missionStartSystem;
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
                newHero = Instantiate(resourceManager.emptyHero);
                newHero.transform.SetParent(gameObject.transform);
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

        for(int i = 0; i < mapData.playerMercenaries.Count; i++)
        {
            UnitGroup newMercenary = null;
            if (!mapData.playerMercenaries[i].isEmpty)
            {
                newMercenary = Instantiate(resourceManager.mercenaries[i]);
                characterSystem.companyData.SetMercenary(i, newMercenary);
            }
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
        mapData.playerItemIndexes = itemIndexes;
        mapData.playerItemQuantities = itemQuantities;

        // Saving CharacterData
        List<Unit> playerHeroes = characterSystem.companyData.heroes;
        List<unitLoadoutData> heroes =  new List<unitLoadoutData>();
        for(int i = 0; i < playerHeroes.Count; i++)
        {
            unitLoadoutData tempHero = new unitLoadoutData();
            if (playerHeroes[i] != null)
            {
                tempHero.jobIndex = resourceManager.job.IndexOf(playerHeroes[i].unitClass);
                tempHero.skillTree1Branch1Unlocks = playerHeroes[i].skillTreeOneBranchOne;
                tempHero.skillTree1Branch2Unlocks = playerHeroes[i].skillTreeOneBranchTwo;
                tempHero.skillTree2Branch1Unlocks = playerHeroes[i].skillTreeTwoBranchOne;
                tempHero.skillTree2Branch2Unlocks = playerHeroes[i].skillTreeTwoBranchTwo;

                tempHero.helmetIndex = resourceManager.allItems.IndexOf(playerHeroes[i].helmet);
                tempHero.armorIndex = resourceManager.allItems.IndexOf(playerHeroes[i].armor);
                tempHero.bootsIndex = resourceManager.allItems.IndexOf(playerHeroes[i].legs);
                tempHero.mainHandIndex = resourceManager.allItems.IndexOf(playerHeroes[i].mainHand);
                tempHero.offHandIndex = resourceManager.allItems.IndexOf(playerHeroes[i].offHand);
                tempHero.item1Index = resourceManager.allItems.IndexOf(playerHeroes[i].Item1);
                tempHero.item2Index = resourceManager.allItems.IndexOf(playerHeroes[i].Item2);
                tempHero.item3Index = resourceManager.allItems.IndexOf(playerHeroes[i].Item3);
                tempHero.item4Index = resourceManager.allItems.IndexOf(playerHeroes[i].Item4);
                tempHero.backUpMainHandIndex = resourceManager.allItems.IndexOf(playerHeroes[i].backUpMainHand);
                tempHero.backUpOffHandIndex = resourceManager.allItems.IndexOf(playerHeroes[i].backUpOffHand);
            }
            heroes.Add(tempHero);
        }
        mapData.playerHeroes = heroes;

        List<UnitGroup> unitGroups = characterSystem.companyData.mercenaries;
        List<mercenaryData> mercenaries =  new List<mercenaryData>();
        for(int i = 0; i < mercenaries.Count; i++)
        {
            mercenaryData tempMercenary = new mercenaryData();
            if (unitGroups[i] != null) 
            {
                tempMercenary.mercenaryIndex = unitGroups[i].mercenaryIndex;
            }
            mercenaries.Add(tempMercenary);
        }
        mapData.playerMercenaries = mercenaries;

        //Saving Player BattleLine Data
        List<UnitSuperClass> frontLineUnits = missionStartSystem.frontLineSystem.companyData.units;
        List<battleLineData> frontLineData = new List<battleLineData>();
        for(int i = 0; i < frontLineUnits.Count; i++)
        {
            if (frontLineUnits[i] == null)
            {
                continue;
            }
            battleLineData tempBattleLineData = new battleLineData();
            UnitSuperClass unitSuperClass = frontLineUnits[i];
            if (unitSuperClass.GetType() == typeof(UnitGroup))
            {
                mercenaryData mercenaryData =  new mercenaryData();
                UnitGroup mercenaryGroup = (UnitGroup)unitSuperClass;
                mercenaryData.mercenaryIndex = mercenaryGroup.mercenaryIndex;
                tempBattleLineData.unitGroupData = mercenaryData;
            }
            else
            {
                unitLoadoutData unitLoadoutData = new unitLoadoutData();
                Unit tempHero = (Unit)unitSuperClass;
                unitLoadoutData.jobIndex = resourceManager.job.IndexOf(tempHero.unitClass);
                unitLoadoutData.skillTree1Branch1Unlocks = tempHero.skillTreeOneBranchOne;
                unitLoadoutData.skillTree1Branch2Unlocks = tempHero.skillTreeOneBranchTwo;
                unitLoadoutData.skillTree2Branch1Unlocks = tempHero.skillTreeTwoBranchOne;
                unitLoadoutData.skillTree2Branch2Unlocks = tempHero.skillTreeTwoBranchTwo;

                unitLoadoutData.helmetIndex = resourceManager.allItems.IndexOf(tempHero.helmet);
                unitLoadoutData.armorIndex = resourceManager.allItems.IndexOf(tempHero.armor);
                unitLoadoutData.bootsIndex = resourceManager.allItems.IndexOf(tempHero.legs);
                unitLoadoutData.mainHandIndex = resourceManager.allItems.IndexOf(tempHero.mainHand);
                unitLoadoutData.offHandIndex = resourceManager.allItems.IndexOf(tempHero.offHand);
                unitLoadoutData.item1Index = resourceManager.allItems.IndexOf(tempHero.Item1);
                unitLoadoutData.item2Index = resourceManager.allItems.IndexOf(tempHero.Item2);
                unitLoadoutData.item3Index = resourceManager.allItems.IndexOf(tempHero.Item3);
                unitLoadoutData.item4Index = resourceManager.allItems.IndexOf(tempHero.Item4);
                unitLoadoutData.backUpMainHandIndex = resourceManager.allItems.IndexOf(tempHero.backUpMainHand);
                unitLoadoutData.backUpOffHandIndex = resourceManager.allItems.IndexOf(tempHero.backUpOffHand);
                tempBattleLineData.unitData = unitLoadoutData;
                tempBattleLineData.unitGroupData.mercenaryIndex = -1;
            }
            frontLineData.Add(tempBattleLineData);
        }

        mapData.frontLineData = frontLineData;

        List<UnitSuperClass> backLineUnits = missionStartSystem.backLineSystem.companyData.units;
        List<battleLineData> backLineData = new List<battleLineData>();
        for (int i = 0; i < backLineUnits.Count; i++)
        {
            if (backLineUnits[i] == null)
            {
                continue;
            }
            battleLineData tempBattleLineData = new battleLineData();
            UnitSuperClass unitSuperClass = backLineUnits[i];
            if (unitSuperClass.GetType() == typeof(UnitGroup))
            {
                mercenaryData mercenaryData = new mercenaryData();
                UnitGroup mercenaryGroup = (UnitGroup)unitSuperClass;
                mercenaryData.mercenaryIndex = mercenaryGroup.mercenaryIndex;
                tempBattleLineData.unitGroupData = mercenaryData;
            }
            else
            {
                unitLoadoutData unitLoadoutData = new unitLoadoutData();
                Unit tempHero = (Unit)unitSuperClass;
                unitLoadoutData.jobIndex = resourceManager.job.IndexOf(tempHero.unitClass);
                unitLoadoutData.skillTree1Branch1Unlocks = tempHero.skillTreeOneBranchOne;
                unitLoadoutData.skillTree1Branch2Unlocks = tempHero.skillTreeOneBranchTwo;
                unitLoadoutData.skillTree2Branch1Unlocks = tempHero.skillTreeTwoBranchOne;
                unitLoadoutData.skillTree2Branch2Unlocks = tempHero.skillTreeTwoBranchTwo;

                unitLoadoutData.helmetIndex = resourceManager.allItems.IndexOf(tempHero.helmet);
                unitLoadoutData.armorIndex = resourceManager.allItems.IndexOf(tempHero.armor);
                unitLoadoutData.bootsIndex = resourceManager.allItems.IndexOf(tempHero.legs);
                unitLoadoutData.mainHandIndex = resourceManager.allItems.IndexOf(tempHero.mainHand);
                unitLoadoutData.offHandIndex = resourceManager.allItems.IndexOf(tempHero.offHand);
                unitLoadoutData.item1Index = resourceManager.allItems.IndexOf(tempHero.Item1);
                unitLoadoutData.item2Index = resourceManager.allItems.IndexOf(tempHero.Item2);
                unitLoadoutData.item3Index = resourceManager.allItems.IndexOf(tempHero.Item3);
                unitLoadoutData.item4Index = resourceManager.allItems.IndexOf(tempHero.Item4);
                unitLoadoutData.backUpMainHandIndex = resourceManager.allItems.IndexOf(tempHero.backUpMainHand);
                unitLoadoutData.backUpOffHandIndex = resourceManager.allItems.IndexOf(tempHero.backUpOffHand);
                tempBattleLineData.unitData = unitLoadoutData;
                tempBattleLineData.unitGroupData.mercenaryIndex = -1;
            }
            backLineData.Add(tempBattleLineData);
        }
        mapData.backLineData = backLineData;

        //Saving Mission data
        Mission currentMission = missionSelectSystem.currentMission;
        mapData.missionType = currentMission.missionType;
        mapData.missionUnitPlacementName = currentMission.missionUnitFormation;
        mapData.missionProviderFaction = currentMission.missionProviderFaction.factionName;
        mapData.missionTargetFaction = currentMission.missionTargetFaction.factionName;
        if(currentMission.missionAdditionalFaction != null)
        {
            mapData.missionAdditionalFaction = currentMission.missionAdditionalFaction.factionName;
        }

        List<UnitSuperClass> missionEnemyUnits1 = currentMission.enemyUnits;
        List<battleLineData> enemyUnits1 = new List<battleLineData>();
        for (int i = 0; i < missionEnemyUnits1.Count; i++)
        {
            if (missionEnemyUnits1[i] == null)
            {
                continue;
            }
            battleLineData tempBattleLineData = new battleLineData();
            UnitSuperClass unitSuperClass = missionEnemyUnits1[i];
            if (unitSuperClass.GetType() == typeof(UnitGroup))
            {
                mercenaryData mercenaryData = new mercenaryData();
                UnitGroup mercenaryGroup = (UnitGroup)unitSuperClass;
                mercenaryData.mercenaryIndex = mercenaryGroup.mercenaryIndex;
                tempBattleLineData.unitGroupData = mercenaryData;
            }
            else
            {
                unitLoadoutData unitLoadoutData = new unitLoadoutData();
                Unit tempHero = (Unit)unitSuperClass;
                unitLoadoutData.heroIndex = tempHero.heroIndex;
                unitLoadoutData.jobIndex = resourceManager.job.IndexOf(tempHero.unitClass);
                unitLoadoutData.skillTree1Branch1Unlocks = tempHero.skillTreeOneBranchOne;
                unitLoadoutData.skillTree1Branch2Unlocks = tempHero.skillTreeOneBranchTwo;
                unitLoadoutData.skillTree2Branch1Unlocks = tempHero.skillTreeTwoBranchOne;
                unitLoadoutData.skillTree2Branch2Unlocks = tempHero.skillTreeTwoBranchTwo;

                unitLoadoutData.helmetIndex = resourceManager.allItems.IndexOf(tempHero.helmet);
                unitLoadoutData.armorIndex = resourceManager.allItems.IndexOf(tempHero.armor);
                unitLoadoutData.bootsIndex = resourceManager.allItems.IndexOf(tempHero.legs);
                unitLoadoutData.mainHandIndex = resourceManager.allItems.IndexOf(tempHero.mainHand);
                unitLoadoutData.offHandIndex = resourceManager.allItems.IndexOf(tempHero.offHand);
                unitLoadoutData.item1Index = resourceManager.allItems.IndexOf(tempHero.Item1);
                unitLoadoutData.item2Index = resourceManager.allItems.IndexOf(tempHero.Item2);
                unitLoadoutData.item3Index = resourceManager.allItems.IndexOf(tempHero.Item3);
                unitLoadoutData.item4Index = resourceManager.allItems.IndexOf(tempHero.Item4);
                unitLoadoutData.backUpMainHandIndex = resourceManager.allItems.IndexOf(tempHero.backUpMainHand);
                unitLoadoutData.backUpOffHandIndex = resourceManager.allItems.IndexOf(tempHero.backUpOffHand);
                tempBattleLineData.unitData = unitLoadoutData;
                tempBattleLineData.unitGroupData.mercenaryIndex = -1;
            }
            enemyUnits1.Add(tempBattleLineData);
        }
        mapData.enemyUnits1 = enemyUnits1;

        //Saving Mission data
        List<UnitSuperClass> missionEnemyUnits2 = currentMission.enemyUnits2;
        List<battleLineData> enemyUnits2 = new List<battleLineData>();
        for (int i = 0; i < missionEnemyUnits2.Count; i++)
        {
            if (missionEnemyUnits2[i] == null)
            {
                continue;
            }
            battleLineData tempBattleLineData = new battleLineData();
            UnitSuperClass unitSuperClass = missionEnemyUnits2[i];
            if (unitSuperClass.GetType() == typeof(UnitGroup))
            {
                mercenaryData mercenaryData = new mercenaryData();
                UnitGroup mercenaryGroup = (UnitGroup)unitSuperClass;
                mercenaryData.mercenaryIndex = mercenaryGroup.mercenaryIndex;
                tempBattleLineData.unitGroupData = mercenaryData;
            }
            else
            {
                unitLoadoutData unitLoadoutData = new unitLoadoutData();
                Unit tempHero = (Unit)unitSuperClass;
                unitLoadoutData.heroIndex = tempHero.heroIndex;
                unitLoadoutData.jobIndex = resourceManager.job.IndexOf(tempHero.unitClass);
                unitLoadoutData.skillTree1Branch1Unlocks = tempHero.skillTreeOneBranchOne;
                unitLoadoutData.skillTree1Branch2Unlocks = tempHero.skillTreeOneBranchTwo;
                unitLoadoutData.skillTree2Branch1Unlocks = tempHero.skillTreeTwoBranchOne;
                unitLoadoutData.skillTree2Branch2Unlocks = tempHero.skillTreeTwoBranchTwo;

                unitLoadoutData.helmetIndex = resourceManager.allItems.IndexOf(tempHero.helmet);
                unitLoadoutData.armorIndex = resourceManager.allItems.IndexOf(tempHero.armor);
                unitLoadoutData.bootsIndex = resourceManager.allItems.IndexOf(tempHero.legs);
                unitLoadoutData.mainHandIndex = resourceManager.allItems.IndexOf(tempHero.mainHand);
                unitLoadoutData.offHandIndex = resourceManager.allItems.IndexOf(tempHero.offHand);
                unitLoadoutData.item1Index = resourceManager.allItems.IndexOf(tempHero.Item1);
                unitLoadoutData.item2Index = resourceManager.allItems.IndexOf(tempHero.Item2);
                unitLoadoutData.item3Index = resourceManager.allItems.IndexOf(tempHero.Item3);
                unitLoadoutData.item4Index = resourceManager.allItems.IndexOf(tempHero.Item4);
                unitLoadoutData.backUpMainHandIndex = resourceManager.allItems.IndexOf(tempHero.backUpMainHand);
                unitLoadoutData.backUpOffHandIndex = resourceManager.allItems.IndexOf(tempHero.backUpOffHand);
                tempBattleLineData.unitData = unitLoadoutData;
                tempBattleLineData.unitGroupData.mercenaryIndex = -1;
            }
            enemyUnits2.Add(tempBattleLineData);
        }
        mapData.enemyUnits2 = enemyUnits2;

        //Saving Mission data
        List<UnitSuperClass> missionAllyUnits = currentMission.allyUnits;
        List<battleLineData> allyUnits = new List<battleLineData>();
        for (int i = 0; i < missionAllyUnits.Count; i++)
        {
            if (missionAllyUnits[i] == null)
            {
                continue;
            }
            battleLineData tempBattleLineData = new battleLineData();
            UnitSuperClass unitSuperClass = missionAllyUnits[i];
            if (unitSuperClass.GetType() == typeof(UnitGroup))
            {
                mercenaryData mercenaryData = new mercenaryData();
                UnitGroup mercenaryGroup = (UnitGroup)unitSuperClass;
                mercenaryData.mercenaryIndex = mercenaryGroup.mercenaryIndex;
                tempBattleLineData.unitGroupData = mercenaryData;
            }
            else
            {
                unitLoadoutData unitLoadoutData = new unitLoadoutData();
                Unit tempHero = (Unit)unitSuperClass;
                unitLoadoutData.heroIndex = tempHero.heroIndex;
                unitLoadoutData.jobIndex = resourceManager.job.IndexOf(tempHero.unitClass);
                unitLoadoutData.skillTree1Branch1Unlocks = tempHero.skillTreeOneBranchOne;
                unitLoadoutData.skillTree1Branch2Unlocks = tempHero.skillTreeOneBranchTwo;
                unitLoadoutData.skillTree2Branch1Unlocks = tempHero.skillTreeTwoBranchOne;
                unitLoadoutData.skillTree2Branch2Unlocks = tempHero.skillTreeTwoBranchTwo;

                unitLoadoutData.helmetIndex = resourceManager.allItems.IndexOf(tempHero.helmet);
                unitLoadoutData.armorIndex = resourceManager.allItems.IndexOf(tempHero.armor);
                unitLoadoutData.bootsIndex = resourceManager.allItems.IndexOf(tempHero.legs);
                unitLoadoutData.mainHandIndex = resourceManager.allItems.IndexOf(tempHero.mainHand);
                unitLoadoutData.offHandIndex = resourceManager.allItems.IndexOf(tempHero.offHand);
                unitLoadoutData.item1Index = resourceManager.allItems.IndexOf(tempHero.Item1);
                unitLoadoutData.item2Index = resourceManager.allItems.IndexOf(tempHero.Item2);
                unitLoadoutData.item3Index = resourceManager.allItems.IndexOf(tempHero.Item3);
                unitLoadoutData.item4Index = resourceManager.allItems.IndexOf(tempHero.Item4);
                unitLoadoutData.backUpMainHandIndex = resourceManager.allItems.IndexOf(tempHero.backUpMainHand);
                unitLoadoutData.backUpOffHandIndex = resourceManager.allItems.IndexOf(tempHero.backUpOffHand);
                tempBattleLineData.unitData = unitLoadoutData;
                tempBattleLineData.unitGroupData.mercenaryIndex = -1;
            }
            allyUnits.Add(tempBattleLineData);
        }
        mapData.allyUnits = allyUnits;

    }

    public void OpenCombatScene()
    {
        SceneManager.LoadSceneAsync("Combat");
    }

    public void MakeAutoSave()
    {
        dataPersistenceManager.SaveGame(dataPersistenceManager.autoSaveID);
        dataPersistenceManager.saveID = dataPersistenceManager.autoSaveID;
        dataPersistenceManager.saveNumID = dataPersistenceManager.GetCurrentWorldMapData().saveNumber.ToString();
    }
}
