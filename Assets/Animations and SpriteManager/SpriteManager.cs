using CodeMonkey.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Events;
using UnityEngine.WSA;

public class SpriteManager : MonoBehaviour
{
    [SerializeField] private List<LineController> lineControllers;
    [SerializeField] private LineController oneActionLineController;
    [SerializeField] private LineController twoActionLineController;
    [SerializeField] private CombatGameManager combatGameManager;
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private CombatUISystem combatUI;

    public GridHex<SpriteNode> spriteGrid;

    public MovementTargeting movementTargeting;
    public MeleeTargeting meleeTargeting;
    public ConeTargeting coneTargeting;

    public ActionConfirmationMenu actionConfirmationMenu;
    public GameObject spriteHolderPrefab;

    // 0 - Ground Level, 1 GroundLevelHighlights, 2 GrondLevelTargetingHighlights, 3 Ground Level Units
    [SerializeField] private List<Tilemap> tileMaps;

    [SerializeField] private TargetingSystem activeTargetingSystems;

    public List<CustomAnimations> animations = new List<CustomAnimations>();
    public bool playNewAnimation;
    public bool DebugAnimations = false;
    public List<float> timeBetweenAnimations = new List<float>();
    public float currentTime;
    public string debugWord;

    public void CreateGrid(int mapWidth, int mapHeight, float cellSize, Vector3 defaultGridAdjustment)
    {
        spriteGrid = new GridHex<SpriteNode>(mapWidth, mapHeight, cellSize, defaultGridAdjustment, (GridHex<SpriteNode> g, int x, int y) => new SpriteNode(g, x, y), false);

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            movementTargeting.enabled = true;
        }

        currentTime += Time.deltaTime;
        if (timeBetweenAnimations.Count != 0 && currentTime >= timeBetweenAnimations[0])
        {
            if (playNewAnimation)
            {
                if (DebugAnimations)
                {
                    Debug.Log("Playing New Line Of Animation");
                    debugWord = "";
                }

                debugWord += animations[0].ToString() + " ";
                animations[0].PlayAnimation();

                if (DebugAnimations)
                {
                    Debug.Log(debugWord);
                }
            }
            currentTime = 0;
        }

        if(currentTime > 100)
        {
            currentTime = 0;
        }
    }

    public void AddAnimations(CustomAnimations newAnimation, int index, float timeInterval = 0)
    {
        combatGameManager.playingAnimation = true;
        newAnimation.spriteManager = this;
        playNewAnimation = true;
        if (animations.Count == 0)
        {
            newAnimation.xindex = 0;
            playNewAnimation = true;
            animations.Add(newAnimation);
            enabled = true;
        }
        else if (index >= animations.Count)
        {
            newAnimation.xindex = animations.Count;
            animations.Add(newAnimation);
        }
        else
        {
            newAnimation.xindex = index;
            animations.Insert(index, newAnimation);

            for (int i = index + 1; i < animations.Count; i++)
            {
                animations[i].xindex = i;
            }

        }

        timeBetweenAnimations.Add(timeInterval);

        if (DebugAnimations)
        {
            string debugWord = "";
            for (int i = 0; i < animations.Count; i++)
            {
                debugWord += "index " + i + ": " + animations[i].ToString();
                debugWord += "\n";
            }

            Debug.Log(debugWord);
        }
    }

    public void EndAnimations()
    {
        playNewAnimation = false;
        combatGameManager.playingAnimation = false;
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

    public void DrawLine(List<Vector3> oneActionLinePath = null, List<Vector3> twoActionLinePath = null)
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

    public void DrawLine(List<Vector3> linePath, int lineControllerIndex)
    {
        if (linePath != null)
        {
            lineControllers[lineControllerIndex].SetLine(linePath);
        }
    }

    public void ClearLines()
    {
        List<Vector3> emptyList = new List<Vector3>();
        for(int i = 0; i < lineControllers.Count; i++)
        {
            lineControllers[i].SetLine(emptyList);
        }
        oneActionLineController.SetLine(emptyList);
        twoActionLineController.SetLine(emptyList);
    }

    public void ActivateMovementTargeting(Unit movingUnit, bool targetFriendly, int actionPointsLeft)
    {
        inputManager.TargetPositionMoved += movementTargeting.SelectNewPosition;
        inputManager.FoundPosition += movementTargeting.EndTargeting;
        activeTargetingSystems = movementTargeting;
        movementTargeting.SetParameters(movingUnit, targetFriendly, actionPointsLeft);
        movementTargeting.SelectNewPosition(UtilsClass.GetMouseWorldPosition());
    }

    public void ActivateMeleeAttackTargeting(Unit movingUnit, bool targetFriendly, int actionPointsLeft, int actionPointUseAmount, int meleeRange,
        MeleeTargeting.CalculateAttackData calculateAttackData)
    {
        combatUI.OnActivateTargetingSystem();
        inputManager.TargetPositionMoved += meleeTargeting.SelectNewPosition;
        inputManager.FoundPosition += meleeTargeting.EndTargeting;
        activeTargetingSystems = meleeTargeting;
        meleeTargeting.SetParameters(movingUnit, targetFriendly, actionPointsLeft, actionPointUseAmount, meleeRange, calculateAttackData);
        meleeTargeting.SelectNewPosition(UtilsClass.GetMouseWorldPosition());
    }

    public void ActivateConeTargeting(Unit movingUnit, bool targetFriendly, int actionPointsLeft, int range, int coneRange)
    {
        combatUI.OnActivateTargetingSystem();
        inputManager.TargetPositionMoved += coneTargeting.SelectNewPosition;
        inputManager.FoundPosition += coneTargeting.EndTargeting;
        activeTargetingSystems = coneTargeting;
        coneTargeting.SetParameters(movingUnit, targetFriendly, actionPointsLeft, range, coneRange);
        coneTargeting.SelectNewPosition(UtilsClass.GetMouseWorldPosition());
    }

    public void DeactiveTargetingSystem()
    {
        combatUI.OnDeactivateTargetingSystem();
        CancelAction();
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

    public GameObject CreateTempSpriteHolder(Vector2Int hexPosition, int spriteLayer, Sprite sprite)
    {
        Vector3 worldPosition = combatGameManager.grid.GetWorldPosition(hexPosition.x, hexPosition.y);
        return CreateSpriteRenderer(spriteLayer, -7, sprite, worldPosition);
    }

    public GameObject CreateSpriteRenderer(int index, int sortingOrder, Sprite sprite, Vector3 objectPosition)
    {
        SpriteNode currentSpriteNode = spriteGrid.GetGridObject(objectPosition);
        SpriteRenderer ownSpriteRenderer = Instantiate(resourceManager.spriteHolder, objectPosition, new Quaternion(0, 0, 0, 1f));
        ownSpriteRenderer.sortingOrder = sortingOrder;
        ownSpriteRenderer.transform.parent = transform;
        currentSpriteNode.sprites[index] = ownSpriteRenderer;
        currentSpriteNode.sprites[index].sprite = sprite;
        return ownSpriteRenderer.gameObject;
    }

    public void ActivateCombatAttackUI(Unit targetUnit, List<AttackDataUI> attackData, Vector3 newPosition)
    {
        combatUI.SetDataAttackUi(targetUnit, attackData, newPosition);
    }

    public void ResetCombatAttackUI()
    {
        combatUI.ResetDataAttackUI();
    }
}
