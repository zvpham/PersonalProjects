using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Passives/OpportunityAttack")]
public class OpportunityAttack : Passive
{
    public override void AddPassive(Unit unit)
    {
        Debug.Log("add Passive: "  +  unit);
        base.AddPassive(unit);
        unit.OnPositionChanged += OnPositionChanged;
        if(unit.MeleeAttack == null)
        {
            for(int i = 0; i < unit.actions.Count; i++)
            {
                Action action = unit.actions[i];
                if (action.GetType() == typeof(MeleeAttack))
                {
                    unit.MeleeAttack = (MeleeAttack)action;
                    break;
                }
            }
        }
    }

    public override void OnSelectedAction(Action action, TargetingSystem targetingSystem)
    {

    }

    public override void RemovePassive(Unit unit)
    {   
        Debug.Log("remove Passive");
        unit.OnPositionChanged -= OnPositionChanged;
    }

    public void OnPositionChanged(Vector2Int oldPosition, Vector2Int newPosition, Unit movingUnit)
    {
        int passiveIndex = movingUnit.passives.IndexOf(this);
        if (passiveIndex != -1)
        {
            List<Vector2Int> passiveLocations = movingUnit.activePassiveLocations[passiveIndex];
            for (int i = 0; i < passiveLocations.Count; i++)
            {
                List<PassiveObjects> passivesOnLocation = movingUnit.gameManager.passiveGrid.GetGridObject(passiveLocations[i].x, passiveLocations[i].y).passiveObjects;
                for (int j = 0; j < passivesOnLocation.Count; j++)
                {
                    if (passivesOnLocation[j].passive == this && passivesOnLocation[j].originUnit == movingUnit)
                    {
                        passivesOnLocation[j] = null;
                        passivesOnLocation.RemoveAt(j);
                        PassiveGridObject passiveGridObject = movingUnit.gameManager.passiveGrid.GetGridObject(passiveLocations[i].x, passiveLocations[i].y);
                        passiveGridObject.passiveObjects = passivesOnLocation;
                        movingUnit.gameManager.passiveGrid.SetGridObject(passiveLocations[i].x, passiveLocations[i].y, passiveGridObject);
                        break;
                    }
                }
            }
        }

        MeleeAttack meleeAttack = movingUnit.MeleeAttack;
        List<Vector2Int> mapNodes;
        PassiveObjects newPassiveObject = new PassiveObjects();
        newPassiveObject.originUnit = movingUnit;
        newPassiveObject.passive = this;
        movingUnit.gameManager.map.ResetMap(true);
        movingUnit.gameManager.map.SetGoals(new List<Vector2Int> { newPosition }, movingUnit.gameManager, movingUnit.moveModifier);
        movingUnit.activePassiveLocations[passiveIndex].Clear();
        for (int j = 1; j <= meleeAttack.range; j++)
        {
            mapNodes = movingUnit.gameManager.map.getGrid().GetGridPositionsInRing(newPosition.x, newPosition.y, j);
            for (int k = 0; k < mapNodes.Count; k++)
            {
                Debug.Log("add zone of Control");
                Vector2Int currentNodePosition = new Vector2Int(mapNodes[k].x, mapNodes[k].y);
                PassiveGridObject passiveGridObject = movingUnit.gameManager.passiveGrid.GetGridObject(currentNodePosition.x, currentNodePosition.y);
                passiveGridObject.passiveObjects.Add(newPassiveObject);
                movingUnit.gameManager.passiveGrid.SetGridObject(currentNodePosition.x, currentNodePosition.y, passiveGridObject);
                movingUnit.activePassiveLocations[passiveIndex].Add(currentNodePosition);
            }
        }
    }
}
