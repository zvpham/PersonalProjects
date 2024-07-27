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

    public float dragSpeed = 0.1f;
    public bool draggingMouse = false;
    public Vector3 difference;
    public Vector3 prevMousePosition;
    public Vector3 currentMousePosition;

    public GameObject target;
    public bool targetIsBackground;
    public bool dontMoveCamera;
    public bool attachedToTarget;

    public CameraBounds cameraBoundsData;
    public float scrollChangeMultiplierStartingRate = 1.8f;
    public float scrollChangeMultiplierRate = 1.1f;
    public float scrollChangeMultiplierMax = 5f;
    public float scrollChangeMultiplierCurrent;
    public float scrollChangeMultiplierMin = 0.3f;
    public float bottomUISize;
    public float topUISize;
    public float leftUISize;
    public float rightUISize;
    public float minUISize;
    public float minSize = 0.3f;
    public float maxSize = 5;
    public float targetSize;
    public Vector3 defaultPosition;
    void Start()
    {
        if (target.TryGetComponent(out SpriteRenderer targetSpriteRenderer))
        {

        }
        if (target != null)
        {
            defaultPosition = new Vector3(target.transform.position.x, target.transform.position.y, -10);
            attachedToTarget = true;
        }
        else
        {
            Debug.Log("No Target Attached");
        }
        SetCameraBounds();
        transform.position = defaultPosition;
        scrollChangeMultiplierCurrent = scrollChangeMultiplierMax;
    }
    /*
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
    */
    public void ChangeCameraZoom(float zoomChange)
    {
        targetSize = mainCamera.orthographicSize - (zoomChange * scrollChangeMultiplierCurrent);
        mainCamera.orthographicSize = GetCameraSize();
        SetCameraBounds();
        // Case - zooming out and extents x or y goes negative
        if ((cameraBounds.extents.y < 0 || cameraBounds.extents.x < 0) && zoomChange < 0)
        {
            mainCamera.orthographicSize = maxSize;
            scrollChangeMultiplierCurrent = scrollChangeMultiplierStartingRate;
        }
        // zooming in and already max zoom out size
        else if (zoomChange > 0 && mainCamera.orthographicSize == maxSize
            - (zoomChange * scrollChangeMultiplierStartingRate))
        {
            scrollChangeMultiplierCurrent = scrollChangeMultiplierMax;
        }
        // zooming in
        else if (zoomChange > 0)
        {
            scrollChangeMultiplierCurrent = scrollChangeMultiplierCurrent / scrollChangeMultiplierRate;
            if (scrollChangeMultiplierCurrent < scrollChangeMultiplierMin)
            {
                scrollChangeMultiplierCurrent = scrollChangeMultiplierMin;
            }
        }
        //zooming out
        else if (zoomChange < 0)
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
            if (attachedToTarget)
            {
                targetPosition = target.transform.position;
            }
            targetPosition = GetCameraBounds();
            transform.position = targetPosition;
        }
    }

    // Output -  Whether Camera Successfully Moved
    public bool MoveCamera(Vector3 newDirection)
    {
        if (mainCamera.orthographicSize != maxSize && cameraBounds.extents.y >= 0)
        {
            attachedToTarget = false;
            difference = newDirection * dragSpeed;
            targetPosition = transform.position - difference;
            targetPosition = GetCameraBounds();

            transform.position = targetPosition;
            SetCameraBounds();
            return true;
        }
        return false;
    }

    public void MoveCameraToTarget()
    {
        if(target == null || dontMoveCamera || enabled == false)
        {
            return;
        }

        Debug.Log("Hello");
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


        float minX, maxX, minY, maxY;

        if(cameraBoundsData != null)
        {
            minX = cameraBoundsData.WorldBounds.min.x + width - Mathf.Clamp((height * leftUISize), minUISize, 100);
            maxX = cameraBoundsData.WorldBounds.max.x - width + Mathf.Clamp((height * rightUISize), minUISize, 100);

            minY = cameraBoundsData.WorldBounds.min.y + height - Mathf.Clamp((height * bottomUISize), minUISize, 100);
            maxY = cameraBoundsData.WorldBounds.max.y - height + Mathf.Clamp((height * topUISize), minUISize, 100);
        }
        else
        {
            if(target != null)
            {
                minX = (target.transform.position.x - target.transform.localScale.x / 2) + width - Mathf.Clamp((width * leftUISize), minUISize, 100);
                maxX = (target.transform.position.x + target.transform.localScale.x / 2) - width + Mathf.Clamp((width * rightUISize), minUISize, 100);

                minY = (target.transform.position.y - target.transform.localScale.y / 2) + height - Mathf.Clamp((height * bottomUISize), minUISize, 100);
                maxY = (target.transform.position.y + target.transform.localScale.y / 2) - height + Mathf.Clamp((height * topUISize), minUISize, 100);
            }
            else
            {
                minX = 0;
                maxX = 0;
                minY = 0;
                maxY = 0;
                Debug.LogError("Need Either CameraBounds or Target to create MainCamera Bounds");
            }
        }

        cameraBounds = new Bounds();
        cameraBounds.center = defaultPosition;
        cameraBounds.SetMinMax(
            new Vector3(minX, minY, 0f),
            new Vector3(maxX, maxY, 0f)
            );
    }

    private Vector3 GetCameraBounds()
    {
        if(cameraBounds.extents.x < 0 || cameraBounds.extents.y < 0)
        {
            return new Vector3 (targetPosition.x, targetPosition.y, transform.position.z);
        }

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
