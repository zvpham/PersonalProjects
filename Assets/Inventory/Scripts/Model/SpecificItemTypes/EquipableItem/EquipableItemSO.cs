using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            for (int i = 0; i < actions.Count; i++)
            {
                unit.AddAction(actions[i]);
            }

            for(int i = 0 ; i < passives.Count; i++)
            {
               
            }
        }
        
        public virtual void UnequipItem(Unit unit)
        {

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
