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

        public int size = 0;

        public event Action<List<Unit>> OnCharacterMenuUpdated;

        public void Initialize()
        {
            heroes = new List<Unit>();
            for (int i = 0; i < size; i++)
            {
                heroes.Add(null);
            }
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
    }
}
