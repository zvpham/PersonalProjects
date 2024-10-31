using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Passives/OpportunityAttack")]
public class OpportunityAttack : AreaPassive
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

    public void OnPositionChanged(Vector2Int oldPosition, Vector2Int newPosition, Unit movingUnit, bool finalMove)
    {
        if (!finalMove)
        {
            return;
        }
        int passiveIndex = movingUnit.passives.IndexOf(this);
        for (int i = 0; i < movingUnit.passiveEffects.Count; i++)
        {
            if (movingUnit.passiveEffects[i].passive == this)
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
        List<Vector2Int> mapNodes;
        movingUnit.gameManager.map.ResetMap(true);
        movingUnit.gameManager.map.SetGoals(new List<Vector2Int> { newPosition }, movingUnit.gameManager, movingUnit.moveModifier);
        DijkstraMapNode currentunitNode = movingUnit.gameManager.map.getGrid().GetGridObject(newPosition);
        for (int j = 1; j <= meleeAttack.range; j++)
        {
            mapNodes = movingUnit.gameManager.map.getGrid().GetGridPositionsInRing(newPosition.x, newPosition.y, j);
            for (int k = 0; k < mapNodes.Count; k++)
            {
                Vector2Int surroundingNodePosition = new Vector2Int(mapNodes[k].x, mapNodes[k].y);
                DijkstraMapNode surroundingUnitNode = movingUnit.gameManager.map.getGrid().GetGridObject(surroundingNodePosition);
                if (movingUnit.moveModifier.ValidMeleeAttack(movingUnit.gameManager, currentunitNode, surroundingUnitNode, meleeAttack.range))
                {
                    movingUnit.passiveEffects[passiveIndex].passiveLocations.Add(surroundingNodePosition);
                    PassiveGridObject passiveGridObject = passiveGrid.GetGridObject(surroundingNodePosition);
                    passiveGridObject.passiveObjects.Add(movingUnit.passiveEffects[passiveIndex]);
                    passiveGrid.SetGridObject(surroundingNodePosition, passiveGridObject);
                }
            }
        }
    }

    public override System.Tuple<Passive, Vector2Int> GetTargetingData(List<Vector2Int> path, List<Vector2Int> setPath, List<Vector2Int> passiveArea)
    {
        bool foundPosition = false;
        Vector2Int spriteLocation = new Vector2Int(-1, -1);
        for(int i = 0;  i< setPath.Count; i++)
        {
            if (passiveArea.Contains(setPath[i]))
            {
                if( i < setPath.Count - 1 || path.Count != 0)
                {
                    foundPosition = true;
                    spriteLocation = setPath[i];
                    break;
                }
            }
        }

        if(!foundPosition)
        {
            for (int i = 0; i < path.Count; i++)
            {
                if (passiveArea.Contains(path[i]))
                {
                    if (i < path.Count - 1)
                    {
                        spriteLocation = path[i];
                        break;
                    }
                }
            }
        }

        return new System.Tuple<Passive, Vector2Int>(this, spriteLocation);
    }
}
