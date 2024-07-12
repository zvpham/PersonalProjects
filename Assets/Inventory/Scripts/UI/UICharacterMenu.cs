using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        public List<UICharacterProfile> listOfUIProfiles = new List<UICharacterProfile>();

        private int currentlyDraggedItemIndex = -1;

        public event Action<int> OnProfileClicked,
            OnItemActionRequested,
            OnStartDragging;

        public event Action<int, int> OnSwapItems;

        public void Start()
        {
            menuInputManager.OnMouseUp += ResetDraggedItem;
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

        private void HandleSwap(UICharacterProfile inventoryItemUI)
        {
            int index = listOfUIProfiles.IndexOf(inventoryItemUI);
            if (index == -1)
            {
                return;
            }
            OnSwapItems?.Invoke(currentlyDraggedItemIndex, index);
            HandleItemSelection(inventoryItemUI);
        }

        private void ResetDraggedItem(EquipSlot ignore =  null)
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

        public UICharacterProfile GetCharacterProfile(int index)
        {
            return listOfUIProfiles[index];
        }

        public void ResetSelection()
        {
            DeselectAllItems();
        }

        private void DeselectAllItems()
        {
            for(int i =0; i < listOfUIProfiles.Count; i++)
            {
                if (listOfUIProfiles[i] != null)
                {
                    listOfUIProfiles[i].Deselect();
                }
            }
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
