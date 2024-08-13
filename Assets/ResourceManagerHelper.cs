using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ResourceManagerHelper : MonoBehaviour
{
    public ResourceManager resourceManager;
    public void Compile()
    {
        for(int i = 0; i < resourceManager.allItems.Count; i++)
        {
            resourceManager.allItems[i].itemIndex = i;
            if (resourceManager.allItems[i].GetType() == typeof(EquipableWeaponSO))
            {
                EquipableWeaponSO weapon = (EquipableWeaponSO)resourceManager.allItems[i];
                weapon.mainCategoryTwo = "Damage";
                if (weapon.actions[0].GetType() == typeof(MeleeAttack))
                {
                    MeleeAttack meleeAttack = (MeleeAttack)weapon.actions[0];
                    weapon.mainTwoMin = meleeAttack.minDamage;
                    weapon.mainTwoMax = meleeAttack.maxDamage;
                }
                else if (weapon.actions[0].GetType() == typeof(RangedAttack))
                {
                    RangedAttack rangedAttack = (RangedAttack)weapon.actions[0];
                    weapon.mainTwoMin = rangedAttack.minDamage;
                    weapon.mainTwoMax = rangedAttack.maxDamage;
                }
            }
            else if (resourceManager.allItems[i].GetType() == typeof(EquipableAmmoSO))
            {
                EquipableAmmoSO ammo = (EquipableAmmoSO)resourceManager.allItems[i];
                ammo.mainCategoryTwo = "Damage";
                ammo.mainCategoryThree = "Capacity";
            }
        }

        for (int i = 0; i < resourceManager.heroes.Count; i++)
        {
            resourceManager.heroes[i].heroIndex = i;
        }

        for (int i = 0; i < resourceManager.mercenaries.Count; i++)
        {
            resourceManager.mercenaries[i].mercenaryIndex = i;
        }
        EditorUtility.SetDirty(resourceManager);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ResourceManagerHelper))]
public class WallSpriteEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ResourceManagerHelper me = (ResourceManagerHelper)target;
        if (GUILayout.Button("Compile"))
        {
            me.Compile();
        }
        DrawDefaultInspector();
    }
}
#endif