using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillNodeUI : MonoBehaviour, IPointerClickHandler
{
    public Image border;
    public Image skillImage;

    public event Action<SkillNodeUI> OnClick;

    public void LoadImage(Sprite skillSprite)
    {
        skillImage.sprite = skillSprite;
    }

    public void Unlock()
    {
        border.color = GlobalSkillTree.unlockedColor;
    }

    public void Lock()
    {
        border.color = GlobalSkillTree.lockedColor;
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        OnClick?.Invoke(this);
    }
}
