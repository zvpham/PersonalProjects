using Inventory.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseFollower : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    public UICharacterProfile item;

    public void Awake()
    {
        canvas = transform.root.GetComponent<Canvas>();
    }

    public void SetData(Sprite sprite)
    {
        item.SetData(sprite);
    }

    private void Update()
    {
        //StopScrollLocationChangeOnSwap();
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform
            , Input.mousePosition
            , canvas.worldCamera
            , out position);
        transform.position = canvas.transform.TransformPoint(position);
    }

    public void Toggle(bool value)
    {
        gameObject.SetActive(value);
    }
}
