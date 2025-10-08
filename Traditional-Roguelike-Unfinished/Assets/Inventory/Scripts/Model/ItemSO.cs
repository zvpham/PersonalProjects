using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu(menuName = "Item/Item")]
    public abstract class ItemSO : ScriptableObject
    {
        [field: SerializeField]
        public int itemIndex { get; set; }

        [field: SerializeField]
        public bool isStackable { get; set; }

        public int iD => GetInstanceID();

        [field: SerializeField]
        public int maxStackSize { get; set; } = 1;

        [field: SerializeField]
        public string itemName { get; set; }

        [field: SerializeField]
        [field: TextArea]
        public string description { get; set; }

        [field: SerializeField]
        public Sprite itemImage { get; set; }

        [field: SerializeField]
        public List<ItemParameter> DefaultParameterList { get; set; }

    }

    [Serializable]
    public struct  ItemParameter: IEquatable<ItemParameter>
    {
        public ItemParameterSO itemParameter;
        public float value;

        public bool Equals(ItemParameter other)
        {
            return other.itemParameter == itemParameter;
        }
    }
}   