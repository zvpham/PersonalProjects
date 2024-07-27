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

        public string mainCategoryOne = "Weight";
        public int mainOneMin;
        public int mainOneMax;

        public string mainCategoryTwo;
        public int mainTwoMin;
        public int mainTwoMax;

        public string mainCategoryThree;
        public int mainThreeMin;
        public int mainThreeMax;


        // Data
        public EquipType equipType;
        public List<Action> actions = new List<Action>();
        public List<Passive> passives = new List<Passive>();

        public virtual void EquipItem(Unit unit)
        {
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
                    unit.mainHand = this;
                    break;
                case (EquipType.OffHand):
                    unit.offHand = this;
                    break;
            }

            for (int i = 0; i < actions.Count; i++)
            {
                unit.AddAction(actions[i]);
            }

            for(int i = 0 ; i < passives.Count; i++)
            {
               
            }

            unit.ChangeStrength(unit.strength);
            unit.ChangeWeight(unit.currentWeight + mainOneMin);
        }
        
        public virtual void UnequipItem(Unit unit)
        {
            unit.ChangeWeight(unit.currentWeight - mainOneMin);
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
