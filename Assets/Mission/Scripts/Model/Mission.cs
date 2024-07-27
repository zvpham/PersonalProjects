using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

namespace Inventory.Model
{
    public class Mission : MonoBehaviour
    {
        // UI Elements
        public Sprite missionProviderImage;
        public Sprite missionTargetImage;
        public string missionName;
        public string missionDescription;
        public int dangerRating;
        public int reward;

        // Data Elements
        public MissionType missionType;
        public MissionUnitPlacementName missionUnitFormation;
        public Faction missionProviderFaction;
        public Faction missionTargetFaction;  
        
        //Additional Faction is for third enemy Faction in Intervene missions or a possible additional ally faction in other missions (not sure about last one)
        public Faction missionAdditionalFaction;
        public List<UnitSuperClass> enemyUnits;
        public List<UnitSuperClass> enemyUnits2;
        public List<UnitSuperClass> allyUnits;
        
        public void LoadMissionData(int newDangerRating = -1,  Faction newMissionProviderFaction = null,
            Faction newMissionTargetFaction = null, Faction newMissionAdditionalFaction = null)
        {
            if(newMissionProviderFaction != null)
            {
                missionProviderFaction = newMissionProviderFaction;
            }

            if (newMissionTargetFaction != null)
            {
                missionTargetFaction = newMissionTargetFaction;
            }

            if (newMissionAdditionalFaction != null)
            {
                missionAdditionalFaction = newMissionAdditionalFaction;
            }

            if(newDangerRating != -1)
            {
                dangerRating = newDangerRating;
            }

            int dangerRatingIndex = dangerRating - 1;

            missionProviderImage = missionProviderFaction.factionImage;
            missionTargetImage = missionTargetFaction.factionImage;

            List<MissionComposition> missionCompositions = new List<MissionComposition>();
            int seed = System.DateTime.Now.Millisecond;
            Random.InitState(seed);
            switch (missionType)
            {
                case (MissionType.Elimination):
                    missionCompositions = missionTargetFaction.missionDangerLevels[dangerRatingIndex].eleminationMissions;
                    int missionCompsoitionIndex = Random.Range(0, missionCompositions.Count);
                    MissionComposition finalMissionComposition = missionCompositions[missionCompsoitionIndex];
                    missionCompsoitionIndex = Random.Range(0, finalMissionComposition.enemyCompositions.Count);
                    EnemyComposition finalEnemyComposition = finalMissionComposition.enemyCompositions[missionCompsoitionIndex];
                    enemyUnits = finalEnemyComposition.GetComposition(missionTargetFaction);
                    missionUnitFormation = MissionUnitPlacementName.LineBattleWestStart;
                    break;
                case (MissionType.Assassination):
                    break;
                case (MissionType.Escort):
                    break;
                case (MissionType.Defend):
                    break;
                case (MissionType.Destroy):
                    break;
                case (MissionType.Duel):
                    break;
                case (MissionType.Intervene):
                    break;
            }


        }
    }



    public enum MissionType
    {
        Elimination,
        Assassination,
        Escort,
        Defend,
        Destroy,
        Duel,
        // Intervene - third party a fight between two hostile factions
        Intervene
    }
}
