using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIClassStats : MonoBehaviour
{
    public TMP_Text stats;
    
    public void ChangeStrength(int abiltyScore)
    {
        if(abiltyScore == 0)
        {
            return;
        }

        if(abiltyScore < 0)
        {
            stats.text += "-" + (abiltyScore * -1) + " STR";
        }
        else
        {
            stats.text += "+" + (abiltyScore) + " STR";
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
            stats.text += "-" + (abiltyScore * -1) + " AGI";
        }
        else
        {
            stats.text += "+" + (abiltyScore) + " AGI";
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
            stats.text += "-" + (abiltyScore * -1) + " End";
        }
        else
        {
            stats.text += "+" + (abiltyScore) + " END";
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
            stats.text += "-" + (abiltyScore * -1) + " INT";
        }
        else
        {
            stats.text += "+" + (abiltyScore) + " INT";
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
            stats.text += "-" + (abiltyScore * -1) + " WIS";
        }
        else
        {
            stats.text += "+" + (abiltyScore) + " WIS";
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
            stats.text += "-" + (abiltyScore * -1) + " CHA";
        }
        else
        {
            stats.text += "+" + (abiltyScore) + " CHA";
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
            stats.text += "-" + (abiltyScore * -1) + " LUK";
        }
        else
        {
            stats.text += "+" + (abiltyScore) + " LUK";
        }
    }
}
