using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

namespace Inventory.UI
{
    public class UIInventoryItem : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDropHandler, IDragHandler
    {
        [SerializeField]
        public Image itemImage;

        [SerializeField]
        private Image borderImage;

        [SerializeField]
        private TMP_Text quantityTxt;

        [SerializeField]
        private TMP_Text itemNameTxt;

        [SerializeField]
        private TMP_Text attributeOne;
        [SerializeField]
        private TMP_Text attributeTwo;

        [SerializeField]
        private TMP_Text mainStatOneCategory;
        [SerializeField]
        private TMP_Text mainStatOneTxt;

        [SerializeField]
        private TMP_Text mainStatTwoCategory;
        [SerializeField]
        private TMP_Text mainStatTwoTxt;

        [SerializeField]
        private TMP_Text mainStatThreeCategory;
        [SerializeField]
        private TMP_Text mainStatThreeTxt;


        public event Action<UIInventoryItem> OnItemClicked, OnItemDroppedOn, OnItemBeginDrag, OnItemEndDrag, OnRightMouseBtnClick;

        private bool empty = true;

        public void Awake()
        {
            ResetData();
            Deselect();
        }

        public void ResetData()
        {
            //Debug.Log(this);
            //Debug.Log(itemImage);
            itemImage.gameObject.SetActive(false);
            empty = false;
        }

        public void Deselect()
        {
            borderImage.enabled = false;
        }

        public void Select()
        {
            borderImage.enabled = true;
        }

        public void SetData(Sprite sprite, int quantity, string name, string attributeOne, string attributeTwo, string mainCategoryOne, string mainTextOne, string mainCategoryTwo, string mainTextTwo, string mainCategoryThree, string mainTextThree)
        {
            itemImage.gameObject.SetActive(true);   
            itemImage.sprite = sprite;
            quantityTxt.text = quantity + "";
            itemNameTxt.text = name;
            this.attributeOne.text = attributeOne;
            this.attributeTwo.text = attributeTwo;
            if(mainCategoryOne == "")
            {
                mainStatOneCategory.gameObject.SetActive(false);
                mainStatOneTxt.gameObject.SetActive(false);
            }
            mainStatOneCategory.text = mainCategoryOne;
            mainStatOneTxt.text = mainTextOne;

            if (mainCategoryTwo == "")
            {
                mainStatTwoCategory.gameObject.SetActive(false);
                mainStatTwoTxt.gameObject.SetActive(false);
            }
            mainStatTwoCategory.text = mainCategoryTwo;
            mainStatTwoTxt.text = mainTextTwo;

            if (mainCategoryThree == "")
            {
                mainStatThreeCategory.gameObject.SetActive(false);
                mainStatThreeTxt.gameObject.SetActive(false);
            }
            mainStatThreeCategory.text = mainCategoryThree;
            mainStatThreeTxt.text = mainTextThree;
            empty = false;
        }



        public void OnPointerClick(PointerEventData pointerData)
        {
            if (pointerData.button == PointerEventData.InputButton.Right)
            {
                OnRightMouseBtnClick?.Invoke(this);
            }
            else
            {
                OnItemClicked?.Invoke(this);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (empty)
            {
                return;
            }
            OnItemBeginDrag?.Invoke(this);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            OnItemEndDrag?.Invoke(this);
        }

        public void OnDrop(PointerEventData eventData)
        {
            OnItemDroppedOn?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {

        }
    }
}