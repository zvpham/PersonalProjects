using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
public class CombatUISystem : MonoBehaviour
{
    public CombatUIProfile combatUiProfile;
    public InputManager inputManager;
    public CombatGameManager gameManager;
    public List<CombatAttackUI> attackUIList;
    public int mostRecentAttackUIActivated;

    public void Start()
    {
        inputManager.MouseStayedInPlace += HandleMouseStayedStill;
        inputManager.MouseMoved += HandleMouseMoved;
    }

    public void HandleMouseStayedStill()
    {
        Vector3 mousePosition = UtilsClass.GetMouseWorldPosition();
        gameManager.grid.GetXY(mousePosition, out int x, out int y);
        if(gameManager.grid.GetGridObject(x, y) != null && gameManager.grid.GetGridObject(x,y).unit != null)
        {
            combatUiProfile.SetData(gameManager.grid.GetGridObject(x, y).unit);
        }
    }

    public void HandleMouseMoved()
    {
        combatUiProfile.Deactivate();
    }

    public void OnActivateTargetingSystem()
    {
        inputManager.MouseStayedInPlace -= HandleMouseStayedStill;
        inputManager.MouseMoved -= HandleMouseMoved;
        mostRecentAttackUIActivated = 0;
    }

    public void OnDeactivateTargetingSystem()
    {
        inputManager.MouseStayedInPlace -= HandleMouseStayedStill;
        inputManager.MouseMoved -= HandleMouseMoved;
        inputManager.MouseStayedInPlace += HandleMouseStayedStill;
        inputManager.MouseMoved += HandleMouseMoved;
    }

    public void SetDataAttackUi(Unit targetUnit, List<AttackDataUI> attackDatas, Vector3 newPosition)
    {
        attackUIList[mostRecentAttackUIActivated].SetData(targetUnit, attackDatas, newPosition);
        mostRecentAttackUIActivated += 1;
    }

    public void ResetDataAttackUI()
    {
        for (int i = 0; i < mostRecentAttackUIActivated; i++)
        {
            attackUIList[i].Deactivate();
        }
        mostRecentAttackUIActivated = 0;
    }

}
