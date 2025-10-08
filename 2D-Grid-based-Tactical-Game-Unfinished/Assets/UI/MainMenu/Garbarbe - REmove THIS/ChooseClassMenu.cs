using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChooseClassMenu : BaseUIPage
{
    public List<BaseGameUIObject> gameUIObjects;
    public ResourceManager resourceManager;
    public DataPersistenceManager dataPersistenceManager;
    public RectTransform Classes;
    public BaseGameUIObject JobClass;
    public BaseGameUIObject RaceClass;
    public UIClassDescription classDescription;
    public GameObject confirmButton;
    public GameData gameData;
    public int maxJobClasses = 1;
    public int maxRaceClasses = 1;

    [SerializeField]
    private InputManager inputManager;
    public List<MenuAction> menuActions;

    // Start is called before the first frame update
    public override void Start()
    {
        gameData = dataPersistenceManager.GetGameData();
        JobClass.UIPage = this;
        JobClass.SetOriginalText(JobClass.GetText());
        RaceClass.UIPage = this;
        RaceClass.SetOriginalText(RaceClass.GetText());
        LoadGameData();
        gameObject.SetActive(false);
        bottomIndex = maxUIObjectsVisibleOnScreen;
        confirmButton.GetComponent<CanvasGroup>().interactable = false;
    }

    public void OnConfirmClicked()
    {
        SaveGameAndLoadScene();
    }


    public void OpenMenu()
    {
        UpdateOnScreeenUIObjects();
    }

    private void SaveGameAndLoadScene()
    {
        /*
        List<int> classIndexes =  new List<int>();
        List<int> classLevels = new List<int>();
        for (int i = 0; i < chosenClassList.Count; i++)
        {
            classIndexes.Add(chosenClassList[i].Class.classIndex);
            classLevels.Add(1);
        }

        // save the game anytime before Loading a new Scene
        MapData newMapData =  DataPersistenceManager.Instance.GetMapData();
        unitPrefabData newPlayerData = new unitPrefabData();
        newPlayerData.classIndexes = classIndexes;
        newPlayerData.classLevels = classLevels;
        newMapData.unitPrefabDatas = newPlayerData;
        DataPersistenceManager.Instance.SetMapData(newMapData);

        DataPersistenceManager.Instance.SaveGame(0, DataPersistenceManager.Instance.playerID);
        DataPersistenceManager. Instance.SaveGame(DataPersistenceManager.Instance.autoSaveID, DataPersistenceManager.Instance.playerID);
        DataPersistenceManager.Instance.SaveWorldMapData();
        DataPersistenceManager.userID = DataPersistenceManager.Instance.playerID;
        // Load the Scene -  Which will inturn save the game Because of OnSceneUnloaded() in the DataPersistenceManager
        SceneManager.LoadSceneAsync("Game");
        */
    }
    public void LoadGameData()
    {
        /*
        for(int i = 0; i < gameData.startingClasses.Count; i++)
        {
            Class newClass = Instantiate(resourceManager.classes[gameData.startingClasses[i]]);
            UIClass newUiClass = Instantiate(UIClassPrefab, Classes);
            newUiClass.UIPage = this;
            newUiClass.Class = newClass;
            if (newClass.jobClass)
            {
                newUiClass.transform.SetSiblingIndex(gameUIObjects.IndexOf(RaceClass));
                jobClasses.Add(newUiClass);
                JobClass.groupMembers.Add(newUiClass);
            }
            else
            {
                newUiClass.transform.SetAsLastSibling();
                racialClasses.Add(newUiClass);
                RaceClass.groupMembers.Add(newUiClass);
            }

            newUiClass.SetOriginalText(newClass.className.ToString());
            newUiClass.setAmountOfIndents(1);
            newUiClass.SetText(newUiClass.AddIndents(newUiClass.GetOriginalText()));
        }
        UpdateBaseUIObjects();
        */
    }


    public void UpdateOnScreeenUIObjects()
    {
        onScreenUIObjects = new List<BaseGameUIObject>();
        int bottomOfMenu = bottomIndex;
        if (bottomIndex > activeUIObjects.Count)
        {
            bottomOfMenu = activeUIObjects.Count;
        }
        for (int i = topIndex; i < bottomOfMenu; i++)
        {
            onScreenUIObjects.Add(activeUIObjects[i]);
            string originalText = activeUIObjects[i].GetOriginalText();
            activeUIObjects[i].SetText("   " + activeUIObjects[i].AddIndents(selectionNames[i - topIndex] + ") " + originalText));
        }
        activeUIObjects[currentIndex].SetText(selectedIcon + activeUIObjects[currentIndex].GetText().Substring(3));
    }

    public override void UpdateBaseUIObjects()
    {
        bool newMenu = true;
        BaseGameUIObject currentObject = null;
        if (activeUIObjects.Count > 0)
        {
            currentObject = activeUIObjects[currentIndex];
            newMenu = false;
        }
        activeUIObjects = new List<BaseGameUIObject>();
        JobClass.GetActiveBaseUIOBjects(activeUIObjects);
        RaceClass.GetActiveBaseUIOBjects(activeUIObjects);

        activeUIObjects.Remove(RaceClass);
        activeUIObjects.Remove(JobClass);

        for(int i = 0; i < activeUIObjects.Count; i++)
        {
            /*
            UIClass currentClass = (UIClass)activeUIObjects[i];
            if (!currentClass.interactable)
            {
                activeUIObjects.RemoveAt(i);
                i--;
            }
            */
        }
        if(!newMenu)
        {
            currentIndex = activeUIObjects.IndexOf(currentObject);
        }
        UpdateOnScreeenUIObjects();
    }

   

    public override void MouseMoved()
    {
        classDescription.ResetDescription();
    }

    public override void SelectMenuObject(int itemIndex)
    {
        BaseGameUIObject currentObject = activeUIObjects[currentIndex];
        base.SelectMenuObject(itemIndex);
        if (itemIndex < activeUIObjects.Count)
        {
            classDescription.ResetDescription();
            currentObject.SetText(currentObject.AddIndents(currentObject.GetOriginalText()));
            currentIndex = itemIndex;
            currentObject = activeUIObjects[currentIndex];
            currentObject.SetText(selectedIcon + currentObject.GetText());
            currentObject.HoverUI();
        }
        UpdateOnScreeenUIObjects();
    }
    public override void IndexUp()
    {
        if (currentIndex - 1 >= 0)
        {
            classDescription.ResetDescription();
            if (currentIndex == topIndex && topIndex > 0)
            {
                topIndex -= 1;
                bottomIndex -= 1;
                contentPanel.anchoredPosition = new Vector2(0, contentPanel.GetComponent<GridLayoutGroup>().cellSize.y * topIndex);
            }
            currentIndex -= 1;
            activeUIObjects[currentIndex].SetText(activeUIObjects[currentIndex].AddIndents(activeUIObjects[currentIndex].GetOriginalText()));
            if (activeUIObjects[currentIndex].groupMembers.Count == 0)
            {
                activeUIObjects[currentIndex].HoverUI();
            }
        }
        UpdateOnScreeenUIObjects();
    }

    public override void IndexDown()
    {
        if (currentIndex + 1 < activeUIObjects.Count)
        {
            classDescription.ResetDescription();
            activeUIObjects[currentIndex].SetText(activeUIObjects[currentIndex].AddIndents(activeUIObjects[currentIndex].GetOriginalText()));
            currentIndex += 1;
            if (activeUIObjects[currentIndex].groupMembers.Count == 0)
            {
                activeUIObjects[currentIndex].HoverUI();
            }
            if (currentIndex == bottomIndex && bottomIndex < activeUIObjects.Count)
            {
                topIndex += 1;
                bottomIndex += 1;
                contentPanel.anchoredPosition = new Vector2(0, contentPanel.GetComponent<GridLayoutGroup>().cellSize.y * topIndex);
            }
        }
        UpdateOnScreeenUIObjects();
    }

    public void ActivateMenu()
    {
        this.gameObject.SetActive(true);
        currentIndex = 0;
        topIndex = 0;
        bottomIndex = maxUIObjectsVisibleOnScreen;
    }

    public void DeactivateMenu()
    {
        this.gameObject.SetActive(false);
    }
}
    