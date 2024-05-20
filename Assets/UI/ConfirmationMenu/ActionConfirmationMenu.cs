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
    public void Awake()
    {
        gameObject.SetActive(false);
    }
    
    public void ActivateMenu(   UnityAction confirmAction, UnityAction cancelAction)
    {
        this.gameObject.SetActive(true);

        //Remove any existing Listeners to make sure there aren't any previous ones hanging around
        // Note -  this only remveos listeners added through code
        confirmButton.onClick.RemoveAllListeners();

        // assign the the onClick Listeners
        confirmButton.onClick.AddListener(() =>
        {
            DeactivateMenu();
            confirmAction();
        });

        this.cancelAction = cancelAction;
        this.confirmAction = confirmAction;
    }

    public void ActivateConfirmAction()
    {
        DeactivateMenu();
        confirmAction();
    }

    public void ActivateCancelAction()
    {
        DeactivateMenu();
        cancelAction();
    }

    private void DeactivateMenu()
    {
        this.gameObject.SetActive(false);
    }
}