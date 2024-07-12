using Inventory.Model;
using Inventory.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MissionSelectSystem : MonoBehaviour
{
    [SerializeField]
    public OverWorldMenu overWorldMenu;

    [SerializeField]
    public UIMissionSelectMenu missionSelectMenu;

    [SerializeField]
    public MissionSelectMenuSO missionData;

    public List<Mission> initialMissions = new List<Mission>();

    [SerializeField]
    public AudioClip dropClip;

    [SerializeField]
    public AudioSource audioSource;

    public event Action<Mission> OnMissionClicked;
    public Mission currentMission;

    void Start()
    {
        PrepareUI();
        PrepareMissionSelectSystem();
    }

    public void OpenMenu()
    {
        missionSelectMenu.gameObject.SetActive(true);
        missionSelectMenu.OnOpenMenu();
        overWorldMenu.ChangeMenu += CloseMenu;
    }

    public void CloseMenu()
    {
        missionSelectMenu.gameObject.SetActive(false);
        overWorldMenu.ChangeMenu -= CloseMenu;
    }

    public void PrepareMissionSelectSystem()
    {
        missionData.OnMissionSelectMenuUpdated += UpdateMissionMenu;
    }

    private void UpdateMissionMenu(List<Mission> missions)
    {
        missionSelectMenu.ResetAllItems();

        for (int i = 0; i < missions.Count; i++)
        {
            if (missions[i] == null)
            {
                continue;
            }
            missionSelectMenu.UpdateData(i, missions[i].missionProviderImage, missions[i].missionTargetImage, missions[i].dangerRating,
                missions[i].missionName, missions[i].reward.ToString());
        }
    }

    private void PrepareUI()
    {
        this.missionSelectMenu.OnProfileClicked += HandleMissionClicked;
        this.missionSelectMenu.InitializeMissionSelectUI(missionData.size);
    }

    public void LoadInitialMissions()
    {
        for (int i = 0; i < initialMissions.Count; i++)
        {
            Mission mission = null;
            if (initialMissions[i] != null)
            {
                mission = Instantiate(initialMissions[i]);
                mission.transform.parent = this.transform;
                mission.LoadMissionData();
            }

            missionData.SetMission(i, mission);
        }
    }

    public void SetMission(int index, Mission newMission)
    {
        missionData.SetMission(index, newMission);
    }

    private void HandleMissionClicked(int missionINdex)
    {
        Mission mission = missionData.GetMissionAt(missionINdex);
        if (mission == null)
        {
            return;
        }
        currentMission = mission;
        OnMissionClicked?.Invoke(currentMission);
    }



    /*
    public void OnLoadEquipSoul(int soulSlotIndex, SoulItemSO soul)
    {
        player = Player.Instance;
        inventoryUI = UIInventoryPage.Instance;
        inventoryUI.soulSlots[soulSlotIndex].AddSoul(soul, player, true);
        player.UpdatePlayerActions();
    }
    */
}
