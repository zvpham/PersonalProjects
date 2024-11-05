using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "ResourceManager")]
public class ResourceManager : ScriptableObject
{
    public List<Sprite> hexBaseSprites;
    public List<Tile> BaseTile;

    public List<Color> highlightedHexColors;

    public List<EquipableItemSO> allItems;
    public List<EquipableItemSO> weapons;
    public List<EquipableItemSO> offHands;
    public List<EquipableItemSO> helmets;
    public List<EquipableItemSO> armor;
    public List<EquipableItemSO> boots;
    public List<EquipableItemSO> accessories;


    public StandardUnitActions standardUnitActions;
    public List<Action> actions;
    public List<MoveModifier> moveModifiers;
    public List<AttackModifier> meleeAttackModifiers;

    public List<UnitClass> job;
    public Unit emptyHero;
    public List<Unit> heroes;
    public List<UnitGroup> mercenaries;

    public SpriteRenderer spriteHolder;

    public List<Faction> factions;
    public Mission emptyMission;

    public List<MapTerrain> mapTerrains;
    public List<PrefabTerrain> prefabTerrains;

    public DamageAnimation damageAnimation;
    public AttackedAnimation attackedAnimation;
}
