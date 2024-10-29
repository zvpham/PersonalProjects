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
                movingUnit.passiveEffects[passiveIndex].passiveLocations.Add(surroundingNodePosition);
            }
        }
    }
}
