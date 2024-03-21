using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameMenu : MonoBehaviour
{
    public UIClassPage classPage;
    [SerializeField]
    private InputManager inputManager;

    private int currentIndex;
    public List<BaseUIPage> pages;

    public Player player;
    public List<MenuAction> menuActions;

    public static GameMenu Instance;

    public void Awake()
    {
        Instance = this;
    }
    public void Start()
    {
        inputManager = InputManager.instance;
        player = Player.Instance;
        gameObject.SetActive(false);
    }

    public void PageUp()
    {
        pages[currentIndex].gameObject.SetActive(false);
        currentIndex += 1;
        if(currentIndex >= pages.Count)
        {
            currentIndex = 0;
        }
        pages[currentIndex].gameObject.SetActive(true);
    }

    public void PageDown()
    {
        pages[currentIndex].gameObject.SetActive(false);
        currentIndex -= 1;
        if (currentIndex < 0)
        {
            currentIndex = pages.Count - 1;
        }
        pages[currentIndex].gameObject.SetActive(true);
    }

    public void OpenMenu(int currentIndex)
    {
        this.currentIndex = currentIndex;
        this.gameObject.SetActive(true);
        pages[currentIndex].gameObject.SetActive(true);
    }

    public void CloseMenu()
    {
        this.gameObject.SetActive(false);
        player.CloseGameMenu();
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < menuActions.Count; i++)
        {
            if (inputManager.GetKeyDownMenu(menuActions[i].menuInputName))
            {
                menuActions[i].Activate(pages[currentIndex]);
            }
        }
    }
}