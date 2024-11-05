using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class ActionConfirmationMenu : Menu
{
    [Header("Componenets")]
    [SerializeField] private Button confirmButton;

    private UnityAction cancelAction;
    private UnityAction confirmAction;
    private bool activated = false;
    public void Awake()
    {
        gameObject.SetActive(false);
    }
    
    public void ActivateMenu(UnityAction confirmAction, UnityAction cancelAction)
    {
        activated = false;
        this.gameObject.SetActive(true);

        //Remove any existing Listeners to make sure there aren't any previous ones hanging around
        // Note -  this only remveos listeners added through code
        confirmButton.onClick.RemoveAllListeners();

        // assign the the onClick Listeners
        confirmButton.onClick.AddListener(() =>
        {
            if(!activated)
            {
                DeactivateMenu();
                confirmAction();
                activated = true;
            }
        });

        this.cancelAction = cancelAction;
        this.confirmAction = confirmAction;
    }

    public void ActivateConfirmAction()
    {
        Debug.Log("Confirm Action");
        if (this.gameObject.activeSelf && !activated)
        {
            DeactivateMenu();
            confirmAction();
            activated = true;
        }
    }

    public void ActivateCancelAction()
    {
        if (this.gameObject.activeSelf && !activated)
        {
            DeactivateMenu();
            cancelAction();
            activated = true;
        }
    }

    private void DeactivateMenu()
    {
        this.gameObject.SetActive(false);
    }
}