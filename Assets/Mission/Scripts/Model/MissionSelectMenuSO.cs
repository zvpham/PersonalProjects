using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Model
{
    public class MissionSelectMenuSO : MonoBehaviour
    {
        [SerializeField]
        public List<Mission> missions;

        public int size = 0;

        public event Action<List<Mission>> OnMissionSelectMenuUpdated;

        public void SetMission(int index, Mission mission)
        {
            if(index >= missions.Count)
            {
                missions.Add(null);
            } 
            missions[index] = mission;
            InformAboutChange();
            return;
        }

        public void AddMission(Mission mission)
        {
            AddMissionFirstAvailableSlot(mission);
            InformAboutChange();
            return;
        }

        private void AddMissionFirstAvailableSlot(Mission mission)
        {
            bool foundOpenSlot = false;
            for (int i = 0; i < missions.Count; i++)
            {
                if (missions[i] == null)
                {
                    missions[i] = mission;
                    foundOpenSlot = true;
                }
            }

            if (!foundOpenSlot)
            {
                missions.Add(mission);
            }
            return;
        }

        public Mission GetMissionAt(int missionIndex)
        {
            Debug.Log(missionIndex);
            return missions[missionIndex];
        }

        public int GetIndexFirstNonEmptyHero()
        {
            for (int i = 0; i < missions.Count; i++)
            {
                if (missions[i] != null)
                {
                    return i;
                }
            }
            return -1;
        }

        private void InformAboutChange()
        {
            OnMissionSelectMenuUpdated?.Invoke(missions);
        }

        public void RemoveHero(int missionIndex)
        {
            if (missions.Count > missionIndex)
            {
                if (missions[missionIndex] == null)
                {
                    return;
                }

                missions[missionIndex] = null;
                InformAboutChange();
            }
        }
    }
}
