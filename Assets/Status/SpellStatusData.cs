using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellStatusData : StatusData
{
    public int spellPointsRequired;
    public Action spell;
    public SpellStatusData(Status status, int duration, bool noDuration, int spellPointsRequired, Action spell) : base(status, duration, noDuration)
    {
        this.spellPointsRequired = spellPointsRequired;
        this.spell = spell;
    }
}
