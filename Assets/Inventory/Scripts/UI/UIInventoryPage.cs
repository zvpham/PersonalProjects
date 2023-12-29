using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;

namespace Inventory.UI
{
    public class UIInventoryPage : MonoBehaviour
    {
        [SerializeField]
        private UIInventoryItem itemPrefab;

        [SerializeField]
        private RectTransform contentPanel;

        [SerializeField]
        private UIInventoryDescription itemDescription;

        [SerializeField]
        private MouseFollower mouseFollower;

        public List<UIInventoryItem> listOfUIItems = new List<UIInventoryItem>();

        public List<SoulSlot> soulSlots = new List<SoulSlot>();

        private int currentlyDraggedItemIndex = -1;

        public static UIInventoryPage Instance;

        public event Action<int> OnDescriptionRequested,
            OnItemActionRequested,
            OnStartDragging;

        public event Action<int, int> OnSwapItems;

        public event Action<int, SoulSlot> OnEquipSoul;

        [SerializeField]
        private ItemActionPanel actionPanel;
        public void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Found more than ONe InventoryUiPage in scene in the Scence");
                Destroy(this.gameObject);
                return;
            }
            Instance = this;

            Hide();
            mouseFollower.Toggle(false);
            itemDescription.ResetDescription();
        }

        public void InitializeInventoryUI(int inventorySize)
        {
            for (int i = 0; i < soulSlots.Count; i++)
            {
                soulSlots[i].OnItemDroppedOn += HandleSoulEquip;
            }
                for (int i = 0; i < inventorySize; i++)
            {
                UIInventoryItem uiItem = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
                uiItem.transform.SetParent(contentPanel);
                listOfUIItems.Add(uiItem);
                uiItem.OnItemClicked += HandleItemSelection;
                uiItem.OnItemBeginDrag += HandleBeginDrag;
                uiItem.OnItemDroppedOn += HandleSwap;
                uiItem.OnItemEndDrag += HandleEndDrag;
                uiItem.OnRightMouseBtnClick += HandleShowItemActions;
            }
        }

        internal void UpdateDescription(int itemIndex, Sprite itemImage, string itemName, string description)
        {
            itemDescription.SetDescription(itemImage, itemName, description);
            DeselectAllItems();
            listOfUIItems[itemIndex].Select();
        }

        public void UpdateData(int itemIndex, Sprite itemImage, int itemQuantity)
        {
            if (listOfUIItems.Count > itemIndex)
            {
                listOfUIItems[itemIndex].SetData(itemImage, itemQuantity);
            }
        }
        private void HandleShowItemActions(UIInventoryItem inventoryItemUI)
        {
            int index = listOfUIItems.IndexOf(inventoryItemUI);
            if (index == -1)
            {
                return;
            }
            OnItemActionRequested?.Invoke(index);
        }

        private void HandleEndDrag(UIInventoryItem inventoryItemUI)
        {
            ResetDraggedItem();
        }

        private void HandleSwap(UIInventoryItem inventoryItemUI)
        {
            int index = listOfUIItems.IndexOf(inventoryItemUI);
            if (index == -1)
            {
                return;
            }
            OnSwapItems?.Invoke(currentlyDraggedItemIndex, index);
            HandleItemSelection(inventoryItemUI);
        }

        private void HandleSoulEquip(SoulSlot soulSlot)
        {
            if(currentlyDraggedItemIndex == -1 || mouseFollower.item == null)
            {
                return;
            }

            soulSlot.contentImage.gameObject.SetActive(true);
            soulSlot.contentImage.sprite = listOfUIItems[currentlyDraggedItemIndex].itemImage.sprite;
            OnEquipSoul?.Invoke(currentlyDraggedItemIndex, soulSlot);
        }

        private void ResetDraggedItem()
        {
            mouseFollower.Toggle(false);
            currentlyDraggedItemIndex = -1;
        }

        private void HandleBeginDrag(UIInventoryItem inventoryItemUI)
        {
            int index = listOfUIItems.IndexOf(inventoryItemUI);
            if (index == -1)
            {
                return;
            }
            currentlyDraggedItemIndex = index;
            HandleItemSelection(inventoryItemUI);
            OnStartDragging?.Invoke(index);
        }

        public void CreateDraggedItem(Sprite sprite, int quantity)
        {
            mouseFollower.Toggle(true);
            mouseFollower.SetData(sprite, quantity);
        }

        private void HandleItemSelection(UIInventoryItem inventoryItemUI)
        {
            int index = listOfUIItems.IndexOf(inventoryItemUI);
            if (index == -1)
            {
                return;
            }
            OnDescriptionRequested?.Invoke(index);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            itemDescription.ResetDescription();
            ResetSelection();
        }

        public void ResetSelection()
        {
            itemDescription.ResetDescription();
            DeselectAllItems();
        }

        public void AddAction(string actionName, UnityAction performAction)
        {
            actionPanel.AddButton(actionName, performAction);
        }

        public void ShowItemAction(int itemIndex)
        {
            actionPanel.Toggle(true, listOfUIItems[itemIndex]);
            actionPanel.transform.position = listOfUIItems[itemIndex].transform.position;
        }

        private void DeselectAllItems()
        {
            foreach (UIInventoryItem item in listOfUIItems)
            {
                item.Deselect();
            }
            actionPanel.Toggle(false);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            ResetDraggedItem();
            actionPanel.Toggle(false);
        }

        internal void ResetAllItems()
        {
            if (listOfUIItems[0] == null)
            {
                int inventorySize = listOfUIItems.Count;
                listOfUIItems = new List<UIInventoryItem>();
                InitializeInventoryUI(inventorySize);
            }
            foreach (var item in listOfUIItems)
            {   
                item.ResetData();
                item.Deselect();
            }
        }

        internal void AddAction(string v, Func<object> value)
        {
            throw new NotImplementedException();
        }
    }
}

