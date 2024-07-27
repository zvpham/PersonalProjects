using CodeMonkey.Utils;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;
    public CombatGameManager gameManager;
    public CameraControllerPlayer cameraController;
    public PlayerTurn player;

    private KeyBindings keyBindings;
    public GameObject inputManager;

    public bool isDragging = false;
    public bool startDragging = false;
    public bool mouseOverUI = false;
    public Vector3 prevMousePosition;
    public Vector3 currentMousePosition;

    public float amountOfTimeMouseInSamePosition = 0;
    public float amountOfTimeMouseNeedsToStayInPlaceToCallEvent = 1.5f;

    public List<PlayerAction> playerActions;

    [SerializeField] GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    [SerializeField] EventSystem m_EventSystem;
    [SerializeField] RectTransform canvasRect;
    public List<RaycastResult> results;

    public UnityAction FoundPosition, MouseMoved, MouseStayedInPlace;
    public UnityAction<Vector3> TargetPositionMoved;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void Update()
    {
        Vector3 tempCurrentMousePosition = currentMousePosition;
        currentMousePosition = Input.mousePosition;

        if (tempCurrentMousePosition == currentMousePosition)
        {
            amountOfTimeMouseInSamePosition += Time.deltaTime;
            if(amountOfTimeMouseInSamePosition > amountOfTimeMouseNeedsToStayInPlaceToCallEvent)
            {
                MouseStayedInPlace?.Invoke();
            }
        }
        else
        {
            amountOfTimeMouseInSamePosition = 0;
            MouseMoved?.Invoke();
        }

        cameraController.ChangeCameraZoom(Input.mouseScrollDelta.y);

        if (Input.GetMouseButtonDown(0))
        {
            prevMousePosition = Input.mousePosition;
            startDragging = true;

            //Check to see if Mouse Hits UI when clicked
            m_PointerEventData = new PointerEventData(m_EventSystem);
            //Set the Pointer Event Position to that of the game object
            m_PointerEventData.position = Input.mousePosition;

            //Create a list of Raycast Results
            results = new List<RaycastResult>();

            //Raycast using the Graphics Raycaster and mouse click position
            m_Raycaster.Raycast(m_PointerEventData, results);

            if (results.Count > 0)
            {
                for (int i = 0; i < results.Count; i++)
                {
                    BaseUiObject UIObject = results[i].gameObject.GetComponent<BaseUiObject>();
                    if (UIObject != null)
                    {
                        mouseOverUI = true;
                    }
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            m_PointerEventData = new PointerEventData(m_EventSystem);
            //Set the Pointer Event Position to that of the game object
            m_PointerEventData.position = Input.mousePosition;

            //Create a list of Raycast Results
            results = new List<RaycastResult>();

            //Raycast using the Graphics Raycaster and mouse click position
            m_Raycaster.Raycast(m_PointerEventData, results);

            if (results.Count > 0)
            {
                for(int i = 0; i < results.Count; i++)
                {
                    BaseUiObject UIObject = results[i].gameObject.GetComponent<BaseUiObject>();
                    if (UIObject != null)
                    {
                        mouseOverUI = true;
                    }
                }
            }

            if (!isDragging && !mouseOverUI)
            {
                gameManager.grid.GetXY(currentMousePosition, out int x, out int y);
                FoundPosition?.Invoke();
            }
            isDragging = false;
            startDragging = false;
            mouseOverUI = false;
        }

        if (prevMousePosition != currentMousePosition)
        {
            if(startDragging)
            {
                isDragging = cameraController.MoveCamera(currentMousePosition - prevMousePosition);
            }
            else
            {
                ChangeTargetPosition();
            }
            prevMousePosition = currentMousePosition;
        }

        for (int i = 0; i < playerActions.Count; i++)
        {
            if (GetKeyDown(playerActions[i].actionName))
            {
                playerActions[i].Activate(this);
                break;
            }
        }

    }

    public void ChangeTargetPosition()
    {
        TargetPositionMoved?.Invoke(UtilsClass.GetMouseWorldPosition());
    }

    private void Start()
    {
        keyBindings = KeyBindings.instance;
    }

    public List<KeyCode> GetKeyForAction(PlayerActionName KeyBindingAction)
    {
        foreach (PlayerActionName key in keyBindings.defaultActionKeyBinds.Keys)
        {
            if (key == KeyBindingAction)
            {
                return keyBindings.defaultActionKeyBinds[key];
            }
        }
        KeyCode[] input = { KeyCode.None };
        List<KeyCode> temp = new List<KeyCode>(input);
        return  temp;
    }

    public bool GetKeyDown(PlayerActionName action)
    {
        // detects if player is pressing button
        if (!Input.anyKey)
            return false;

        foreach (PlayerActionName key in keyBindings.defaultActionKeyBinds.Keys)
        {
            if (key == action)
            {
                for (int i = 0; i < keyBindings.defaultActionKeyBinds[key].Count; i++)
                {
                    if (i == keyBindings.defaultActionKeyBinds[key].Count - 1)
                    {
                        return (Input.GetKeyDown(keyBindings.defaultActionKeyBinds[key][i]));
                    }
                    else
                    {
                        if (!Input.GetKey(keyBindings.defaultActionKeyBinds[key][i]))
                        {
                            return false;
                        }
                    }
                }
            }
        }
        return false;
    }



    public bool GetKey(PlayerActionName action)
    {
        // detects if player is pressing button
        if (!Input.anyKey)
            return false;

        foreach (PlayerActionName key in keyBindings.defaultActionKeyBinds.Keys)
        {
            if (key.Equals(action))
            {
                for (int i = 0; i < keyBindings.defaultActionKeyBinds[key].Count; i++)
                {
                    if (i == keyBindings.defaultActionKeyBinds[key].Count - 1)
                    {

                        return (Input.GetKey(keyBindings.defaultActionKeyBinds[key][i]));
                    }
                    else
                    {
                        if (!Input.GetKey(keyBindings.defaultActionKeyBinds[key][i]))
                        {
                            return false;
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool GetKeyUp(PlayerActionName action)
    {
        // detects if player is pressing button
        if (!Input.anyKey)
            return false;

        foreach (PlayerActionName key in keyBindings.defaultActionKeyBinds.Keys)
        {
            if (key.Equals(action))
            {
                for (int i = 0; i < keyBindings.defaultActionKeyBinds[key].Count; i++)
                {
                    if (i == keyBindings.defaultActionKeyBinds[key].Count - 1)
                    {

                        return (Input.GetKeyUp(keyBindings.defaultActionKeyBinds[key][i]));
                    }
                    else
                    {
                        if (!Input.GetKey(keyBindings.defaultActionKeyBinds[key][i]))
                        {
                            return false;
                        }
                    }
                }
            }
        }
        return false;
    }
}