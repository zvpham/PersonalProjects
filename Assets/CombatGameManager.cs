using CodeMonkey.Utils;
using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class CombatGameManager : MonoBehaviour, IDataPersistence
{
    [SerializeField]
    public ResourceManager resourceManager;
    [SerializeField]
    public DataPersistenceManager dataPersistenceManager;

    [SerializeField]
    public CombatMapGenerator combatMapGenerator;
    public SpriteManager spriteManager;
    public DijkstraMap map;
    public GridHex<GridPosition> grid;
    public GridHex<PassiveGridObject> passiveGrid; // This is for when actions play

    public List<PassiveEffectArea> passiveAreas = new List<PassiveEffectArea>(); // This makes targeting easier
    public List<Unit> units = new List<Unit>();
    public List<IInititiave> allinitiativeGroups = new List<IInititiave>();
    public List<IInititiave> initiativeOrder = new List<IInititiave>();
    public List<ActionData> actionsInQueue = new List<ActionData>();
    
    public bool playerTurnActive = false;
    public PlayerTurn playerTurn;
    public AITurn enemyTurn;
    public Move move;

    public bool startOfCombat = true;
    public bool playingAnimation = false;
    public bool testing = false;
    public bool addedAction = false;

    public int mapSize = 32;
    public float terrainHeightDifference = 0.16125f;
    public int defaultElevation = 3;
    public int amountOfElevations = 7;

    public Vector3 defaultGridAdjustment = Vector3.zero;
    public float cellSize;
    public UnityAction<ActionData> OnActivateAction;

    // Start is called before the first frame update
    void Awake()
    {
        if (!testing)
        {
            OnStartGame(mapSize, mapSize);
            dataPersistenceManager = DataPersistenceManager.Instance;
        }
    }

    public void Start()
    {
        if (!testing)
        {
            dataPersistenceManager.LoadGame();
            enabled = false;
            startOfCombat = true;
        }
        else
        {
            enabled = false;
        }
    }

    public void StartCombat()
    {
        if (!startOfCombat)
        {
            return;
        }
        enabled = true;
        startOfCombat = false;
    }

    public void LoadData(WorldMapData mapData = null)
    {
        Debug.Log("Load Game");
        // Load Units
        List<UnitSuperClass> frontLineUnits = new List<UnitSuperClass>();
        for (int i = 0; i < mapData.frontLineData.Count; i++)
        {
            battleLineData frontLineUnit = mapData.frontLineData[i];
            frontLineUnits.Add(LoadPlayerUnitSuperGroupData(frontLineUnit));
        }

        List<UnitSuperClass> backLineUnits = new List<UnitSuperClass>();
        for (int i = 0; i < mapData.backLineData.Count; i++)
        {
            battleLineData backLineUnit = mapData.backLineData[i];
            backLineUnits.Add(LoadPlayerUnitSuperGroupData(backLineUnit));
            
        }

        List<UnitSuperClass> enemyUnits1 = new List<UnitSuperClass>();
        for (int i = 0; i < mapData.enemyUnits1.Count; i++)
        {
            battleLineData enemyUnit = mapData.enemyUnits1[i];
            if (enemyUnit.unitGroupData.mercenaryIndex != -1)
            {
                UnitGroup newUnitGroup = Instantiate(resourceManager.mercenaries[enemyUnit.unitGroupData.mercenaryIndex]);
                newUnitGroup.gameManager = this;
                newUnitGroup.team = Team.Team2;
                for (int j = 0; j < newUnitGroup.transform.childCount; j++)
                {
                    Unit childUnit = newUnitGroup.transform.GetChild(j).GetComponent<Unit>();
                    childUnit.team = Team.Team2;
                    childUnit.gameManager = this;
                    childUnit.group = newUnitGroup;
                    newUnitGroup.units.Add(childUnit);
                }
            enemyUnits1.Add(newUnitGroup);
            }
            else
            {
                Unit newHero = Instantiate(resourceManager.heroes[enemyUnit.unitData.heroIndex]);   
                newHero.gameManager = this;
                newHero.team = Team.Team2;
                newHero.transform.SetParent(gameObject.transform);
                enemyUnits1.Add(newHero);
            }
        }

        List<UnitSuperClass> enemyUnits2 = new List<UnitSuperClass>();
        for (int i = 0; i < mapData.enemyUnits2.Count; i++)
        {
            battleLineData enemyUnit = mapData.enemyUnits2[i];
            if (enemyUnit.unitGroupData.mercenaryIndex != -1)
            {
                UnitGroup newUnitGroup = Instantiate(resourceManager.mercenaries[enemyUnit.unitGroupData.mercenaryIndex]);
                newUnitGroup.gameManager = this;
                newUnitGroup.team = Team.Team3;
                for (int j = 0; j < newUnitGroup.transform.childCount; j++)
                {
                    Unit childUnit = newUnitGroup.transform.GetChild(j).GetComponent<Unit>();
                    childUnit.team = Team.Team3;
                    childUnit.gameManager = this;
                    childUnit.group = newUnitGroup;
                    newUnitGroup.units.Add(childUnit);
                }
                enemyUnits2.Add(newUnitGroup);
            }
            else
            {
                Unit newHero = Instantiate(resourceManager.heroes[enemyUnit.unitData.heroIndex]);
                newHero.gameManager = this;
                newHero.team = Team.Team3;
                newHero.transform.SetParent(gameObject.transform);
                enemyUnits2.Add(newHero);
            }
        }

        List<UnitSuperClass> allyUnits = new List<UnitSuperClass>();
        for (int i = 0; i < mapData.allyUnits.Count; i++)
        {
            battleLineData enemyUnit = mapData.allyUnits[i];
            if (enemyUnit.unitGroupData.mercenaryIndex != -1)
            {
                UnitGroup newUnitGroup = Instantiate(resourceManager.mercenaries[enemyUnit.unitGroupData.mercenaryIndex]);
                newUnitGroup.gameManager = this;
                newUnitGroup.team = Team.Team4;
                for (int j = 0; j < newUnitGroup.transform.childCount; j++)
                {
                    Unit childUnit = newUnitGroup.transform.GetChild(j).GetComponent<Unit>();
                    childUnit.team = Team.Team4;
                    childUnit.gameManager = this;
                    childUnit.group = newUnitGroup;
                    newUnitGroup.units.Add(childUnit);
                }
                allyUnits.Add(newUnitGroup);
            }
            else
            {
                Unit newHero = Instantiate(resourceManager.heroes[enemyUnit.unitData.heroIndex]);
                newHero.gameManager = this;
                newHero.team = Team.Team4;
                newHero.transform.SetParent(gameObject.transform);
                allyUnits.Add(newHero);
            }
        }

        if (mapData.inCombat == false)
        {
            //Puts enemy units into a frontLine and BackLine, 0  - FrontLine, 1 - BackLine
            List<List<UnitSuperClass>> enemyUnits1BattleLines =  enemyTurn.LoadEnemyPositions(mapData.missionUnitPlacementName, 
                enemyUnits1);

            // Load Mission Data
            combatMapGenerator.InitializeCombatMapGenerator(grid, mapSize, mapData.missionType, mapData.missionUnitPlacementName, 
                resourceManager.mapTerrains[mapData.mapTerrainData],
                mapData.missionProviderFaction, mapData.missionTargetFaction, mapData.missionAdditionalFaction, frontLineUnits, 
                backLineUnits, enemyUnits1BattleLines[0], enemyUnits1BattleLines[1]);
            combatMapGenerator.GenerateTerrain();
            combatMapGenerator.PlaceUnits();
        }
    }

    private UnitSuperClass LoadPlayerUnitSuperGroupData(battleLineData backLineUnit)
    {
        if (backLineUnit.unitGroupData.mercenaryIndex != -1)
        {
            UnitGroup newUnitGroup = Instantiate(resourceManager.mercenaries[backLineUnit.unitGroupData.mercenaryIndex]);
            newUnitGroup.gameManager = this;
            for (int j = 0; j < newUnitGroup.transform.childCount; j++)
            {
                Unit childUnit = newUnitGroup.transform.GetChild(j).GetComponent<Unit>();
                childUnit.team = Team.Player;
                childUnit.gameManager = this;
                childUnit.group = newUnitGroup;
                newUnitGroup.units.Add(childUnit);
            }
            newUnitGroup.team = Team.Player;
            return (newUnitGroup);
        }
        else
        {
            unitLoadoutData hero = backLineUnit.unitData;
            Unit newHero = Instantiate(resourceManager.emptyHero);
            newHero.gameManager = this;
            newHero.team = Team.Player;
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

            if (hero.helmetIndex != -1)
            {
                EquipableItemSO item = resourceManager.allItems[hero.helmetIndex];
                newHero.helmet = item;
            }
            if (hero.armorIndex != -1)
            {
                EquipableItemSO item = resourceManager.allItems[hero.armorIndex];
                newHero.armor = item;
            }
            if (hero.bootsIndex != -1)
            {
                EquipableItemSO item = resourceManager.allItems[hero.bootsIndex];
                newHero.legs = item;
            }
            if (hero.mainHandIndex != -1)
            {
                EquipableItemSO item = resourceManager.allItems[hero.mainHandIndex];
                newHero.mainHand = item;
            }
            if (hero.offHandIndex != -1)
            {
                EquipableItemSO item = resourceManager.allItems[hero.offHandIndex];
                newHero.offHand = item;
            }
            if (hero.item1Index != -1)
            {
                EquipableItemSO item = resourceManager.allItems[hero.item1Index];
                newHero.Item1 = item;
            }
            if (hero.item2Index != -1)
            {
                EquipableItemSO item = resourceManager.allItems[hero.item2Index];
                newHero.Item2 = item;
            }
            if (hero.item3Index != -1)
            {
                EquipableItemSO item = resourceManager.allItems[hero.item3Index];
                newHero.Item3 = item;
            }
            if (hero.item4Index != -1)
            {
                EquipableItemSO item = resourceManager.allItems[hero.item4Index];
                newHero.Item4 = item;
            }
            if (hero.backUpMainHandIndex != -1)
            {
                EquipableItemSO item = resourceManager.allItems[hero.backUpMainHandIndex];
                newHero.backUpMainHand = item;
            }
            if (hero.backUpOffHandIndex != -1)
            {
                EquipableItemSO item = resourceManager.allItems[hero.backUpOffHandIndex];
                newHero.backUpOffHand = item;
            }
            return (newHero);
        }
    }

    public void SaveData(WorldMapData mapData = null)
    {
        throw new System.NotImplementedException();
    }

    public void Update()
    {
        if (!playingAnimation)
        {
            NextTurn();
        }
    }

    public void NextTurn()
    {
        enabled = false;
        // Round End
        if (initiativeOrder.Count == 0)
        {
            setInitiativeOrder();
            ResetUnitActionAmounts();
        }
        initiativeOrder[initiativeOrder.Count - 1].StartTurn();
    }

    public void StartPlayerTurn(Unit unit)
    {
        StartPlayerTurn(new List<Unit>() { unit });
    }

    public void StartPlayerTurn(List<Unit> unitGroup)
    {
        playerTurn.OnTurnStart(unitGroup);
        playerTurnActive = true;
    }

    public void TurnEnd(IInititiave initiativeGroup)
    {
        initiativeOrder.Remove(initiativeGroup);
        enabled = true;
    }

    public void ResetUnitActionAmounts()
    {
        for(int i = 0; i < units.Count; i++)
        {
            units[i].amountMoveUsedDuringRound = 0;
            for(int j = 0; j < units[i].amountActionUsedDuringRound.Count; j++)
            {
                units[i].amountActionUsedDuringRound[j] = 0;
            }
        }
    }

    public void setInitiativeOrder()
    {
        List<int> initiatives = new List<int>();
        initiativeOrder.Clear();
        for (int i = 0; i < allinitiativeGroups.Count; i++)
        {
            initiatives.Add(allinitiativeGroups[i].CalculateInititive());
            initiativeOrder.Add(allinitiativeGroups[i]);
        }

        Quicksort(initiatives, 0, initiatives.Count - 1);
    }

    public void AddActionToQueue(ActionData newAction, bool addToStart)
    {
        if (addToStart)
        {
            actionsInQueue.Insert(0, newAction);
        }
        else
        {
            actionsInQueue.Add(newAction);
        }
    }

    public void PlayActions()
    {
        Unit initialMovingUnit = actionsInQueue[0].actingUnit;
        while(actionsInQueue.Count != 0)
        {
            OnActivateAction?.Invoke(actionsInQueue[0]);

            if (!addedAction)
            {
                actionsInQueue[0].action.ConfirmAction(actionsInQueue[0]);
                actionsInQueue.RemoveAt(0);
            }
        }

        if(initialMovingUnit != null)
        {
            initialMovingUnit.ActionsFinishedActivating();
        }
        else
        {
            Debug.LogError("try to finish actions but unit died");
        }
    }

    public void SetGridObject(Unit unit, Vector3 unitPosition)
    {
        GridPosition gridPosition = grid.GetGridObject(unitPosition);

        if(unit == null)
        {
            gridPosition.unit =  gridPosition.tempUnit;
            gridPosition.tempUnit = null;
        }
        else
        {
            if(gridPosition.unit != null && gridPosition.unit != unit)
            {
                gridPosition.tempUnit = gridPosition.unit;
            }
            gridPosition.unit = unit;
        }
        grid.SetGridObject(unitPosition, gridPosition);
    }

    public void OnStartGame(int mapWidth, int mapHeight)
    {
        grid = new GridHex<GridPosition>(mapWidth, mapHeight, cellSize, defaultGridAdjustment, (GridHex<GridPosition> g, int x, int y) =>
        new GridPosition(g, x, y, defaultElevation), false);
        map = new DijkstraMap(mapWidth, mapHeight, cellSize, defaultGridAdjustment, false);
        spriteManager.CreateGrid(mapWidth, mapHeight, amountOfElevations, cellSize, defaultGridAdjustment);
    }

    private void Quicksort(List<int> array, int left, int right)
    {
        if (left < right)
        {
            int pivotIndex = Partition(array, left, right);
            Quicksort(array, left, pivotIndex - 1);
            Quicksort(array, pivotIndex + 1, right);
        }
    }

    private int Partition(List<int> array, int left, int right)
    {
        int pivot = array[right];
        int i = left - 1;

        for (int j = left; j < right; j++)
        {
            if (array[j] < pivot)
            {
                i++;
                Swap(array, i, j);
            }
        }

        Swap(array, i + 1, right);
        return i + 1;
    }
    private void Swap(List<int> array, int i, int j)
    {
        int temp = array[i];
        array[i] = array[j];
        array[j] = temp;

        IInititiave tempi = initiativeOrder[i];
        initiativeOrder[i] = initiativeOrder[j];
        initiativeOrder[j] = tempi;
    }

    public void CreateBackGround()
    {
        grid = new GridHex<GridPosition>(mapSize, mapSize, cellSize, defaultGridAdjustment, (GridHex<GridPosition> g, int x, int y) =>
new GridPosition(g, x, y, defaultElevation), false);
        spriteManager.allGroundTerrainHolders = new List<TerrainHolder>();
        int amountOfLayers = 4;
        int layerAdjustment = 2;
        for (int i = 0; i < mapSize; i++)
        {
            int startingSortingOrder;
            if(i % 2 == 0)
            {
                startingSortingOrder = mapSize * amountOfLayers * layerAdjustment;
            }
            else
            {
                startingSortingOrder = mapSize * amountOfLayers * layerAdjustment - amountOfLayers;
            }
            for(int j = 0; j < mapSize; j++)
            {
                TerrainHolder newHex = Instantiate(spriteManager.newGroundprefab);
                newHex.elevation = defaultElevation;
                newHex.x = i; 
                newHex.y = j;
                newHex.transform.position = grid.GetWorldPosition(i, j);
                newHex.sprite.sortingOrder = startingSortingOrder - (amountOfLayers * layerAdjustment * j);
                spriteManager.allGroundTerrainHolders.Add(newHex);
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CombatGameManager))]
public class combatGameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CombatGameManager me = (CombatGameManager)target;
        if (GUILayout.Button("CreateBackground"))
        {
            me.CreateBackGround();
        }
        DrawDefaultInspector();
    }
}
#endif
