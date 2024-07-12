using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEditorInternal.Profiling.Memory.Experimental;
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
        private MouseFollower mouseFollower;

        [SerializeField]
        private MenuInputManager menuInputManager;

        public List<UIInventoryItem> listOfUIItems = new List<UIInventoryItem>();

        public List<EquipSlot> equipSlots = new List<EquipSlot>();

        private int currentlyDraggedItemIndex = -1;

        public EquipSlot currentlyDraggingEquipSlot = null;

        public event Action<int> OnDescriptionRequested,
            OnItemActionRequested,
            OnStartDragging;

        public event Action<int, int> OnSwapItems;

        public event Action<int, EquipSlot> OnEquipItem;
        public event Action<EquipableItemSO, EquipSlot> OnProfileClicked;

        public event Action<EquipSlot> OnUnequipItem;

        public void Awake()
        {
            Hide();
            mouseFollower.Toggle(false);
        }

        public void Start()
        {
            menuInputManager.OnMouseUp += ResetDraggedItem;
        }

        public void AddInventoryUIItem()
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

        public void InitializeInventoryUI()
        {
            for (int i = 0; i < equipSlots.Count; i++)
            {
                equipSlots[i].OnItemDroppedOn -= HandleItemEquip;
                equipSlots[i].OnItemDroppedOn += HandleItemEquip;

                equipSlots[i].OnItemBeginDrag -= HandleBeginDragEquip;
                equipSlots[i].OnItemBeginDrag += HandleBeginDragEquip;
            }
        }

        public void UpdateData(int itemIndex, Sprite itemImage, int itemQuantity, string name, string attributeOne, string attributeTwo, string mainCategoryOne, string mainTextOne, string mainCategoryTwo, string mainTextTwo, string mainCategoryThree, string mainTextThree)
        {
            int placementIndex = itemIndex;
            if (placementIndex >= listOfUIItems.Count)
            {
                AddInventoryUIItem();
                placementIndex = listOfUIItems.Count - 1;
            }
            listOfUIItems[placementIndex].SetData(itemImage, itemQuantity, name, attributeOne, attributeTwo, mainCategoryOne, mainTextOne, mainCategoryTwo, mainTextTwo, mainCategoryThree, mainTextThree);
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

        private void UnequipItemUI()
        {
            OnUnequipItem?.Invoke(currentlyDraggingEquipSlot);
        }

        private void HandleEndDragEquip(EquipSlot equipSlot)
        {
            if(currentlyDraggingEquipSlot != equipSlot)
            {
                UnequipItemUI();
            }
            currentlyDraggingEquipSlot = null;
            menuInputManager.OnMouseUp -= HandleEndDragEquip;
        }

        private void HandleEndDrag(UIInventoryItem inventoryItemUI)
        {
            //ResetDraggedItem();
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

        public void ItemEquip(EquipableItemSO item, EquipSlot equipSlot)
        {
            OnProfileClicked?.Invoke(item, equipSlot);
        } 

        private void HandleItemEquip(EquipSlot equipSlot)
        {
            if(currentlyDraggedItemIndex == -1 || mouseFollower.item == null)
            {
                return;
            }
            OnEquipItem?.Invoke(currentlyDraggedItemIndex, equipSlot);
        }

        public void ConfirmItemEquip(EquipSlot equipSlot)
        {
            equipSlot.contentImage.gameObject.SetActive(true);
            equipSlot.contentImage.sprite = listOfUIItems[currentlyDraggedItemIndex].itemImage.sprite;
        }
        public void ConfirmItemEquip(EquipableItemSO item, EquipSlot equipSlot)
        {
            equipSlot.contentImage.gameObject.SetActive(true);
            equipSlot.contentImage.sprite = item.itemImage;
        }

        private void ResetDraggedItem(EquipSlot IgnoreThis = null)
        {
            mouseFollower.Toggle(false);
            currentlyDraggedItemIndex = -1;
        }

        private void HandleBeginDragEquip(EquipSlot equipSlot)
        {
            CreateDraggedItem(equipSlot.contentImage.sprite);
            currentlyDraggingEquipSlot = equipSlot;
            menuInputManager.OnMouseUp += HandleEndDragEquip;
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

        public void CreateDraggedItem(Sprite sprite)
        {
            mouseFollower.Toggle(true);
            mouseFollower.SetData(sprite);
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
            ResetSelection();
        }

        public void ResetSelection()
        {
            DeselectAllItems();
        }

        private void DeselectAllItems()
        {
            foreach (UIInventoryItem item in listOfUIItems)
            {
                item.Deselect();
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            ResetDraggedItem();
        }

        internal void ResetAllItems()
        {
            if (listOfUIItems.Count == 0 || listOfUIItems[0] == null)
            {
                listOfUIItems = new List<UIInventoryItem>();
                InitializeInventoryUI();
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

