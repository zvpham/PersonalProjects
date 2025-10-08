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
        int attackUIIndex = -1; 
        for(int i = 0; i < attackUIList.Count; i++)
        {
            if (!attackUIList[i].gameObject.activeSelf) 
            {
                attackUIIndex = i;
                break;
            }
        }
        attackUIList[attackUIIndex].SetData(targetUnit, attackDatas, newPosition);
        attackUIList[attackUIIndex].readyToReset = true;
        targetUnit.combatAttackUi = attackUIList[attackUIIndex];
        
    }

    public void ResetDataAttackUI()
    {
        for (int i = 0; i < attackUIList.Count; i++)
        {
            if (attackUIList[i].readyToReset && attackUIList[i].gameObject.activeSelf)
            {
                attackUIList[i].unit.combatAttackUi = null;
                attackUIList[i].Deactivate();
            }
        }
    }

}
