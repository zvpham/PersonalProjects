using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIClassDescription : MonoBehaviour
{
    [SerializeField]
    private TMP_Text description;

    public void ResetDescription()
    {
        description.text = "";
    }

    public void SetDescription(string newDescription)
    {
        description.text = newDescription;
    }
}
