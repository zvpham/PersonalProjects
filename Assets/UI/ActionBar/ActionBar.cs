using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ActionBar : MonoBehaviour
{
    [SerializeField]
    private RectTransform actionsPanel;

    [SerializeField]
    private ActionButton actionButtonPrefab;

    [SerializeField]
    private Button PageUpButton;

    [SerializeField]
    private Button PageDownButton;

    [SerializeField]
    private int maxActionBarSize;

    [SerializeField]
    private int newButtonSizeValue;

    [SerializeField]
    private int currentActionBarLevelIndex = 0;

    [SerializeField]
    private List<ActionButton> currentActionButtons = new List<ActionButton>();

    public List<ActionBarLevel> actionBarlevelList = new List<ActionBarLevel>();

    public PlayerTurn player;

    public static ActionBar Instance;


    public struct ActionBarLevel
    {
        public List<Sprite> actionImages;
        public List<int> actionCooldowns;
        public List<int> actionUses;
        public int currentBarSize;

        public ActionBarLevel(List<Sprite> sprites, List<int> actionCooldowns, List<int> actionUses, int currentBarSize)
        {
            this.actionImages = sprites;
            this.actionCooldowns = actionCooldowns;
            this.actionUses = actionUses;
            this.currentBarSize = currentBarSize;
        }
    }

    public void ResetActionBarList()
    {
        ClearActionButtons();
        actionBarlevelList.Clear();
    }

    public void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Found more than ONe Action Bar in the Scence");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    public void HandleActionButtonPress(int actionIndex)
    {
        //Debug.Log(actionName.ToString());
        player.OnActionButtonPresed(actionIndex);
    }

    public void UpdateActionsDisplay()
    {
        for (int i = 0; i < currentActionButtons.Count; i++)
        {
            currentActionButtons[i].GetComponent<Button>().interactable = false;
            currentActionButtons[i].useAction -= HandleActionButtonPress;
        }

        for (int i = 0; i < currentActionButtons.Count; i++)
        {
            if (actionBarlevelList[currentActionBarLevelIndex].actionCooldowns[i] == 0 || actionBarlevelList[currentActionBarLevelIndex].actionUses[i] == 0)
            {
                currentActionButtons[i].GetComponent<Button>().interactable = true;
                currentActionButtons[i].useAction += HandleActionButtonPress;
                currentActionButtons[i].ChangeSprite(actionBarlevelList[currentActionBarLevelIndex].actionImages[i]);
            }
            currentActionButtons[i].ChangeActionIndex(i + 1);
            currentActionButtons[i].ChangeActionCoolDown(actionBarlevelList[currentActionBarLevelIndex].actionCooldowns[i].ToString());
            currentActionButtons[i].ChangeUsesLeft(actionBarlevelList[currentActionBarLevelIndex].actionUses[i].ToString());
        }
    }

    public void AddAction(Sprite actionSprite, int coolDown, int actionUses)
    {
        // Checking to see if Action has already been added to one of the actionBarLevels
        if(GetIndexOfActionSprite(actionSprite).x != -1)
        {
            Debug.LogWarning("Action has alraedy been added: " + actionSprite);
            return;
        }

        // Base Case -  an ActionBar hasn't been created yet
        // New Action Bar Created and new action added to it
        if(actionBarlevelList.Count == 0)
        {
            actionBarlevelList.Add(new ActionBarLevel(new List<Sprite>(), new List<int>(), new List<int>(), 0));
            ActionBarLevel currentActionBarLevel = actionBarlevelList[0];
            currentActionBarLevel.actionImages.Add(actionSprite);
            currentActionBarLevel.actionCooldowns.Add(coolDown);
            currentActionBarLevel.actionUses.Add(actionUses);
            currentActionBarLevel.currentBarSize += newButtonSizeValue;
            AddNewButton(actionSprite, coolDown, actionUses);
        }
        // Case - new Action goes over maxBarSizeValue Limit
        // Create a new Bar Level and add Action to it
        else if (actionBarlevelList[actionBarlevelList.Count - 1].currentBarSize + newButtonSizeValue > maxActionBarSize)
        {
            actionBarlevelList.Add(new ActionBarLevel(new List<Sprite>(), new List<int>(), new List<int>(), 0));
            ActionBarLevel currentActionBarLevel = actionBarlevelList[0];
            currentActionBarLevel.actionImages.Add(actionSprite);
            currentActionBarLevel.actionCooldowns.Add(coolDown);
            currentActionBarLevel.actionUses.Add(actionUses);
            currentActionBarLevel.currentBarSize += newButtonSizeValue;
        }
        // case -  new Action Added to latest ActionBarLevel and There is room 
        // Just add action to newest bar leve
        else
        {
            ActionBarLevel currentActionBarLevel = actionBarlevelList[actionBarlevelList.Count - 1];
            currentActionBarLevel.actionImages.Add(actionSprite);
            currentActionBarLevel.actionCooldowns.Add(coolDown);
            currentActionBarLevel.actionUses.Add(actionUses);
            currentActionBarLevel.currentBarSize += newButtonSizeValue;

            // Adding a button if currentBarLevel is the newestBarLevel
            // Instantiainting new Button
            if(currentActionBarLevelIndex == actionBarlevelList.Count - 1)
            {
                AddNewButton(actionSprite, coolDown, actionUses);
            }
        }
        UpdateActionsDisplay();
    }

    public void AddAction(Sprite actionSprite, int coolDown, int actionUses, int actionBarLevel)
    {
        // Checking to see if Action has already been added to one of the actionBarLevels
        if (GetIndexOfActionSprite(actionSprite).x != -1)
        {
            Debug.LogError(" SOMETHING IS REALLY WRONG Action has alraedy been added: " + actionSprite);
            return;
        }

        ActionBarLevel currentActionBarLevel = actionBarlevelList[actionBarLevel];
        currentActionBarLevel.actionImages.Add(actionSprite);
        currentActionBarLevel.actionCooldowns.Add(coolDown);
        currentActionBarLevel.actionUses.Add(actionUses);
        currentActionBarLevel.currentBarSize += newButtonSizeValue;

        if (actionBarLevel == currentActionBarLevelIndex)
        {

            AddNewButton(actionSprite, coolDown, actionUses);
        }
        UpdateActionsDisplay();
    }

    public void RemoveAction(Sprite actionSprite)
    {
        Vector2Int actionNameIndexes = GetIndexOfActionSprite(actionSprite);
        // Checking to see if Action exists in one of the actionBarLevels
        if (actionNameIndexes.x == -1) 
        { 
            Debug.LogError("Action Does not Exist, Cant Remove: " + actionSprite);
            return;
        }

        // Case -  Action is in currentActionBarLevel
        // Remove Action button Remove Action From its ActionBarLevel
        if(actionNameIndexes.x == currentActionBarLevelIndex)
        {
            ActionBarLevel currentBarlevel = actionBarlevelList[actionNameIndexes.x];
            currentBarlevel.currentBarSize -= newButtonSizeValue;
            currentBarlevel.actionImages.RemoveAt(actionNameIndexes.y);
            currentBarlevel.actionCooldowns.RemoveAt(actionNameIndexes.y);
            currentBarlevel.actionUses.RemoveAt(actionNameIndexes.y);

            Destroy(currentActionButtons[actionNameIndexes.y].gameObject);
            currentActionButtons.RemoveAt(actionNameIndexes.y);
        }
        // Case -  Action is not in currentActionBarLevel
        // Remove Action From its ActionBarLevel
        else
        {
            ActionBarLevel currentBarlevel = actionBarlevelList[actionNameIndexes.x];
            currentBarlevel.currentBarSize -= newButtonSizeValue;
            currentBarlevel.actionImages.RemoveAt(actionNameIndexes.y);
            currentBarlevel.actionCooldowns.RemoveAt(actionNameIndexes.y);
            currentBarlevel.actionUses.RemoveAt(actionNameIndexes.y);
        }

        int actionBarLevelIndex = actionNameIndexes.x + 1;
        while(actionBarLevelIndex <= actionBarlevelList.Count - 1)
        {
            if (actionBarlevelList[actionBarLevelIndex - 1].currentBarSize + newButtonSizeValue <= maxActionBarSize)
            {
                AddAction(actionBarlevelList[actionBarLevelIndex].actionImages[0], actionBarlevelList[actionBarLevelIndex].actionCooldowns[0], actionBarlevelList[actionBarLevelIndex].actionUses[0], actionBarLevelIndex);
                ActionBarLevel currentBarlevel = actionBarlevelList[actionBarLevelIndex];
                currentBarlevel.currentBarSize -= newButtonSizeValue;
                currentBarlevel.actionImages.RemoveAt(0);
                currentBarlevel.actionCooldowns.RemoveAt(0);
                currentBarlevel.actionUses.Remove(0);

                if(actionBarLevelIndex ==  currentActionBarLevelIndex)
                {
                    Destroy(currentActionButtons[0].gameObject);
                    currentActionButtons.RemoveAt(0);
                }
                actionBarLevelIndex += 1;
            }
            else
            {
                break;
            }
        }
    }

    public void AddNewButton(Sprite actionSprite, int coolDown, int numUses)
    {
        ActionButton actionButton = Instantiate(actionButtonPrefab, Vector3.zero, Quaternion.identity);
        actionButton.transform.SetParent(actionsPanel);
        actionButton.ChangeSprite(actionSprite);
        actionButton.ChangeUsesLeft(numUses.ToString());
        actionButton.ChangeActionCoolDown(coolDown.ToString());
        currentActionButtons.Add(actionButton);
    }

    public void UpdateCoolDowns(Sprite actionSprite, int coolDown)
    {
        Vector2Int actionNameIndexes = GetIndexOfActionSprite(actionSprite);
        actionBarlevelList[actionNameIndexes.x].actionCooldowns[actionNameIndexes.y] = coolDown;
    } 

    public void UpdateActionUses(Sprite actionSprite, int actionUses)
    {
        Vector2Int actionNameIndexes = GetIndexOfActionSprite(actionSprite);
        actionBarlevelList[actionNameIndexes.x].actionUses[actionNameIndexes.y] = actionUses;
    }



    public void ClearActionButtons()
    {
        foreach(ActionButton actionButton in currentActionButtons)
        {
            Destroy(actionButton.gameObject);
        }
        currentActionButtons.Clear();
    }

    public void OnActionRebind()
    {

    }

    public void OnPageUp()
    {
        ClearActionButtons();
        currentActionBarLevelIndex += 1;
        ActionBarLevel currentActionBar = actionBarlevelList[currentActionBarLevelIndex];
        for(int i = 0; i < currentActionBar.actionImages.Count; i++)
        {
            AddNewButton(currentActionBar.actionImages[i], currentActionBar.actionCooldowns[i], currentActionBar.actionUses[i]);
        }
        UpdateActionsDisplay();
    }

    public void OnPageDown()
    {
        ClearActionButtons();
        currentActionBarLevelIndex -= 1;
        ActionBarLevel currentActionBar = actionBarlevelList[currentActionBarLevelIndex];
        for (int i = 0; i < currentActionBar.actionImages.Count; i++)
        {
            AddNewButton(currentActionBar.actionImages[i], currentActionBar.actionCooldowns[i], currentActionBar.actionUses[i]);
        }
        UpdateActionsDisplay();
    }

    public void UpdateDisplayPageUpDown()
    {
        if(currentActionBarLevelIndex == 0)
        {
           
        }
    }

    // item1 is actionBarlevelIndex. Item2 is index within actionbar
    public Vector2Int GetIndexOfActionSprite(Sprite actionSprite) 
    { 
        if(actionBarlevelList.Count == 0)
        {
            return new Vector2Int(-1, -1);
        }

        for(int i = 0; i < actionBarlevelList.Count; i++)
        {
            for(int j = 0; j < actionBarlevelList[i].actionImages.Count; j++)
            {
                if (actionBarlevelList[i].actionImages[j] == actionSprite)
                {
                    return new Vector2Int(i, j);
                }
            }
        }
        return new Vector2Int(-1, -1); ;
    }
}
