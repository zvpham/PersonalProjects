using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Passives/OpportunityAttack")]
public class OpportunityAttack : AreaPassive
{
    public override void AddPassive(Unit unit)
    {
        base.AddPassive(unit);
        unit.OnPositionChanged += OnPositionChanged;
        if(unit.MeleeAttack == null)
        {
            for(int i = 0; i < unit.actions.Count; i++)
            {
                Action action = unit.actions[i].action;
                if (action.GetType() == typeof(MeleeAttack))
                {
                    unit.MeleeAttack = (MeleeAttack)action;
                    break;
                }
            }
        }
    }

    public override void RemovePassive(Unit unit)
    {   
        Debug.Log("remove Passive");
        if(unit == null)
        {
            return;
        }
        unit.OnPositionChanged -= OnPositionChanged;
    }

    public void OnPositionChanged(Vector2Int oldPosition, Vector2Int newPosition, Unit movingUnit, bool finalMove)
    {
        if (!finalMove)
        {
            return;
        }
        int passiveIndex = movingUnit.passives[0].passive.GetPassiveIndex(movingUnit);
        for (int i = 0; i < movingUnit.passiveEffects.Count; i++)
        {
            if (movingUnit.passiveEffects[i].passive.passive == this)
            {
                passiveIndex = i;
                break;
            }
        }
        GridHex<PassiveGridObject> passiveGrid = movingUnit.gameManager.passiveGrid;
        for (int i = 0; i < movingUnit.passiveEffects[passiveIndex].passiveLocations.Count; i++)
        {
            Vector2Int passiveLocation = movingUnit.passiveEffects[passiveIndex].passiveLocations[i];
            PassiveGridObject passiveGridObject = passiveGrid.GetGridObject(passiveLocation);
            passiveGridObject.passiveObjects.Remove(movingUnit.passiveEffects[passiveIndex]);
            passiveGrid.SetGridObject(passiveLocation, passiveGridObject);
        }

        movingUnit.passiveEffects[passiveIndex].passiveLocations.Clear();

        MeleeAttack meleeAttack = movingUnit.MeleeAttack;
        DijkstraMapNode currentunitNode = movingUnit.gameManager.map.getGrid().GetGridObject(newPosition);
        for (int j = 1; j <= meleeAttack.range; j++)
        {
            List<Vector2Int> mapNodes = movingUnit.gameManager.map.getGrid().GetGridPositionsInRing(newPosition.x, newPosition.y, j);
            for (int k = 0; k < mapNodes.Count; k++)
            {
                Vector2Int surroundingNodePosition = new Vector2Int(mapNodes[k].x, mapNodes[k].y);
                DijkstraMapNode surroundingUnitNode = movingUnit.gameManager.map.getGrid().GetGridObject(surroundingNodePosition);
                if (movingUnit.moveModifier.NewValidMeleeAttack(movingUnit.gameManager, currentunitNode, surroundingUnitNode, meleeAttack.range))
                {
                    movingUnit.passiveEffects[passiveIndex].passiveLocations.Add(surroundingNodePosition);
                    PassiveGridObject passiveGridObject = passiveGrid.GetGridObject(surroundingNodePosition);
                    passiveGridObject.passiveObjects.Add(movingUnit.passiveEffects[passiveIndex]);
                    passiveGrid.SetGridObject(surroundingNodePosition, passiveGridObject);
                }
            }
        }
    }

    public override System.Tuple<Passive, Vector2Int> GetTargetingData(Vector2Int orignalPosition, List<Vector2Int> path,
        List<Vector2Int> setPath, List<Vector2Int> passiveArea)
    {
        bool foundPosition = false;
        Vector2Int spriteLocation = new Vector2Int(-1, -1);

        if ((setPath.Count != 0 || path.Count != 0) && passiveArea.Contains(orignalPosition))
        {
            foundPosition = true;
            spriteLocation = orignalPosition;
        }

        for (int i = 0;  i < setPath.Count; i++)
        {
            if (passiveArea.Contains(setPath[i]) && (i < setPath.Count - 1 || path.Count != 0))
            {
                foundPosition = true;
                spriteLocation = setPath[i];
                break;
            }
        }

        if(!foundPosition)
        {
            for (int i = 0; i < path.Count; i++)
            {
                if (passiveArea.Contains(path[i]) && i < path.Count - 1)
                {
                    spriteLocation = path[i];
                    break;
                }
            }
        }

        return new System.Tuple<Passive, Vector2Int>(this, spriteLocation);
    }

    public override void ActivatePassive(Unit unit, ActionData actionData)
    {
        actionData.actionCalculated = true;
        if (actionData.action.actionTypes.Contains(ActionType.Movement) && unit.team != actionData.actingUnit.team
            && actionData.actingUnit.CheckStatuses(null, this))
        {
            int passiveAreaIndex = -1;
            for (int i = 0; i < unit.passiveEffects.Count; i++)
            {
                if (unit.passiveEffects[i].passive.passive = this)
                {
                    passiveAreaIndex = i;
                    break;
                }
            }

            List<Vector2Int> passiveArea = unit.passiveEffects[passiveAreaIndex].passiveLocations;
            List<Vector2Int> path = actionData.path;

            if (passiveArea.Contains(actionData.originLocation) && path.Count != 0)
            {
                unit.gameManager.grid.GetXY(unit.transform.position, out int x, out int y);
                actionData.intReturnData = 0;
                ActionData newActionData = new ActionData();
                newActionData.action = unit.MeleeAttack;
                newActionData.actingUnit = unit;
                newActionData.affectedUnits.Add(actionData.actingUnit);
                newActionData.originLocation = new Vector2Int(x, y);
                unit.gameManager.AddActionToQueue(newActionData, true, false);
            }

            for (int i = 0; i < actionData.path.Count; i++)
            {
                if (passiveArea.Contains(path[i]))
                {
                    if (i < path.Count - 1 &&  i < actionData.intReturnData )
                    {
                        unit.gameManager.grid.GetXY(unit.transform.position, out int x, out int y);
                        actionData.intReturnData = i + 1;
                        ActionData newActionData = new ActionData();
                        newActionData.action = unit.MeleeAttack;
                        newActionData.actingUnit = unit;
                        newActionData.affectedUnits.Add(actionData.actingUnit);
                        newActionData.originLocation = new Vector2Int(x, y);
                        unit.gameManager.AddActionToQueue(newActionData, true, false);
                        break;
                    }
                }
            }

        }
    }

    public override void ModifiyAction(Action action, AttackData attackData)
    {
        return;
    }

    public override void CalculatePredictedActionConsequences(AIActionData AiActionData, Unit actingUnit, Vector2Int orignalPosition, List<Vector2Int> path, List<Vector2Int> passiveArea)
    {
        Unit movingUnit = AiActionData.unit;
        MeleeAttack meleeAttack =  actingUnit.MeleeAttack;
        if (path.Count != 0 && passiveArea.Contains(orignalPosition))
        {
            //Debug.Log("Origin Position Oppurtunty Attack");
            meleeAttack.ExpectedEffectsOfPassivesActivations(AiActionData, actingUnit);
        }

        for (int i = 0; i < path.Count; i++)
        {
            //Debug.Log("Walking Path: " + path[i]);
            if (passiveArea.Contains(path[i]) && i < path.Count - 1)
            {
                //Debug.Log("Oppurtunty Attack: " + path[i]);
                meleeAttack.ExpectedEffectsOfPassivesActivations(AiActionData, actingUnit);
            }
        }

    }
}
