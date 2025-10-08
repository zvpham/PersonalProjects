using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory.UI
{
    public class UIMissionSelectMenu : MonoBehaviour
    {
        [SerializeField]
        private UIMissionItem missionPrefab;

        [SerializeField]
        private RectTransform contentPanel;

        [SerializeField]
        private MenuInputManager menuInputManager;

        [SerializeField]
        private Button startMissionButton;

        public List<UIMissionItem> ListOfMissionItems = new List<UIMissionItem>();

        public event Action<int> OnProfileClicked;

        public void OnOpenMenu()
        {
            ResetSelection();
        }

        public void InitializeMissionSelectUI(int menuSize)
        {
            for (int i = 0; i < menuSize; i++)
            {
                UIMissionItem UIMission = Instantiate(missionPrefab, Vector3.zero, Quaternion.identity);
                UIMission.transform.SetParent(contentPanel);
                ListOfMissionItems.Add(UIMission);
                UIMission.OnMissionClicked += HandleItemSelection;
            }
        }
        
        public void AddMissionUI()
        {
            UIMissionItem UIMission = Instantiate(missionPrefab, Vector3.zero, Quaternion.identity);
            UIMission.transform.SetParent(contentPanel);
            ListOfMissionItems.Add(UIMission);
            UIMission.OnMissionClicked += HandleItemSelection;
        }

        public void UpdateData(int missionIndex, Sprite missionProviderSpirite, Sprite missionTargetSprite, int dangerRating, string missionName, string reward)
        {
            int placementIndex = missionIndex;
            if (placementIndex >= ListOfMissionItems.Count)
            {
                AddMissionUI();
                placementIndex = ListOfMissionItems.Count - 1;
            }
            ListOfMissionItems[placementIndex].SetData(missionProviderSpirite, missionTargetSprite, dangerRating, missionName, reward);
        }

        private void HandleItemSelection(UIMissionItem missionItemUI)
        {
            int index = ListOfMissionItems.IndexOf(missionItemUI);
            if (index == -1)
            {
                return;
            }
            DeselectAllItems();
            missionItemUI.Select();
            startMissionButton.interactable = true;
            OnProfileClicked?.Invoke(index);
        }

        public UIMissionItem GetMissionItem(int index)
        {
            return ListOfMissionItems[index];
        }

        public void ResetSelection()
        {
            DeselectAllItems();
        }

        private void DeselectAllItems()
        {
            foreach (UIMissionItem item in ListOfMissionItems)
            {
                item.Deselect();
            }
            startMissionButton.interactable = false;
        }


        internal void ResetAllItems()
        {
            if (ListOfMissionItems.Count == 0 || ListOfMissionItems[0] == null)
            {
                ListOfMissionItems = new List<UIMissionItem>();
            }
            foreach (var item in ListOfMissionItems)
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
