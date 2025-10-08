using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TestInventoryTypeUI : MonoBehaviour
{
    [SerializeField]
    private Image borderImage;
    public bool selected;

    public event Action<TestInventoryTypeUI> OnInventoryTypeClicked;

    public void Start()
    {
        Deselect();
    }

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
        OnInventoryTypeClicked?.Invoke(this);
    }
}
