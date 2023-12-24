using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CharacterStatHealthModifierSO : CharacterStatModifierSO
{
    public override void AffectCharacter(GameObject character, float value)
    {
        Debug.Log(character);
        Debug.Log("THIS IS THE HEALTH" + value);
        if (character.GetComponent<Unit>().health != -1)
        {
            character.GetComponent<Unit>().health += (int)value;
        }
    }
}