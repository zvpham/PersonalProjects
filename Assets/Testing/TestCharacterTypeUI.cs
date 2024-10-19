using Inventory.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TestCharacterTypeUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private Image borderImage;
    public bool selected;

    public event Action<TestCharacterTypeUI> OnCharacterTypeClicked;

    public void Deselect()
    {
        borderImage.enabled = false;
        selected = false;
    }

    public void Select()
    {
        borderImage.enabled = true;
        selected = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnCharacterTypeClicked?.Invoke(this);
    }
}
