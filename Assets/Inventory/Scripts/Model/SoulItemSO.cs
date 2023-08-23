using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Inventory.Model
{
    [CreateAssetMenu(menuName = "Item/SoulItem")]
    public class SoulItemSO : ItemSO, IDestroyableItem, IItemAction
    {
        public string ActionName => "Equip";


        [field: SerializeField]
        public List<Action> physicalAbilityList { get; set; }

        public int physicalStrength;
        public int physicalAgility;
        public int physicalEndurance;
        public int physicalWisdom;
        public int physicalIntelligence;
        public int physicalCharisma;


        [field: SerializeField]
        public List<Action> mentalAbilityList { get; set; }
            
        public int mentalStrength;
        public int mentalAgility;
        public int mentalEndurance;
        public int mentalWisdom;
        public int mentalIntelligence;
        public int mentalCharisma;


        public bool IsActionInUnit(Action action, Unit self)
        {
            foreach (Action unitAction in self.actions)
            {
                if (action.actionName.Equals(unitAction.actionName))
                {
                    return true;
                }
            }
            return false;
        }

        public void AddPhysicalSoul(Unit self)
        {
            self.ChangeStr(physicalStrength);
            self.ChangeAgi(physicalAgility);
            self.ChangeEnd(physicalEndurance);
            self.ChangeWis(physicalWisdom);
            self.ChangeInt(physicalIntelligence);
            self.ChangeCha(physicalCharisma);

            if (physicalAbilityList.Count != 0)
            {
                foreach (Action action in physicalAbilityList)
                {
                    if(IsActionInUnit(action, self))
                    {
                        continue;
                    }
                    else
                    {
                        self.actions.Add(Instantiate(action));
                    }
                }
            }
        }

        public void RemovePhysicalSoul(Unit self)
        {
            self.ChangeStr(-physicalStrength);
            self.ChangeAgi(-physicalAgility);
            self.ChangeEnd(-physicalEndurance);
            self.ChangeWis(-physicalWisdom);
            self.ChangeInt(-physicalIntelligence);
            self.ChangeCha(-physicalCharisma);

            if (physicalAbilityList.Count != 0)
            {
                foreach (Action action in physicalAbilityList)
                {
                    if (IsActionInUnit(action, self))
                    {
                        continue;
                    }
                    else
                    {
                        self.actions.Add(Instantiate(action));
                    }
                }
            }
        }

        public void AddMentalSoul(Unit self)
        {
            self.ChangeStr(mentalStrength);
            self.ChangeAgi(mentalAgility);
            self.ChangeEnd(mentalEndurance);
            self.ChangeWis(mentalWisdom);
            self.ChangeInt(mentalIntelligence);
            self.ChangeCha(mentalCharisma);

            if (mentalAbilityList.Count != 0)
            {
                foreach (Action action in mentalAbilityList)
                {
                    if (IsActionInUnit(action, self))
                    {
                        continue;
                    }
                    else
                    {
                        self.actions.Add(Instantiate(action));
                    }
                }
            }
        }

        public void RemoveMentalSoul(Unit self)
        {
            self.ChangeStr(-mentalStrength);
            self.ChangeAgi(-mentalAgility);
            self.ChangeEnd(-mentalEndurance);
            self.ChangeWis(-mentalWisdom);
            self.ChangeInt(-mentalIntelligence);
            self.ChangeCha(-mentalCharisma);

            if (mentalAbilityList.Count != 0)
            {
                foreach (Action action in mentalAbilityList)
                {
                    if (IsActionInUnit(action, self))
                    {
                        continue;
                    }
                    else
                    {
                        self.actions.Add(Instantiate(action));
                    }
                }
            }
        }

        public bool PerformAction(GameObject character, List<ItemParameter> itemState = null)
        {
            /*
            AgentWeapon weaponSystem = character.GetComponent<AgentWeapon>();
            if (weaponSystem != null){ 
                weaponSystem.SetWeapon(this, itemState == null ?
                    DefaultParameterList: itemState);
                return true;
            }
            return false;
            */
            return true;
        }
    }
}

