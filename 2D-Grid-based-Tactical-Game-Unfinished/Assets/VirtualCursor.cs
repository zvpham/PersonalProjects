using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualCursor : MonoBehaviour
{
    [SerializeField] GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    [SerializeField] EventSystem m_EventSystem;
    [SerializeField] RectTransform canvasRect;
    public List<RaycastResult> results;
    public Vector3 PreviousMousePosition = Vector3.zero;
    public static VirtualCursor Instance;

    public void Awake()
    {
        Instance = this;
    }


    void Update()
    {
        if(Input.mousePosition != PreviousMousePosition)
        {
            PreviousMousePosition = Input.mousePosition;
            //Set up the new Pointer Event
            m_PointerEventData = new PointerEventData(m_EventSystem);
            //Set the Pointer Event Position to that of the game object
            m_PointerEventData.position = Input.mousePosition;

            //Create a list of Raycast Results
            results = new List<RaycastResult>();

            //Raycast using the Graphics Raycaster and mouse click position
            m_Raycaster.Raycast(m_PointerEventData, results);

            if (results.Count > 0)
            {
                BaseGameUIObject UIObject = results[0].gameObject.GetComponent<BaseGameUIObject>();
                UIObject.MouseUseUI();
                /*
                foreach (RaycastResult uiObject in results)
                {
                    Debug.Log(uiObject);
                }
                //Debug.Log(results[0]);
                */
            }
        }
    }
}
