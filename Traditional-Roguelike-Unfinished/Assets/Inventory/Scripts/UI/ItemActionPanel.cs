using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Inventory.UI
{
    public class ItemActionPanel : MonoBehaviour
    {
        [SerializeField]
        private GameObject buttonPrefab;

        private UIInventoryItem item;

        public void AddButton(string name, UnityAction onClickAction)
        {
            Debug.Log("Name" + name);
            GameObject button = Instantiate(buttonPrefab, transform);
            Debug.Log("This is a butyton " + button);
            button.GetComponent<Button>().onClick.AddListener(onClickAction);
            button.GetComponentInChildren<TMPro.TMP_Text>().text = name;
        }

        public void Toggle(bool val, UIInventoryItem inventoryItem = null)
        {
            item = inventoryItem;
            if (val == true)
            {
                RemoveOldButtons();
            }
            gameObject.SetActive(val);
        }

        private void Update()
        {
            transform.position = item.transform.position;
        }

        private void RemoveOldButtons()
        {
            foreach (Transform transformChildObjects in transform)
            {
                Destroy(transformChildObjects.gameObject);
            }
        }
    }
}
