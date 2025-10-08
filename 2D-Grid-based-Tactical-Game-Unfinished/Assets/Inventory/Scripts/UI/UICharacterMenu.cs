using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory.UI
{
    public class UICharacterMenu : MonoBehaviour
    {
        [SerializeField]
        private UICharacterProfile CharacterPrefab;

        [SerializeField]
        private RectTransform contentPanel;

        [SerializeField]
        private MouseFollower mouseFollower;

        [SerializeField]
        private MenuInputManager menuInputManager;

        [SerializeField]
        private Button heroButton;

        [SerializeField]
        private Button mercenaryButton;

        public List<UICharacterProfile> listOfUIProfiles = new List<UICharacterProfile>();

        public bool allowCharacterSwap = true;

        private int currentlyDraggedItemIndex = -1;

        public event Action<int> OnProfileClicked,
            OnItemActionRequested,
            OnStartDragging;

        public event Action<int, int> OnSwapItems;
        public event Action<int> OnItemDropped;

        // true = hero button pressed, false = merc button pressed
        public event Action<bool> OnCategoryButtonPressed;

        public void Start()
        {
            menuInputManager.ResetDragUI += ResetDraggedItem;
        }

        public void InitializeCharacterMenuUI(int menuSize)
        {
            for(int i = 0; i  < menuSize; i++)
            {
                AddProfile();
            }
        }

        public void AddProfile()
        {
            UICharacterProfile uiItem = Instantiate(CharacterPrefab, Vector3.zero, Quaternion.identity);
            uiItem.transform.SetParent(contentPanel);
            listOfUIProfiles.Add(uiItem);
            uiItem.OnCharacterClicked += HandleItemSelection;
            uiItem.OnCharacterBeginDrag += HandleBeginDrag;
            uiItem.OnCharacterDroppedOn += HandleSwap;
            uiItem.OnCharacterDroppedOn += HandleItemDroppedOnMissionStart;
            uiItem.OnCharacterEndDrag += HandleEndDrag;
            uiItem.OnRightMouseBtnClick += HandleShowItemActions;
        }

        // This if For character Inventory in Mission Start Menu Onlt
        public void AddEmptySpace()
        {
            listOfUIProfiles.Add(null);
        }

        public void UpdateData(int unitIndex, Sprite unitImage)
        {
            int placementIndex = unitIndex;
            if (placementIndex >= listOfUIProfiles.Count)
            {
                placementIndex = listOfUIProfiles.Count - 1;
            }
            listOfUIProfiles[placementIndex].SetData(unitImage);
        }

        private void HandleShowItemActions(UICharacterProfile inventoryItemUI)
        {
            int index = listOfUIProfiles.IndexOf(inventoryItemUI);
            if (index == -1)
            {
                return;
            }
            OnItemActionRequested?.Invoke(index);
        }

        private void HandleEndDrag(UICharacterProfile inventoryItemUI)
        {
            //ResetDraggedItem();
        }

        private void ResetDraggedItem(EquipSlot ignore = null)
        {
            mouseFollower.Toggle(false);
            currentlyDraggedItemIndex = -1;
        }

        private void HandleBeginDrag(UICharacterProfile inventoryItemUI)
        {
            int index = listOfUIProfiles.IndexOf(inventoryItemUI);
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

        public void SelectProfile(int profileIndex)
        {
            if (profileIndex == -1)
            {
                return;
            }
            listOfUIProfiles[profileIndex].Select();
        }

        private void HandleItemSelection(UICharacterProfile inventoryItemUI)
        {
            int index = listOfUIProfiles.IndexOf(inventoryItemUI);
            if (index == -1)
            {
                return;
            }
            DeselectAllItems();
            inventoryItemUI.Select();
            OnProfileClicked?.Invoke(index);
        }

        private void HandleSwap(UICharacterProfile inventoryItemUI)
        {
            int index = listOfUIProfiles.IndexOf(inventoryItemUI);
            if (index == -1 || !allowCharacterSwap)
            {
                return;
            }
            OnSwapItems?.Invoke(currentlyDraggedItemIndex, index);
            HandleItemSelection(inventoryItemUI);
        }

        private void HandleItemDroppedOnMissionStart(UICharacterProfile inventoryItemUI)
        {
            int index = listOfUIProfiles.IndexOf(inventoryItemUI);
            if (index == -1)
            {
                return;
            }
            OnItemDropped?.Invoke(index);
        }

        public UICharacterProfile GetCharacterProfile(int index)
        {
            return listOfUIProfiles[index];
        }

        public void ResetSelection()
        {
            DeselectAllItems();
        }

        public virtual void DeselectAllItems()
        {
            for(int i = 0; i < listOfUIProfiles.Count; i++)
            {
                if (listOfUIProfiles[i] != null)
                {
                    listOfUIProfiles[i].Deselect();
                }
            }
        }

        public void ClearCharacterProfiles()
        {
            for(int i = 0; i < listOfUIProfiles.Count; i++)
            {
                if (listOfUIProfiles[i] != null)
                {
                    Destroy(listOfUIProfiles[i].gameObject);
                }
            }
            listOfUIProfiles = new List<UICharacterProfile>();
        }

        public void OnMenuOpened()
        {
            heroButton.interactable = false;
            mercenaryButton.interactable = true;
        }

        public void OnHeroesButtonPresed()
        {
            heroButton.interactable = false;
            mercenaryButton.interactable = true;
            ClearCharacterProfiles();
            OnCategoryButtonPressed?.Invoke(true);
        }

        public void OnMercenariesButtonPressed()
        {
            heroButton.interactable = true;
            mercenaryButton.interactable = false;
            ClearCharacterProfiles();
            OnCategoryButtonPressed?.Invoke(false);

        }

        public int GetCurrentlyDraggedIndex()
        {
            return currentlyDraggedItemIndex;
        }

        internal void ResetAllItems()
        {
            if (listOfUIProfiles.Count == 0 || listOfUIProfiles[0] == null)
            {
                listOfUIProfiles = new List<UICharacterProfile>();
            }
            foreach (var item in listOfUIProfiles)
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
