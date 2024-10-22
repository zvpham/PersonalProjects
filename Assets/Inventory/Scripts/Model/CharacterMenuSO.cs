using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

namespace Inventory.Model
{
    public class CharacterMenuSO : MonoBehaviour
    {
        [SerializeField]
        public List<Unit> heroes;

        [SerializeField]
        public List<UnitGroup> mercenaries;

        public int size = 0;
        public int mercSize = 0;

        public event Action<List<Unit>> OnCharacterMenuUpdated;
        public event Action<List<UnitGroup>> OnCharacterMenuUpdatedMerc;

        public void Initialize()
        {
            heroes = new List<Unit>();
            mercenaries = new List<UnitGroup>();
            for (int i = 0; i < size; i++)
            {
                heroes.Add(null);
            }

            for(int i = 0; i < mercSize; i++)
            {
                mercenaries.Add(null);
            }
        }

        public void IncreaseSize()
        {
            size += 1;
            heroes.Add(null);
        }

        public void SetHero(int index, Unit hero)
        {
            heroes[index] = hero;
            InformAboutChange();
            return;
        }

        public void AddHero(Unit hero)
        {
            AddHeroFirstAvailableSlot(hero);
            InformAboutChange();
            return;
        }

        private void AddHeroFirstAvailableSlot(Unit hero)
        {
            for (int i = 0; i < heroes.Count; i++)
            {
                if (heroes[i] == null)
                {
                    heroes[i] = hero;
                }
            }
            return;
        }

        public Unit GetHeroAt(int heroIndex)
        {
            return heroes[heroIndex];
        }
        
        public int GetIndexFirstNonEmptyHero()
        {
            for(int i = 0; i <heroes.Count; i++)
            {
                if (heroes[i] != null)
                {
                    return i;
                }
            }
            return -1;
        }

        public void SwapHero(int heroIndex1, int heroIndex2)
        {
            if (heroIndex1 != -1 && heroIndex2 != -1)
            {
                Unit item1 = heroes[heroIndex1];
                heroes[heroIndex1] = heroes[heroIndex2];
                heroes[heroIndex2] = item1;
                InformAboutChange();
            }
        }

        private void InformAboutChange()
        {
            OnCharacterMenuUpdated?.Invoke(heroes);
        }

        public void RemoveHero(int heroIndex)
        {
            if (heroes.Count > heroIndex)
            {
                if (heroes[heroIndex] == null)
                {
                    return;
                }

                heroes[heroIndex] = null;
                InformAboutChange();
            }
        }

        public void IncreaseMercenarySize()
        {
            mercSize += 1;
            mercenaries.Add(null);
        }

        public void SetMercenary(int index, UnitGroup merc)
        {
            mercenaries[index] = merc;
            InformAboutChangeMercenary();
            return;
        }

        public void AddMercenary(UnitGroup merc)
        {
            AddMercenaryFirstAvailableSlot(merc);
            InformAboutChangeMercenary();
            return;
        }

        private void AddMercenaryFirstAvailableSlot(UnitGroup merc)
        {
            for (int i = 0; i < mercenaries.Count; i++)
            {
                if (mercenaries[i] == null)
                {
                    mercenaries[i] = merc;
                }
            }
            return;
        }

        public UnitGroup GetMercenaryAt(int mercIndex)
        {
            return mercenaries[mercIndex];
        }

        public int GetIndexFirstNonEmptyMercenary()
        {
            for (int i = 0; i < mercenaries.Count; i++)
            {
                if (mercenaries[i] != null)
                {
                    return i;
                }
            }
            return -1;
        }

        public void SwapMercenary(int mercIndex1, int mercIndex2)
        {
            if (mercIndex1 != -1 && mercIndex2 != -1)
            {
                UnitGroup item1 = mercenaries[mercIndex1];
                mercenaries[mercIndex1] = mercenaries[mercIndex2];
                mercenaries[mercIndex2] = item1;
                InformAboutChangeMercenary();
            }
        }

        private void InformAboutChangeMercenary()
        {
            OnCharacterMenuUpdatedMerc?.Invoke(mercenaries);
        }

        public void RemoveMercenary(int mercIndex)
        {
            if (mercenaries.Count > mercIndex)
            {
                if (mercenaries[mercIndex] == null)
                {
                    return;
                }

                mercenaries[mercIndex] = null;
                InformAboutChangeMercenary();
            }
        }
    }
}
