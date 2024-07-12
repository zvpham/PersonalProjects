using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDataPersistence
{
    public void LoadData(WorldMapData mapData = null);

    public void SaveData(WorldMapData mapData = null);
}
