using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDataPersistence
{
    public void LoadData(MapData mapData = null);

    public void SaveData(MapData mapData = null);
}
