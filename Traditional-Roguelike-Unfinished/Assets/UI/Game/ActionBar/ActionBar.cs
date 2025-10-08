using Inventory.UI;
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
    private int characterCountSizeValue;

    [SerializeField]
    private int currentActionBarLevelIndex = 0;

    [SerializeField]
    private List<ActionButton> currentActionButtons = new List<ActionButton>();

    public List<ActionBarLevel> actionBarlevelList = new List<ActionBarLevel>();

    public Player player;

    public static ActionBar Instance;

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

    public void HandleActionButtonPress(ActionName actionName)
    {
        //Debug.Log(actionName.ToString());
        player.OnActionButtonPressed(actionName);
    }
    [System.Serializable]
    public struct ActionBarLevel
    {
        public List<ActionName> actions;
        public List<int> actionCooldowns;
        public int currentBarSize;

        public ActionBarLevel(List<ActionName> actions, List<int> actionCooldowns, int currentBarSize)
        {
            this.actions = actions;
            this.actionCooldowns = actionCooldowns;
            this.currentBarSize = currentBarSize;
        }
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
            if (actionBarlevelList[currentActionBarLevelIndex].actionCooldowns[i] == 0)
            {
                currentActionButtons[i].GetComponent<Button>().interactable = true;
                currentActionButtons[i].useAction += HandleActionButtonPress;
                currentActionButtons[i].action = actionBarlevelList[currentActionBarLevelIndex].actions[i];
            }
            currentActionButtons[i].ChangeActionCoolDown(actionBarlevelList[currentActionBarLevelIndex].actionCooldowns[i].ToString());
        }
    }

    public void AddAction(ActionName actionName, int coolDown)
    {
        // Checking to see if Action has already been added to one of the actionBarLevels
        if(GetIndexOfActionName(actionName) != null)
        {
            Debug.LogWarning("Action has alraedy been added: " + actionName);
            return;
        }

        // Base Case -  an ActionBar hasn't been created yet
        // New Action Bar Created and new action added to it
        if(actionBarlevelList.Count == 0)
        {
            actionBarlevelList.Add(new ActionBarLevel(new List<ActionName>(), new List<int>(), 0));
            ActionBarLevel currentActionBarLevel = actionBarlevelList[0];
            currentActionBarLevel.actions.Add(actionName);
            currentActionBarLevel.actionCooldowns.Add(coolDown);
            currentActionBarLevel.currentBarSize += newButtonSizeValue + actionName.ToString().Length * characterCountSizeValue;
            AddNewButton(actionName, coolDown);
        }
        // Case - new Action goes over maxBarSizeValue Limit
        // Create a new Bar Level and add Action to it
        else if (actionBarlevelList[actionBarlevelList.Count - 1].currentBarSize + newButtonSizeValue + actionName.ToString().Length * characterCountSizeValue > maxActionBarSize)
        {
            actionBarlevelList.Add(new ActionBarLevel(new List<ActionName>(), new List<int>(), 0));
            ActionBarLevel currentActionBarLevel = actionBarlevelList[0];
            currentActionBarLevel.actions.Add(actionName);
            currentActionBarLevel.actionCooldowns.Add(coolDown);
            currentActionBarLevel.currentBarSize += newButtonSizeValue + actionName.ToString().Length * characterCountSizeValue;
        }
        // case -  new Action Added to latest ActionBarLevel and There is room 
        // Just add action to newest bar leve
        else
        {
            ActionBarLevel currentActionBarLevel = actionBarlevelList[actionBarlevelList.Count - 1];
            currentActionBarLevel.actions.Add(actionName);
            currentActionBarLevel.actionCooldowns.Add(coolDown);
            currentActionBarLevel.currentBarSize += newButtonSizeValue + actionName.ToString().Length * characterCountSizeValue;

            // Adding a button if currentBarLevel is the newestBarLevel
            // Instantiainting new Button
            if(currentActionBarLevelIndex == actionBarlevelList.Count - 1)
            {
                AddNewButton(actionName, coolDown);
            }
        }
    }

    public void AddAction(ActionName actionName, int coolDown, int actionBarLevel)
    {
        // Checking to see if Action has already been added to one of the actionBarLevels
        if (GetIndexOfActionName(actionName) != null)
        {
            Debug.LogError(" SOMETHING IS REALLY WRONG Action has alraedy been added: " + actionName);
            return;
        }

        ActionBarLevel currentActionBarLevel = actionBarlevelList[actionBarLevel];
        currentActionBarLevel.actions.Add(actionName);
        currentActionBarLevel.actionCooldowns.Add(coolDown);
        currentActionBarLevel.currentBarSize += newButtonSizeValue + actionName.ToString().Length * characterCountSizeValue;

        if (actionBarLevel == currentActionBarLevelIndex)
        {

            AddNewButton(actionName, coolDown);
        }
    }

    public void RemoveAction(ActionName actionName)
    {
        Tuple<int, int> actionNameIndexes = GetIndexOfActionName(actionName);
        // Checking to see if Action exists in one of the actionBarLevels
        if (actionNameIndexes == null) 
        { 
            Debug.LogError("Action Does not Exist, Cant Remove: " +  actionName);
            return;
        }

        // Case -  Action is in currentActionBarLevel
        // Remove Action button Remove Action From its ActionBarLevel
        if(actionNameIndexes.Item1 == currentActionBarLevelIndex)
        {
            ActionBarLevel currentBarlevel = actionBarlevelList[actionNameIndexes.Item1];
            currentBarlevel.currentBarSize -= newButtonSizeValue + actionName.ToString().Length * characterCountSizeValue;
            currentBarlevel.actions.RemoveAt(actionNameIndexes.Item2);
            currentBarlevel.actionCooldowns.RemoveAt(actionNameIndexes.Item2);

            Destroy(currentActionButtons[actionNameIndexes.Item2].gameObject);
            currentActionButtons.RemoveAt(actionNameIndexes.Item2);
        }
        // Case -  Action is not in currentActionBarLevel
        // Remove Action From its ActionBarLevel
        else
        {
            ActionBarLevel currentBarlevel = actionBarlevelList[actionNameIndexes.Item1];
            currentBarlevel.currentBarSize -= newButtonSizeValue + actionName.ToString().Length * characterCountSizeValue;
            currentBarlevel.actions.RemoveAt(actionNameIndexes.Item2);
            currentBarlevel.actionCooldowns.RemoveAt(actionNameIndexes.Item2);
        }

        int actionBarLevelIndex = actionNameIndexes.Item1 + 1;
        while(actionBarLevelIndex <= actionBarlevelList.Count - 1)
        {
            if (actionBarlevelList[actionBarLevelIndex - 1].currentBarSize + newButtonSizeValue +
                actionBarlevelList[actionBarLevelIndex].actions[0].ToString().Length * characterCountSizeValue <= maxActionBarSize)
            {
                AddAction(actionBarlevelList[actionBarLevelIndex].actions[0], actionBarlevelList[actionBarLevelIndex].actionCooldowns[0], actionBarLevelIndex);
                ActionBarLevel currentBarlevel = actionBarlevelList[actionBarLevelIndex];
                currentBarlevel.currentBarSize -= newButtonSizeValue + actionName.ToString().Length * characterCountSizeValue;
                currentBarlevel.actions.RemoveAt(0);
                currentBarlevel.actionCooldowns.RemoveAt(0);

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

    public void AddNewButton(ActionName actionName, int coolDown)
    {
        ActionButton actionButton = Instantiate(actionButtonPrefab, Vector3.zero, Quaternion.identity);
        actionButton.transform.SetParent(actionsPanel);
        actionButton.ChangeActionName(actionName.ToString());
        actionButton.ChangeActionCoolDown(coolDown.ToString());
        currentActionButtons.Add(actionButton);
    }

    public void UpdateCoolDowns(ActionName actionName, int coolDown)
    {
        Tuple<int, int> actionNameIndexes = GetIndexOfActionName(actionName);
        actionBarlevelList[actionNameIndexes.Item1].actionCooldowns[actionNameIndexes.Item2] = coolDown;
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
        for(int i = 0; i < currentActionBar.actions.Count; i++)
        {
            AddNewButton(currentActionBar.actions[i], currentActionBar.actionCooldowns[i]);
        }
        UpdateActionsDisplay();
    }

    public void OnPageDown()
    {
        ClearActionButtons();
        currentActionBarLevelIndex -= 1;
        ActionBarLevel currentActionBar = actionBarlevelList[currentActionBarLevelIndex];
        for (int i = 0; i < currentActionBar.actions.Count; i++)
        {
            AddNewButton(currentActionBar.actions[i], currentActionBar.actionCooldowns[i]);
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
    public Tuple<int, int> GetIndexOfActionName(ActionName actionName) 
    { 
        if(actionBarlevelList.Count == 0)
        {
            return null;
        }

        for(int i = 0; i < actionBarlevelList.Count; i++)
        {
            for(int j = 0; j < actionBarlevelList[i].actions.Count; j++)
            {
                if (actionBarlevelList[i].actions[j].Equals(actionName))
                {
                    return new Tuple<int, int>(i, j);
                }
            }
        }
        return null;
    }
}
