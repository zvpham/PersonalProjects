using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inventory.UI;

public class InventoryController : MonoBehaviour
{

    [SerializeField]
    private UIInventoryPage inventoryUI;

    public int inventorySize = 10;

    private InputManager inputManager;
    void Start()
    {
        inventoryUI.InitializeInventoryUI(inventorySize);
        inputManager = InputManager.instance;
    }
    public void Update()
    {
        if (inputManager.GetKeyDown(ActionName.InventoryMenu))
        {
            if (inventoryUI.isActiveAndEnabled == false)
            {
                inventoryUI.Show();
            }
            else
            {
                inventoryUI.Hide();
            }
        }
    }
}
