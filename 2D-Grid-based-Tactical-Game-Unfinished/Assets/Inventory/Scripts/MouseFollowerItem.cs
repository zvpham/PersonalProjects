using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

namespace Inventory.UI
{
    public class MouseFollowerItem : MonoBehaviour
    {
        [SerializeField]
        public Image itemImage;

        public void SetData(Sprite sprite)
        {
            itemImage.sprite = sprite;
        }
    }
}
