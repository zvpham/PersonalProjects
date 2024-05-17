using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class ActionConfirmationMenu : Menu
{
    [Header("Componenets")]
    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private Button confirmButton;

    private UnityAction cancelAction;
    public void Awake()
    {
        gameObject.SetActive(false);
    }

    public void ActivateMenu(string displayText, UnityAction confirmAction, UnityAction cancelAction)
    {
        this.gameObject.SetActive(true);

        // Set the Display Text
        this.displayText.text = displayText;

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
