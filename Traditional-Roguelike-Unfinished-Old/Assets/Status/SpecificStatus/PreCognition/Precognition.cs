using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CreateAssetMenu(menuName = "Status/Precognition")]
public class Precognition : Status
{
    // String Data is path to save file
    // bool Data is wheter status belongs to a player

    public DataPersistenceManager dataPersistenceManager;
    public ConfirmationPopupMenu confirmationPopupMenu;
    public override void ApplyEffect(Unit target, int maxDuration)
    {

    }

    public override void ChangeQuicknessNonstandard(float value)
    {
        throw new System.NotImplementedException();
    }

    public override void onLoadApply(Unit target)
    {
        AddStatusOnLoadPreset(target);
        confirmationPopupMenu = ConfirmationPopupMenu.instance;
        dataPersistenceManager = DataPersistenceManager.Instance;
        target.OnDeath += OnDeath;
    }

    public override void RemoveEffect(Unit target)
    {
        // Note - Bool Data is if status belongs to Player
        if (statusBoolData)
        {
            RemoveEffectPlayer(target);
        }
        else
        {
            RemoveEffectEnemy(target);
        }
    }

    private void RemoveEffectEnemy(Unit target)
    {
        dataPersistenceManager.DeleteUserData(statusStringData);
        target.OnDeath -= OnDeath;
        RemoveStatusPreset(target);
    }

    private void RemoveEffectPlayer(Unit target)
    {
        confirmationPopupMenu.ActivateMenu("Do you want to keep this future?",
        // function to execute if we select 'yes'
        () =>
        {
            dataPersistenceManager.DeleteUserData(statusStringData);
            target.OnDeath -= OnDeath;
            RemoveStatusPreset(target);
        },
        // function to execute if we select 'Cancel'
        () =>
        {
            DataPersistenceManager.userID = statusStringData;
            target.OnDeath -= OnDeath;
            target.gameManager.ClearBoard();
            RemoveStatusPreset(target);
        });
    }

    public override void ApplyEffectEnemy(Unit target, int maxDuration)
    {
        confirmationPopupMenu = ConfirmationPopupMenu.instance;
        dataPersistenceManager = DataPersistenceManager.Instance;
        statusStringData = target.ToString() + DateTime.Now.ToString("yyyyMMdd@HHmmss");
        statusBoolData = false;
        dataPersistenceManager.ChangeGameData(statusStringData);
        dataPersistenceManager.SaveGame(statusStringData);
        dataPersistenceManager.ChangeGameData(null);
        target.OnDeath += OnDeath;
        AddStatusPreset(target, maxDuration);
    }

    public override void ApplyEffectPlayer(Unit target, int maxDuration)
    {
        confirmationPopupMenu = ConfirmationPopupMenu.instance;
        dataPersistenceManager = DataPersistenceManager.Instance;
        statusStringData =  Path.Combine(dataPersistenceManager.playerID + DateTime.Now.ToString("yyyyMMdd@HHmmss"));
        statusBoolData = true;
        dataPersistenceManager.ChangeGameData(statusStringData);
        dataPersistenceManager.SaveGame(statusStringData);
        dataPersistenceManager.ChangeGameData(null);
        target.OnDeath += OnDeath;
        AddStatusPreset(target, maxDuration);
    }

    public void OnDeath()
    {
        DataPersistenceManager.userID = statusStringData;
        affectedUnit.gameManager.ClearBoard();
    }
}
