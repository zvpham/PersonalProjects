using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameMenu : MonoBehaviour
{
    public UIClassPage classPage;
    [SerializeField]
    private InputManager inputManager;

    [Header("Confirmation Popup")]
    [SerializeField] public ConfirmationPopupMenu confirmPopupMenu;

    private int currentIndex;
    public List<BaseUIPage> pages;

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
    }

    public void EnableMenuInputs()
    {
        enabled = true;
    }

    public void DisableMenuInputs()
    {
        enabled = false;
    }

    public void ResetMenu()
    {
        for(int i = 0; i < pages.Count; i++)
        {
            pages[i].ResetPage();
        }
    }
}