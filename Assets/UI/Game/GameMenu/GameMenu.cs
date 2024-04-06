using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameMenu : MonoBehaviour
{
    public UIClassPage classPage;
    [SerializeField]
    private InputManager inputManager;

    [SerializeField]
    private VirtualCursor virtualCursor;

    [Header("Confirmation Popup")]
    [SerializeField] public ConfirmationPopupMenu confirmPopupMenu;

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
        gameObject.SetActive(false);
    }
    public void OpenMenu(int currentIndex)
    {
        this.currentIndex = currentIndex;
        this.gameObject.SetActive(true);
        pages[currentIndex].gameObject.SetActive(true);
        pages[currentIndex].UpdateBaseUIObjects();
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

    public void EnableMenuInputs()
    {
        enabled = true;
        virtualCursor.enabled = true;
    }

    public void DisableMenuInputs()
    {
        enabled = false;
        virtualCursor.enabled = false;
    }

    public void ResetMenu()
    {
        for(int i = 0; i < pages.Count; i++)
        {
            pages[i].ResetPage();
        }
    }
}