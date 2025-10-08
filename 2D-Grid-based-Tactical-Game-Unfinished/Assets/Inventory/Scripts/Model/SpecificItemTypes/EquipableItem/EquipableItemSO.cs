using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

namespace Inventory.Model
{
    [CreateAssetMenu(menuName = "Item/EquipableItem")]
    public class EquipableItemSO : ItemSO
    {
        //Display
        public string attributeOne;
        public string attributeTwo;

        /*
        public string mainCategoryOne = "Weight";
        public int mainOneMin;
        public int mainOneMax;

        public string mainCategoryTwo;
        public float mainTwoMin;
        public int mainTwoMax;

        public string mainCategoryThree;
        public int mainThreeMin;
        public int mainThreeMax;
        */

        // Data
        public EquipType equipType;
        public List<ItemTypes> itemTypes;
        public List<Action> actions = new List<Action>();
        public List<Passive> passives = new List<Passive>();

        public virtual void EquipItem(Unit unit, bool isBackUp = false)
        {
            bool isWeapon = false;

            switch (equipType)
            {
                case (EquipType.Accessory1):
                    unit.Item1 = this;
                    break;
                case (EquipType.Accessory2):
                    unit.Item2 = this;
                    break;
                case (EquipType.Accessory3):
                    unit.Item3 = this;
                    break;
                case (EquipType.Accessory4):
                    unit.Item4 = this;
                    break;
                case (EquipType.Head):
                    unit.helmet = this;
                    break;
                case (EquipType.Body):
                    unit.armor = this;
                    break;
                case (EquipType.Boot):
                    unit.legs = this;
                    break;
                case (EquipType.MainHand):
                    isWeapon = true;
                    if (isBackUp)
                    {
                        unit.backUpMainHand = this;
                    }
                    else
                    {
                        unit.mainHand = this;
                    }
                    break;
                case (EquipType.OffHand):
                    isWeapon = true;
                    if (isBackUp)
                    {
                        unit.backUpOffHand = this;
                    }
                    else
                    {
                        unit.offHand = this;
                    }
                    break;
                case (EquipType.BothHands):
                    isWeapon = true;
                    break;
            }

            for(int i = 0; i < DefaultParameterList.Count; i++)
            {
                EquipParameter(unit, isBackUp, isWeapon, DefaultParameterList[i]);
            }

            unit.ChangeStrength(unit.strength);

            if (!isBackUp)
            {
                for (int i = 0; i < actions.Count; i++)
                {
                    actions[i].AddAction(unit);
                }

                for (int i = 0; i < passives.Count; i++)
                {
                    passives[i].AddPassive(unit);
                }

            }
        }

        public void EquipParameter(Unit unit, bool isBackUp, bool isWeapon, ItemParameter itemParameter)
        { 
            switch(itemParameter.itemParameter.itemParameter)
            {
                case ItemParameterName.Capacity:
                    break;
                case ItemParameterName.spellPointGeneration:
                    unit.powerPointGeneration += (int) itemParameter.value[0];
                    break;
                case ItemParameterName.armor:
                    break;
                case ItemParameterName.damge:
                    break;
                case ItemParameterName.weight:
                    int weight =  (int)itemParameter.value[0];
                    if (isBackUp)
                    {
                        unit.ChangeBackUpWeight(unit.backUpWeight + weight);
                        unit.ChangeWeight(unit.currentWeight + (weight / 2));

                    }
                    else
                    {

                        unit.ChangeWeight(unit.currentWeight + weight);
                        if (isWeapon)
                        {
                            unit.ChangeBackUpWeight(unit.backUpWeight + (weight / 2));
                        }
                        else
                        {
                            unit.ChangeBackUpWeight(unit.backUpWeight + weight);
                        }
                    }
                    break;

            }
        }

        public virtual void UnequipItem(Unit unit, bool isBackUp)
        {
            Debug.LogWarning("Unequip Item: " + name);
            bool isWeapon = false;
            if(equipType == EquipType.MainHand || equipType == EquipType.OffHand || equipType == EquipType.BothHands)
            {
                isWeapon = true;
            }

            for (int i = 0; i < DefaultParameterList.Count; i++)
            {
                UnequipParameter(unit, isBackUp, isWeapon, DefaultParameterList[i]);
            }
        }

        public void UnequipParameter(Unit unit, bool isBackUp, bool isWeapon, ItemParameter itemParameter)
        {
            switch (itemParameter.itemParameter.itemParameter)
            {
                case ItemParameterName.Capacity:
                    break;
                case ItemParameterName.spellPointGeneration:
                    unit.powerPointGeneration -= (int)itemParameter.value[0];
                    break;
                case ItemParameterName.armor:
                    break;
                case ItemParameterName.damge:
                    break;
                case ItemParameterName.weight:
                    int weight = (int)  itemParameter.value[0];
                    if (isBackUp)
                    {
                        unit.ChangeWeight(unit.currentWeight - (weight / 2));
                        unit.ChangeBackUpWeight(unit.backUpWeight - weight);
                    }
                    else
                    {
                        unit.ChangeWeight(unit.currentWeight - weight);
                        if (isWeapon)
                        {
                            unit.ChangeBackUpWeight(unit.backUpWeight - (weight / 2));
                        }
                        else
                        {
                            unit.ChangeBackUpWeight(unit.backUpWeight - weight);
                        }
                    }
                    break;

            }
        }
    }

    [System.Serializable]
    public enum EquipType
    {
        Head,
        Body,
        Boot,
        MainHand,
        OffHand,
        BothHands,
        Accessory,
        Accessory1,
        Accessory2,
        Accessory3,
        Accessory4
    }


}
