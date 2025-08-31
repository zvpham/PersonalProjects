using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillNodeConnectionUI : MonoBehaviour
{
    public Image connecterColor;

    public void Unlock()
    {
        connecterColor.color = GlobalSkillTree.unlockedColor;
    }

    public void Lock()
    {
        connecterColor.color = GlobalSkillTree.lockedColor;
    }
}
