using CodeMonkey.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTurn : MonoBehaviour
{
    public InputManager inputManager;
    public CombatGameManager gameManager;
    public ActionBar actionBar;

    public Unit currentlySelectedUnit;
    public List<Unit> playerUnits;
    public List<Unit> activeUnits;

    public Action currentlySelectedAction;
    // Start is called before the first frame update

    public void Test()
    {

    }

    public void OnTurnStart(List<Unit> units)
    {
        for(int i = 0; i < units.Count; i++)
        {
            activeUnits.Add(units[i]);
        }
        SelectUnit(activeUnits[0]);
        enabled = true;
    }

    public void SelectAction(Action newAction = null)
    {
        if(currentlySelectedAction != null)
        {
            gameManager.spriteManager.DeactiveTargetingSystem();
        }
        currentlySelectedAction = newAction;
        if (newAction != null )
        {
            inputManager.FoundPosition -= NoActionSelected;
            currentlySelectedAction.SelectAction(currentlySelectedUnit);
        }
        else
        {
            inputManager.FoundPosition += NoActionSelected;
        }
    }

    public void NoActionSelected()
    {
        Vector3 mousePosition = UtilsClass.GetMouseWorldPosition();
        gameManager.grid.GetXY(mousePosition, out int x, out int y);
        Vector2Int mouseHexPosition = new Vector2Int(x, y);
        if (CheckSpaceForFriendlyUnit(mouseHexPosition))
        {
            enabled = false;
            SelectUnit(mouseHexPosition);
        }
    }

    public void SelectUnit(Vector2Int hexPosition)
    {
        SelectUnit(gameManager.grid.GetGridObject(hexPosition.x, hexPosition.y).unit);
    }

    public void SelectUnit(Unit unit)
    {
        if (activeUnits.Contains(unit))
        {
            currentlySelectedUnit = unit;
            SelectAction(currentlySelectedUnit.actions[0]);
        }
        else
        {
            currentlySelectedUnit = unit;
            SelectAction(null);
            enabled = true;
        }
        actionBar.ResetActionBarList();
        for(int i = 0; i < currentlySelectedUnit.actions.Count; i++)
        {
            actionBar.AddAction(currentlySelectedUnit.actions[i].actionSprite, currentlySelectedUnit.actionCooldowns[i], currentlySelectedUnit.actionUses[i]);
        }
    }

    public void SelectNextUnit()
    {
        int index = activeUnits.IndexOf(currentlySelectedUnit);
        if(index + 1 == activeUnits.Count)
        {
            index = 0;
        }
        else
        {
            index += 1;
        }
        SelectUnit(activeUnits[index]);
    }

    public void OnActionButtonPresed(int actionIndex)
    {
        SelectAction(currentlySelectedUnit.actions[actionIndex]);
    }

    public void UnitGroupTurnEnd()
    {
        Unit currentUnit = currentlySelectedUnit;
        SelectNextUnit();
        activeUnits.Remove(currentUnit);

    }

    public void TurnEnd()
    {
        activeUnits.Clear();
        enabled = false;
    }

    public bool CheckSpaceForFriendlyUnit(Vector2Int hexPosition)
    {
        GridPosition gridPosition = gameManager.grid.GetGridObject(hexPosition.x, hexPosition.y);
        if(gridPosition == null)
        {
            return false;
        }
        Unit unit = gridPosition.unit;
        return playerUnits.Contains(unit);
    }
}
