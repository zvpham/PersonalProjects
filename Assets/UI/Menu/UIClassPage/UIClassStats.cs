using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIClassStats : BaseGameUIObject
{
    public string statline = "";
    public void ChangeStrength(int abiltyScore)
    {
        if(abiltyScore == 0)
        {
            return;
        }

        if(abiltyScore < 0)
        {
            statline += "-" + (abiltyScore * -1) + " STR";
        }
        else
        {
            statline += "+" + (abiltyScore) + " STR";
        }
    }
    public void ChangeAgility(int abiltyScore)
    {
        if (abiltyScore == 0)
        {
            return;
        }

        if (abiltyScore < 0)
        {
            statline += "-" + (abiltyScore * -1) + " AGI";
        }
        else
        {
            statline += "+" + (abiltyScore) + " AGI";
        }
    }
    public void ChangeEndurance(int abiltyScore)
    {
        if (abiltyScore == 0)
        {
            return;
        }

        if (abiltyScore < 0)
        {
            statline += "-" + (abiltyScore * -1) + " End";
        }
        else
        {
            statline += "+" + (abiltyScore) + " END";
        }
    }
    public void ChangeIntelligence(int abiltyScore)
    {
        if (abiltyScore == 0)
        {
            return;
        }

        if (abiltyScore < 0)
        {
            statline += "-" + (abiltyScore * -1) + " INT";
        }
        else
        {
            statline += "+" + (abiltyScore) + " INT";
        }
    }
    public void ChangeWisdom(int abiltyScore)
    {
        if (abiltyScore == 0)
        {
            return;
        }

        if (abiltyScore < 0)
        {
            statline += "-" + (abiltyScore * -1) + " WIS";
        }
        else
        {
            statline += "+" + (abiltyScore) + " WIS";
        }
    }
    public void ChangeCharisma(int abiltyScore)
    {
        if (abiltyScore == 0)
        {
            return;
        }

        if (abiltyScore < 0)
        {
            statline += "-" + (abiltyScore * -1) + " CHA";
        }
        else
        {
            statline += "+" + (abiltyScore) + " CHA";
        }
    }
    public void ChangeLuck(int abiltyScore)
    {
        if (abiltyScore == 0)
        {
            return;
        }

        if (abiltyScore < 0)
        {
            statline += "-" + (abiltyScore * -1) + " LUK";
        }
        else
        {
            statline += "+" + (abiltyScore) + " LUK";
        }
    }
}
