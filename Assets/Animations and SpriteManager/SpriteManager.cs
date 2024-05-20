using CodeMonkey.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Events;
using UnityEngine.WSA;

public class SpriteManager : MonoBehaviour
{
    [SerializeField] private LineController oneActionLineController;
    [SerializeField] private LineController twoActionLineController;
    [SerializeField] private CombatGameManager combatGameManager;
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private InputManager inputManager;

    public MovementTargeting movementTargeting;
    public MeleeTargeting meleeTargeting;

    public ActionConfirmationMenu actionConfirmationMenu;
    public GameObject spriteHolderPrefab;

    // 0 - Ground Level, 1 GroundLevelHighlights
    [SerializeField] private List<Tilemap> tileMaps;

    [SerializeField] private TargetingSystem activeTargetingSystems;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            movementTargeting.enabled = true;
        }
    }

    public void ActivateActionConfirmationMenu(UnityAction confirmAction, UnityAction cancelAction)
    {
        actionConfirmationMenu.ActivateMenu(confirmAction, cancelAction);
    }

    public void ConfirmAction()
    {
        actionConfirmationMenu.ActivateConfirmAction();
    }


    public void CancelAction()
    {
        actionConfirmationMenu.ActivateCancelAction();
    }

    public GameObject CreateTempSpriteHolder(Vector2Int hexPosition, Sprite sprite)
    {
        Vector3 worldPosition =  combatGameManager.grid.GetWorldPosition(hexPosition.x, hexPosition.y);
        GameObject tempSprite =  Instantiate(spriteHolderPrefab, worldPosition, new Quaternion(0, 0, 0, 1f));
        tempSprite.GetComponent<SpriteRenderer>().sprite = sprite;
        return tempSprite;
    }

    public void DrawLine(List<Vector3> oneActionLinePath, List<Vector3> twoActionLinePath)
    { 
        if(oneActionLinePath != null && oneActionLinePath.Count != 0)
        {
            oneActionLineController.SetLine(oneActionLinePath);
        }

        if (twoActionLinePath != null && twoActionLinePath.Count != 0)
        {
            twoActionLineController.SetLine(twoActionLinePath);
        }
    }

    public void ClearLines()
    {
        List<Vector3> emptyList = new List<Vector3>();
        oneActionLineController.SetLine(emptyList);
        twoActionLineController.SetLine(emptyList);
    }

    public void ActivateMovementTargeting(Unit movingUnit, Vector2 startPosition, bool targetFriendly, int actionPointsLeft)
    {
        inputManager.TargetPositionMoved += movementTargeting.SelectNewPosition;
        inputManager.FoundPosition += movementTargeting.EndMovementTargeting;
        activeTargetingSystems = movementTargeting;
        movementTargeting.SetParameters(startPosition, movingUnit, targetFriendly, actionPointsLeft);
        movementTargeting.SelectNewPosition(UtilsClass.GetMouseWorldPosition());
    }

    public void ActivateMeleeAttackTargeting(Unit movingUnit, Vector2 startPosition, bool targetFriendly, int actionPointsLeft, int meleeRange)
    {
        inputManager.TargetPositionMoved += meleeTargeting.SelectNewPosition;
        inputManager.FoundPosition += meleeTargeting.EndMovementTargeting;
        activeTargetingSystems = meleeTargeting;
        meleeTargeting.SetParameters(startPosition, movingUnit, targetFriendly, actionPointsLeft, meleeRange);
        meleeTargeting.SelectNewPosition(UtilsClass.GetMouseWorldPosition());
    }

    public void DeactiveTargetingSystem()
    {
        if(activeTargetingSystems != null)
        {
            inputManager.TargetPositionMoved = null;
            inputManager.FoundPosition = null;
            activeTargetingSystems.DeactivateTargetingSystem();
        }
    }

    public void ActivateLineOfSightTargeting(TargetingSystem currentTargetingSystem, int range, UnityAction confirmAction, UnityAction cancelAction)
    {

    }

    public void ChangeTile(Vector2Int position, int tilemapIndex, int hexIndex)
    {
        Vector3Int currentNodePosition = new Vector3Int(position.y, position.x);
       tileMaps[tilemapIndex].SetTile(currentNodePosition, resourceManager.BaseTile[hexIndex]);
    }
}
