using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.UIElements;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityEditor.Progress;

namespace Inventory.UI
{
    public class UIInventoryPage : MonoBehaviour
    {
        [SerializeField]
        private UIInventoryItem itemPrefab;

        public GameObject inventoryItemUIHolder;
        public List<UIInventoryItem> unusedItemUIs;

        [SerializeField]
        private RectTransform contentPanel;

        [SerializeField]
        private MouseFollower mouseFollower;

        [SerializeField]
        private MenuInputManager menuInputManager;

        public bool isMissionStart = false;

        public List<TestCharacterTypeUI> mainitemFilters;
        public List<GameObject> SubItemFilterPanels = new List<GameObject>();
        public List<TestCharacterTypeUI> weaponSubFilters;
        public List<TestCharacterTypeUI> armorSubFilters;
        public List<TestCharacterTypeUI> AccessoriesSubFilters;
        public List<TestCharacterTypeUI> SkillBookSubFilters;
        public int mainItemFilterIndex;

        public List<UIInventoryItem> listOfUIItems = new List<UIInventoryItem>();

        public List<EquipSlot> equipSlots = new List<EquipSlot>();

        public EquipSlot backUpMainHand;
        public EquipSlot backUpOffHand;

        public Image currentWeightPanel1;
        public TMP_Text currentWeightTXT1;
        public TMP_Text remainingWeightTXT1;
        public TMP_Text lowWeightTXT1;
        public TMP_Text mediumWieghtTXT1;
        public TMP_Text highWeightTXT1;

        public Image currentWeightPanel2;
        public TMP_Text currentWeightTXT2;
        public TMP_Text remainingWeightTXT2;
        public TMP_Text lowWeightTXT2;
        public TMP_Text mediumWieghtTXT2;
        public TMP_Text highWeightTXT2;

        public Image actionPointPrefab;
        public Image actionPointPanel;

        public TMP_Text armorValue;
        public TMP_Text healthValue;
        public TMP_Text strengthValue;
        public TMP_Text dexterityValue;
        public TMP_Text powerPointGenerationValue;
        public TMP_Text movementValue;
        public TMP_Text VisionValue;

        private int currentlyDraggedItemIndex = -1;

        public EquipSlot currentlyDraggingEquipSlot = null;

        public event Action<int> OnDescriptionRequested,
            OnItemActionRequested,
            OnStartDragging,
            OnMainFilterChanged;

        public event Action<int, int> OnSwapItems, OnFiltersChanged;

        public event Action<int, EquipSlot> OnEquipItem;
        public event Action<EquipableItemSO, EquipSlot> OnProfileClicked;

        public event Action<EquipSlot> OnUnequipItem;
        public event Action<EquipSlot, EquipSlot> OnSwapEquipSlot;

        public event Action<bool> OnSwapHands;

        public void Awake()
        {
            Hide();
            mouseFollower.Toggle(false);
        }

        public void Start()
        {
            menuInputManager.ResetDragUI += ResetDraggedItem;
        }

        public void OnOpenMenu()
        {
            HandleMainItemTypeClicked(mainitemFilters[0]);
        }

        public void AddInventoryUIItem()
        {
            listOfUIItems.Add(UseUIItem());
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

            for(int i = 0; i < mainitemFilters.Count; i++)
            {
                mainitemFilters[i].OnCharacterTypeClicked += HandleMainItemTypeClicked;
            }

            for(int i = 0; i < weaponSubFilters.Count; i++)
            {
                weaponSubFilters[i].OnCharacterTypeClicked += HandleSubItemTypeClicked;
            }

            for (int i = 0; i < armorSubFilters.Count; i++)
            {
                armorSubFilters[i].OnCharacterTypeClicked += HandleSubItemTypeClicked;
            }

            for (int i = 0; i < AccessoriesSubFilters.Count; i++)
            {
                AccessoriesSubFilters[i].OnCharacterTypeClicked += HandleSubItemTypeClicked;
            }

            for (int i = 0; i < SkillBookSubFilters.Count; i++)
            {
                SkillBookSubFilters[i].OnCharacterTypeClicked += HandleSubItemTypeClicked;
            }

            backUpMainHand.OnItemDroppedOn -= HandleItemEquip;
            backUpMainHand.OnItemDroppedOn += HandleItemEquip;

            backUpMainHand.OnItemBeginDrag -= HandleBeginDragEquip;
            backUpMainHand.OnItemBeginDrag += HandleBeginDragEquip;

            backUpOffHand.OnItemDroppedOn -= HandleItemEquip;
            backUpOffHand.OnItemDroppedOn += HandleItemEquip;

            backUpOffHand.OnItemBeginDrag -= HandleBeginDragEquip;
            backUpOffHand.OnItemBeginDrag += HandleBeginDragEquip;
        }

        public void UpdateData(int itemIndex, Sprite itemImage, int itemQuantity, string name, string attributeOne, string attributeTwo,
            string mainCategoryOne, string mainTextOne, string mainCategoryTwo, string mainTextTwo, string mainCategoryThree, string mainTextThree,
            string ammoExtension)
        {
            int placementIndex = itemIndex;

            if (placementIndex >= listOfUIItems.Count)
            {
                AddInventoryUIItem();
                placementIndex = listOfUIItems.Count - 1;
            }

            listOfUIItems[placementIndex].SetData(itemImage, itemQuantity, name, attributeOne, attributeTwo, mainCategoryOne,
                mainTextOne, mainCategoryTwo, mainTextTwo, mainCategoryThree, mainTextThree, ammoExtension);
        }

        public void UpdateWeight(int currentWeight, int lowWeight, int mediumWeight, int highWeight)
        {
            lowWeightTXT1.text = lowWeight.ToString();
            mediumWieghtTXT1.text = mediumWeight.ToString();
            highWeightTXT1.text = highWeight.ToString();
            int weightRemaining;
            if(currentWeight > highWeight)
            {
                currentWeightPanel1.color = Color.red;
                currentWeightTXT1.text =  currentWeight.ToString() + "/" + highWeight.ToString();
                weightRemaining = highWeight - currentWeight;
                remainingWeightTXT1.text = weightRemaining.ToString() + " weight remaining";
            }
            else if(currentWeight > mediumWeight)
            {
                currentWeightPanel1.color = new Color(1f, 0.647f, 0); // Orange
                currentWeightTXT1.text = currentWeight.ToString() + "/" + highWeight.ToString();
                weightRemaining = highWeight - currentWeight;
                remainingWeightTXT1.text = weightRemaining.ToString() + " weight remaining";
            }
            else if(currentWeight > lowWeight)
            {
                currentWeightPanel1.color = Color.yellow;
                currentWeightTXT1.text = currentWeight.ToString() + "/" + mediumWeight.ToString();
                weightRemaining = mediumWeight - currentWeight;
                remainingWeightTXT1.text = weightRemaining.ToString() + " weight remaining";
            }
            else
            {
                currentWeightPanel1.color = Color.green;
                currentWeightTXT1.text = currentWeight.ToString() + "/" + lowWeight.ToString();
                weightRemaining = lowWeight - currentWeight;
                remainingWeightTXT1.text = weightRemaining.ToString() + " weight remaining";
            }
        }

        public void UpdateWeightSecondary(int currentWeight, int lowWeight, int mediumWeight, int highWeight)
        {
            lowWeightTXT2.text = lowWeight.ToString();
            mediumWieghtTXT2.text = mediumWeight.ToString();
            highWeightTXT2.text = highWeight.ToString();
            int weightRemaining = -1;
            if (currentWeight > highWeight)
            {
                currentWeightPanel2.color = Color.red;
                currentWeightTXT2.text = currentWeight.ToString() + "/" + highWeight.ToString();
                weightRemaining = highWeight - currentWeight;
                remainingWeightTXT2.text = weightRemaining.ToString() + " weight remaining";
            }
            else if (currentWeight > mediumWeight)
            {
                currentWeightPanel2.color = new Color(1f, 0.647f, 0); // Orange
                currentWeightTXT2.text = currentWeight.ToString() + "/" + highWeight.ToString();
                weightRemaining = highWeight - currentWeight;
                remainingWeightTXT2.text = weightRemaining.ToString() + " weight remaining";
            }
            else if (currentWeight > lowWeight)
            {
                currentWeightPanel2.color = Color.yellow;
                currentWeightTXT2.text = currentWeight.ToString() + "/" + mediumWeight.ToString();
                weightRemaining = mediumWeight - currentWeight;
                remainingWeightTXT2.text = weightRemaining.ToString() + " weight remaining";
            }
            else
            {

                currentWeightPanel2.color = Color.green;
                currentWeightTXT2.text = currentWeight.ToString() + "/" + lowWeight.ToString();
                weightRemaining = lowWeight - currentWeight;
                remainingWeightTXT2.text = weightRemaining.ToString() + " weight remaining";
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

        private void UnequipItemUI()
        {
            OnUnequipItem?.Invoke(currentlyDraggingEquipSlot);
        }

        private void SwapItemUI(EquipSlot newEquipSlot)
        {
            OnSwapEquipSlot?.Invoke(currentlyDraggingEquipSlot, newEquipSlot);
        }

        public void SwapMainHand()
        {
            OnSwapHands?.Invoke(true);
        }

        public void SwapOffHand()
        {
            OnSwapHands?.Invoke(false);
        }

        private void HandleEndDragEquip(EquipSlot equipSlot)
        {
            EquipableItemSO currentItem = currentlyDraggingEquipSlot.currentItem;
            if(currentlyDraggingEquipSlot != equipSlot)
            {
                if(equipSlot != null)
                {
                    if ((equipSlot.equipType == EquipType.Accessory1 || equipSlot.equipType == EquipType.Accessory2 ||
                        equipSlot.equipType == EquipType.Accessory3 || equipSlot.equipType == EquipType.Accessory4) &&
                        (currentlyDraggingEquipSlot.equipType == EquipType.Accessory1 || currentlyDraggingEquipSlot.equipType == EquipType.Accessory2 ||
                        currentlyDraggingEquipSlot.equipType == EquipType.Accessory3 || currentlyDraggingEquipSlot.equipType == EquipType.Accessory4)
                        || equipSlot.equipType == currentlyDraggingEquipSlot.equipType || (currentItem.equipType == EquipType.BothHands) &&
                        equipSlot.equipType == EquipType.MainHand || equipSlot.equipType == EquipType.OffHand)
                    {
                        SwapItemUI(equipSlot);
                    }
                    else
                    {
                        UnequipItemUI();
                    }
                }
                else
                {
                    UnequipItemUI();
                }
            }
            currentlyDraggingEquipSlot = null;
            menuInputManager.ResetDragUI -= HandleEndDragEquip;
        }

        private void HandleEndDrag(UIInventoryItem inventoryItemUI)
        {
            //ResetDraggedItem();
        }

        private void HandleSwap(UIInventoryItem inventoryItemUI)
        {
            /*
            int index = listOfUIItems.IndexOf(inventoryItemUI);
            if (index == -1)
            {
                return;
            }
            OnSwapItems?.Invoke(currentlyDraggedItemIndex, index);
            HandleItemSelection(inventoryItemUI);
            */
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
            if(item == null)
            {
                equipSlot.contentImage.sprite = null;
            }
            else
            {
                equipSlot.contentImage.sprite = item.itemImage;
            }
        }

        private void ResetDraggedItem(EquipSlot IgnoreThis = null)
        {
            mouseFollower.Toggle(false);
            currentlyDraggedItemIndex = -1;
        }

        private void HandleBeginDragEquip(EquipSlot equipSlot)
        {
            if(isMissionStart)
            {
                return;
            }
            CreateDraggedItem(equipSlot.contentImage.sprite);
            currentlyDraggingEquipSlot = equipSlot;
            menuInputManager.ResetDragUI += HandleEndDragEquip;
        }

        private void HandleBeginDrag(UIInventoryItem inventoryItemUI)
        {
            int index = listOfUIItems.IndexOf(inventoryItemUI);
            if (index == -1 || isMissionStart)
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

        public void UpdateUnitInfo(Unit unit)
        {
            List<GameObject> actionPointChildren = new List<GameObject>();
            for (int i = 0; i < actionPointPanel.transform.childCount; i++)
            {
                actionPointChildren.Add(actionPointPanel.transform.GetChild(i).gameObject);
            }

            for (int i = 0; i < actionPointChildren.Count; i++)
            {
                Destroy(actionPointChildren[i]);
            }

            for (int i = 0; i < unit.maxActionsPoints; i++)
            {
                Image newActionPoint = Instantiate(actionPointPrefab, actionPointPanel.gameObject.transform);
                newActionPoint.transform.position = newActionPoint.transform.position +
                    new Vector3(actionPointPrefab.rectTransform.rect.width * i, 0, 0);
            }

            armorValue.text = unit.maxArmor + "/" + unit.maxArmor;
            healthValue.text =  unit.maxHealth + "/" + unit.maxHealth;
            strengthValue.text = UpdateCharacterStat(unit.strength);
            dexterityValue.text = UpdateCharacterStat(unit.dexterity);
            movementValue.text = UpdateCharacterStat(unit.moveSpeed);
        }

        public string UpdateCharacterStat(int value)
        {
            if(value > 0)
            {
                return " + " + value;
            }
            else if(value == 0)
            {
                return " 0";
            }
            else
            {
                return " - " + Mathf.Abs(value); 
            }
        }

        internal void ResetAllItems(Dictionary<int, InventoryItem> inventoryState)
        {
            List<UIInventoryItem> items = listOfUIItems;
            for(int i = 0; i < items.Count; i++)
            {
                DeactivateUIItem(items[i]);
            }
            /*
            if (listOfUIItems.Count == 0 || listOfUIItems[0] == null)
            {
                listOfUIItems = new List<UIInventoryItem>();
                InitializeInventoryUI();
            }

            int itemAmount = 0;
            foreach(var inventoryItem in inventoryState) 
            {
                if (!inventoryItem.Value.isEmpty)
                {
                    itemAmount += 1;
                }
            }

            for(int i = itemAmount; i <  listOfUIItems.Count; i++)
            {
                Destroy(listOfUIItems[0].gameObject);
                listOfUIItems.RemoveAt(0);
            }

            foreach (var item in listOfUIItems)
            {   
                item.ResetData();
                item.Deselect();
            }
            */
        }

        internal void ResetAllItems()
        {
            List<UIInventoryItem> items = listOfUIItems;
            for (int i = 0; i < items.Count; i++)
            {
                DeactivateUIItem(items[i]);
            }
        }

        public void HandleMainItemTypeClicked(TestCharacterTypeUI mainFilterClicked)
        {
            ResetAllItems();
            int mainFilterIndex = mainitemFilters.IndexOf(mainFilterClicked);

            for(int i = 0; i < mainitemFilters.Count; i++)
            {
                mainitemFilters[i].Deselect();
            }

            mainitemFilters[mainFilterIndex].Select();
            mainItemFilterIndex = mainFilterIndex;
            MainItemFilterChanged(mainFilterIndex);
            OnMainFilterChanged?.Invoke(mainFilterIndex);
            OnFiltersChanged.Invoke(mainFilterIndex, 0);
        }

        public void MainItemFilterChanged(int  mainItemFilterIndex)
        {
            for (int i = 0; i < SubItemFilterPanels.Count; i++)
            {
                SubItemFilterPanels[i].SetActive(false);
            }

            switch (mainItemFilterIndex)
            {
                case 0:
                    SubItemFilterPanels[0].SetActive(true);
                    for(int i = 0; i < weaponSubFilters.Count; i++)
                    {
                        weaponSubFilters[i].Deselect();
                    }
                    weaponSubFilters[0].Select();
                    break;
                case 1:
                    SubItemFilterPanels[1].SetActive(true);
                    for (int i = 0; i < armorSubFilters.Count; i++)
                    {
                        armorSubFilters[i].Deselect();
                    }
                    armorSubFilters[0].Select();
                    break;
                case 2:
                    SubItemFilterPanels[2].SetActive(true);
                    for (int i = 0; i < AccessoriesSubFilters.Count; i++)
                    {
                        AccessoriesSubFilters[i].Deselect();
                    }
                    AccessoriesSubFilters[0].Select();
                    break;
                case 3:
                    SubItemFilterPanels[3].SetActive(true);
                    for (int i = 0; i < SkillBookSubFilters.Count; i++)
                    {
                        SkillBookSubFilters[i].Deselect();
                    }
                    SkillBookSubFilters[0].Select();
                    break;
            }

        }

        public void HandleSubItemTypeClicked(TestCharacterTypeUI subFilterClicked)
        {
            ResetAllItems();
            int subfilterIndex = -1;
            switch (mainItemFilterIndex)
            {
                case 0:
                    subfilterIndex = weaponSubFilters.IndexOf(subFilterClicked);
                    for (int i = 0; i < weaponSubFilters.Count; i++)
                    {
                        weaponSubFilters[i].Deselect();
                    }
                    weaponSubFilters[subfilterIndex].Select();
                    break;
                case 1:
                    subfilterIndex = armorSubFilters.IndexOf(subFilterClicked);
                    for (int i = 0; i < armorSubFilters.Count; i++)
                    {
                        armorSubFilters[i].Deselect();
                    }
                    armorSubFilters[subfilterIndex].Select();
                    break;
                case 2:
                    subfilterIndex = AccessoriesSubFilters.IndexOf(subFilterClicked);
                    for (int i = 0; i < AccessoriesSubFilters.Count; i++)
                    {
                        AccessoriesSubFilters[i].Deselect();
                    }
                    AccessoriesSubFilters[subfilterIndex].Select();
                    break;
                case 3:
                    subfilterIndex = SkillBookSubFilters.IndexOf(subFilterClicked);
                    for (int i = 0; i < SkillBookSubFilters.Count; i++)
                    {
                        SkillBookSubFilters[i].Deselect();
                    }
                    SkillBookSubFilters[subfilterIndex].Select();
                    break;
            }
            OnFiltersChanged?.Invoke(mainItemFilterIndex, subfilterIndex);
        }

        public UIInventoryItem UseUIItem()
        {
            UIInventoryItem item;
            if (unusedItemUIs.Count == 0)
            {
                UIInventoryItem uiItem = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
                uiItem.transform.SetParent(inventoryItemUIHolder.transform);
                unusedItemUIs.Add(uiItem);
            }

            item = unusedItemUIs[0];
            item.transform.SetParent(contentPanel.transform);
            item.OnItemClicked += HandleItemSelection;
            item.OnItemBeginDrag += HandleBeginDrag;
            item.OnItemDroppedOn += HandleSwap;
            item.OnItemEndDrag += HandleEndDrag;
            item.OnRightMouseBtnClick += HandleShowItemActions;
            unusedItemUIs.RemoveAt(0);
            return item;
        }

        public void DeactivateUIItem(UIInventoryItem item)
        {
            item.OnItemClicked -= HandleItemSelection;
            item.OnItemBeginDrag -= HandleBeginDrag;
            item.OnItemDroppedOn -= HandleSwap;
            item.OnItemEndDrag -= HandleEndDrag;
            item.OnRightMouseBtnClick -= HandleShowItemActions;
            item.transform.SetParent(inventoryItemUIHolder.transform);
            item.ResetData();
            item.Deselect();
            listOfUIItems.Remove(item);
            unusedItemUIs.Add(item);
        }



        internal void AddAction(string v, Func<object> value)
        {
            throw new NotImplementedException();
        }
    }
}

