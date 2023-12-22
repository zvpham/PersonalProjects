using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ResourceManager")]
public class ResourceManager : ScriptableObject
{
    public List<SoulItemSO> souls = new List<SoulItemSO>();
    public List<Status> statuses = new List<Status>();
    public List<GameObject> unitPrefabs = new List<GameObject>();
    public GameObject genericItemPrefab = null;
    public List<ItemSO> items = new List<ItemSO>();

    public List<GameObject> animatedFields = new List<GameObject>();
    public List<CreatedField> createdFields = new List<CreatedField>();
    public List<CreatedObject> createdObjects = new List<CreatedObject>();
    public List<GameObject> createdObjectHoldersPrefabs = new List<GameObject>();

    public List<GameObject> setPiecies;
    public List<GameObject> walls;
    public List<WallSpriteStruct> wallSprites;

    public List<TileBase> tilesBases;
    public List<GameObject> tileBasePrefabs;

    public List<GameObject> WFCTrainerTemplates;

}
