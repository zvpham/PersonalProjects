using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace Inventory.UI
{
    public class UIMissionItem : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        public Image missionProviderImage;

        [SerializeField]
        public Image missionEnemyImage;

        [SerializeField]
        private Image borderImage;

        [SerializeField]
        private TMP_Text missionNameTxt;

        [SerializeField]
        private TMP_Text rewardTxt;

        public GameObject dangerRatingHolder;

        [SerializeField]
        private GameObject fullSkullPrefab;

        [SerializeField]
        private GameObject halfSkullPrefab;

        [SerializeField]
        private GameObject noSkullPrefab;

        public event Action<UIMissionItem> OnMissionClicked, OnRightMouseBtnClick;

        public void Awake()
        {
            ResetData();
            Deselect();
        }

        public void ResetData()
        {
            missionProviderImage.gameObject.SetActive(false);
            missionEnemyImage.gameObject.SetActive(false);
        }

        public void Deselect()
        {
            borderImage.enabled = false;
        }

        public void Select()
        {
            borderImage.enabled = true;
        }

        public void SetData(Sprite missionProviderSprite, Sprite missiontargetSprite, int dangerRating, string missionName, string reward)
        {
            if(dangerRating <  0 || dangerRating > 100)
            {
                Debug.LogWarning("Dager rating is out of bounds: " + dangerRating);
            }

            missionProviderImage.gameObject.SetActive(true);
            missionProviderImage.sprite = missionProviderSprite;

            missionEnemyImage.gameObject.SetActive(true);
            missionEnemyImage.sprite = missiontargetSprite;

            missionNameTxt.text = missionName;
            rewardTxt.text = reward;

            int currentDangerRating = dangerRating;

            for(int i = 0; i < 5; i++)
            {
                GameObject newDangerObject = null;
                if(currentDangerRating >= 2)
                {
                    currentDangerRating -= 2;
                    newDangerObject = Instantiate(fullSkullPrefab);
                }
                else if(currentDangerRating >= 1)
                {
                    currentDangerRating -= 1;
                    newDangerObject = Instantiate(halfSkullPrefab);
                }
                else
                {
                    newDangerObject = Instantiate(noSkullPrefab);
                }
                newDangerObject.transform.SetParent(dangerRatingHolder.transform);
            }
        }

        public void OnPointerClick(PointerEventData pointerData)
        {
            if (pointerData.button == PointerEventData.InputButton.Right)
            {
                OnRightMouseBtnClick?.Invoke(this);
            }
            else
            {
                OnMissionClicked?.Invoke(this);
            }
        }
    }
}

