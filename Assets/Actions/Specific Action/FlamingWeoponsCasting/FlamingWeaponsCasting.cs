using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.MPE;
using UnityEngine;
using static SpriteManager;

[CreateAssetMenu(menuName = "Action/FlamingWeaponsCasting")]
public class FlamingWeaponsCasting : Action
{
    public Action spell;
    public int spellPointsRequired;
    public Status channeling;

    public override int CalculateWeight(AIActionData actionData)
    {
        return -1;
    }

    public override int CalculateEnvironmentWeight(AIActionData AiActionData)
    {
        return -1;
    }

    // Should only be called if no enemies in range
    public override void FindOptimalPosition(AIActionData actionData)
    {


        return;
    }

    public override bool CheckIfActionIsInRange(AIActionData actionData)
    {
        return false;
    }

    public override void AIUseAction(AIActionData AiactionData, bool finalAction = false)
    {
       
    }

    public override void SelectAction(Unit self)
    {
        base.SelectAction(self);
        self.gameManager.spriteManager.ActivateActionConfirmationMenu(
            () =>
            {
                /*
                ActionData newData = new ActionData();
                newData.action = this;
                newData.actingUnit = self;
                self.gameManager.AddActionToQueue(newData, false, false);
                self.gameManager.PlayActions();
                */
                SpellActionPreset(self, spellPointsRequired, spell, channeling);
            },
            () =>
            {

            });
    }

    public void FoundTarget(Unit movingUnit, List<Unit> targetUnits, bool foundTarget, int itemIndex)
    {
        Debug.Log("Found Target");
        if (foundTarget)
        {
            movingUnit.gameManager.grid.GetXY(movingUnit.transform.position, out int x, out int y);
            ActionData actionData = new ActionData();
            actionData.action = this;
            actionData.actingUnit = movingUnit;
            actionData.affectedUnits = new List<Unit>(targetUnits) { };
            actionData.originLocation = new Vector2Int(x, y);
            actionData.itemIndex = itemIndex;
            movingUnit.gameManager.AddActionToQueue(actionData, false, false);
            movingUnit.gameManager.PlayActions();
        }
        else
        {
            movingUnit.UseActionPoints(0);
        }
        movingUnit.gameManager.spriteManager.DeactiveTargetingSystem();
    }

    public override void ConfirmAction(ActionData actionData)
    {
        Unit unit = actionData.actingUnit;
        SpellActionPreset(unit, spellPointsRequired, spell, channeling);
    }
}
