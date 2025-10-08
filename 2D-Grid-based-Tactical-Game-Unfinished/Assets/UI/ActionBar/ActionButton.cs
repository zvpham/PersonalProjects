using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour   
{
    [SerializeField]
    private TMP_Text indexDisplay;
    [SerializeField]
    private TMP_Text usesLeft;
    [SerializeField]
    private TMP_Text coolDown;
    [SerializeField]
    private Image actionImage;

    public int actionIndex;

    public event UnityAction<int> useAction;

    public void ChangeSprite(Sprite sprite)
    {
        actionImage.sprite = sprite;
    }

    public void ChangeActionCoolDown(string newCoolDown)
    {
        if (newCoolDown.Equals("0"))
        {
            coolDown.text = null;
        }
        else
        {
            coolDown.text = newCoolDown;
        }
    }

    public void ChangeUsesLeft(string newUsesLeft)
    {
        if (newUsesLeft.Equals("0"))
        {
            usesLeft.text = null;
        }
        else
        {
            usesLeft.text = newUsesLeft;
        }
    }   

    public void ChangeActionIndex(int index)
    {
        actionIndex = index - 1;
        indexDisplay.text = index.ToString();
    }

    public void OnActionPressed()
    {
        useAction?.Invoke(actionIndex); 
    }
}
