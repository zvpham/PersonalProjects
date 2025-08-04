using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/ContinueChanneling")]
public class ContinueChanneling : Action
{
    public Status channeling;
    public override void SelectAction(Unit self)
    {
        self.gameManager.spriteManager.ActivateActionConfirmationMenu(
            () =>
            {
                ActionData newData = new ActionData();
                newData.action = this;
                newData.actingUnit = self;
                self.gameManager.AddActionToQueue(newData, false, false);
                self.gameManager.PlayActions();
            },
            () =>
            {

            });
    }

    public override void ConfirmAction(ActionData actionData)
    {
        Unit unit = actionData.actingUnit;
        int channelingIndex = -1;
        for (int i = 0; i < unit.statuses.Count; i++)
        {
            if (unit.statuses[i].status == channeling)
            {
                channelingIndex = i;
                break;
            }
        }
        SpellStatusData spellData = (SpellStatusData) unit.statuses[channelingIndex];
        int spellpointsRequired = spellData.spellPointsRequired;
        float currentSpellPoints = unit.magicPowerPoints;
        int majorActionPoints = unit.currentMajorActionsPoints;
        int minorActionPoints = unit.currentMinorActionsPoints;
        for (int i = 0; i < majorActionPoints; i++)
        {
            currentSpellPoints += unit.powerPointGeneration;
            unit.currentMajorActionsPoints--;
            if(currentSpellPoints > spellpointsRequired)
            {
                FinishCastingSpell(unit, spellData, channelingIndex);
            }
            else if(i == 0)
            {
                for (int j = 0; j < minorActionPoints; j++)
                {
                    unit.currentMinorActionsPoints--;
                    currentSpellPoints +=  unit.powerPointGeneration * 0.5f;
                    if (currentSpellPoints > spellpointsRequired)
                    {
                        FinishCastingSpell(unit, spellData, channelingIndex);
                    }
                }
            }
        }
    }

    public void FinishCastingSpell(Unit unit, SpellStatusData spellData, int channelingIndex)
    {
        unit.statuses[channelingIndex].status.RemoveStatus(unit);
        spellData.spell.SelectAction(unit);
    }

    public override void AIUseAction(AIActionData AiActionData, bool finalAction = false)
    {
        throw new System.NotImplementedException();
    }

    public override int CalculateWeight(AIActionData AiActionData)
    {
        throw new System.NotImplementedException();
    }

    public override bool CheckIfActionIsInRange(AIActionData AiActionData)
    {
        throw new System.NotImplementedException();
    }

    public override void FindOptimalPosition(AIActionData AiActionData)
    {
        throw new System.NotImplementedException();
    }
}
