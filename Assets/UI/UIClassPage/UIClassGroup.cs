using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIClassGroup : MonoBehaviour
{
    public TMP_Text className;

    public void SetClass(string className)
    {
        this.className.text = className;
    }
}
