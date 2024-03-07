using CodeMonkey.Utils;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraControllerPlayer : MonoBehaviour
{
    public Camera mainCamera;
    public Bounds cameraBounds;
    public Vector3 targetPosition;

    public float dragSpeed;
    public bool draggingMouse = false;
    public Vector3 difference;
    public Vector3 prevMousePosition;
    public Vector3 currentMousePosition;

    public GameObject target;
    public bool attachedToTarget;

    public CameraBounds cameraBoundsData;
    public float scrollChangeMultiplierStartingRate;
    public float scrollChangeMultiplierRate;
    public float scrollChangeMultiplierMax;
    public float scrollChangeMultiplierCurrent;
    public float scrollChangeMultiplierMin;
    public float bottomUISize;
    public float topUISize;
    public float leftUISize;
    public float rightUISize;
    public float minUISize;
    public float minSize;
    public float maxSize;
    public float targetSize;
    public Vector3 defaultPosition;
    void Start()
    {
        defaultPosition = cameraBoundsData.WorldBounds.center;
        SetCameraBounds();
        transform.position = defaultPosition;
        scrollChangeMultiplierCurrent = scrollChangeMultiplierMax;
        if(target != null)
        {
            attachedToTarget = true;
        }
        else
        {
            Debug.Log("No Target Attached");
        }
    }

    public void LateUpdate()
    {
        // For Zooom By Scrolling
        if(Input.mouseScrollDelta.y != 0)
        {
            targetSize = mainCamera.orthographicSize - (Input.mouseScrollDelta.y * scrollChangeMultiplierCurrent);
            mainCamera.orthographicSize = GetCameraSize();
            SetCameraBounds();
            if((cameraBounds.extents.y < 0 || cameraBounds.extents.x < 0)  && Input.mouseScrollDelta.y < 0)
            {
                mainCamera.orthographicSize = maxSize;
                scrollChangeMultiplierCurrent = scrollChangeMultiplierMax;
            }
            else if(Input.mouseScrollDelta.y > 0 && mainCamera.orthographicSize == maxSize
                - (Input.mouseScrollDelta.y * scrollChangeMultiplierMax))
            {
                scrollChangeMultiplierCurrent = scrollChangeMultiplierStartingRate;
            }
            else if(Input.mouseScrollDelta.y > 0)
            {
                scrollChangeMultiplierCurrent = scrollChangeMultiplierCurrent / scrollChangeMultiplierRate;
                if(scrollChangeMultiplierCurrent < scrollChangeMultiplierMin)
                {
                    scrollChangeMultiplierCurrent = scrollChangeMultiplierMin;
                }
            }
            else if(Input.mouseScrollDelta.y < 0)
            {
                scrollChangeMultiplierCurrent = scrollChangeMultiplierCurrent * scrollChangeMultiplierRate;
                if (scrollChangeMultiplierCurrent > scrollChangeMultiplierMax)
                {
                    scrollChangeMultiplierCurrent = scrollChangeMultiplierMax;
                }
            }

            if (mainCamera.orthographicSize == maxSize)
            {
                transform.position = defaultPosition;
            }
            else
            {
                if(attachedToTarget)
                {
                    targetPosition = target.transform.position;
                }
                targetPosition = GetCameraBounds();
                transform.position = targetPosition;
            }
        }

        // For Dragging
        if (mainCamera.orthographicSize != maxSize && cameraBounds.extents.y >= 0)
        {
            if (Input.GetMouseButtonDown(0))
            {
                draggingMouse = true;
                attachedToTarget = false;
                prevMousePosition = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                draggingMouse = false;
            }

            if (draggingMouse)
            {
                currentMousePosition = Input.mousePosition;
                difference = (currentMousePosition - prevMousePosition) * dragSpeed;
                targetPosition = transform.position - difference;
                targetPosition = GetCameraBounds();

                transform.position = targetPosition;
                prevMousePosition = currentMousePosition;
                SetCameraBounds();
            }
        }
    }

    public void MoveCamera()
    {
        attachedToTarget = true;
        targetPosition = target.transform.position;
        targetPosition = GetCameraBounds();
        if(mainCamera.orthographicSize != maxSize)
        {
            transform.position = targetPosition;
        }
    }

    private void SetCameraBounds()
    {
        var height = mainCamera.orthographicSize;
        var width = height * mainCamera.aspect;

        var minX = cameraBoundsData.WorldBounds.min.x + width - Mathf.Clamp((height * leftUISize), minUISize, 100);
        var maxX = cameraBoundsData.WorldBounds.max.x - width + Mathf.Clamp((height * rightUISize), minUISize, 100);

        var minY = cameraBoundsData.WorldBounds.min.y + height - Mathf.Clamp((height * bottomUISize), minUISize, 100);
        var maxY = cameraBoundsData.WorldBounds.max.y - height + Mathf.Clamp((height * topUISize), minUISize, 100);

        cameraBounds = new Bounds();
        cameraBounds.center = defaultPosition;
        cameraBounds.SetMinMax(
            new Vector3(minX, minY, 0f),
            new Vector3(maxX, maxY, 0f)
            );
    }

    private Vector3 GetCameraBounds()
    {
        return new Vector3(
            Mathf.Clamp(targetPosition.x, cameraBounds.min.x, cameraBounds.max.x),
            Mathf.Clamp(targetPosition.y, cameraBounds.min.y, cameraBounds.max.y),
            transform.position.z);
    }

    private float GetCameraSize()
    {
        return Mathf.Clamp(targetSize, minSize, maxSize);
    }
}
