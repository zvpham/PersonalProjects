using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    public class MissionStartCharacterSO : MonoBehaviour
    {

        [SerializeField]
        public List<UnitSuperClass> units;

        public int size = 0;
        public bool isFrontline = false;


        public event Action<List<UnitSuperClass>> OnMissionStartCharacterMenuUpdated;

        public void Initialize()
        {
            units = new List<UnitSuperClass>();
            for (int i = 0; i < size; i++)
            {
                units.Add(null);
            }
        }

        public void IncreaseSize()
        {
            size += 1;
            units.Add(null);
        }

        public void SetUnit(int index, UnitSuperClass unit)
        {
            units[index] = unit;
            InformAboutChange();
            return;
        }

        public UnitSuperClass GetUnitAt(int unitIndex)
        {
            return units[unitIndex];
        }

        public void SwapUnit(int unitIndex1, int unitIndex2)
        {
            if (unitIndex1 != -1 && unitIndex2 != -1)
            {
                UnitSuperClass item1 = units[unitIndex1];
                units[unitIndex1] = units[unitIndex2];
                units[unitIndex2] = item1;
                InformAboutChange();
            }
        }

        private void InformAboutChange()
        {
            OnMissionStartCharacterMenuUpdated?.Invoke(units);
        }

        public void RemoveUnit(int unitIndex)
        {
            if (units.Count > unitIndex)
            {
                if (units[unitIndex] == null)
                {
                    return;
                }

                units[unitIndex] = null;
                InformAboutChange();
            }
        }
    }
}
