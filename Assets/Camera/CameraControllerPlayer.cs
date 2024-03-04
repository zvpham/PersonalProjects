using CodeMonkey.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControllerPlayer : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject target;
    public Bounds cameraBounds;
    public Vector3 targetPosition;


    public float dragSpeed;
    public bool draggingMouse = false;
    public Vector3 difference;
    public Vector3 prevMousePosition;
    public Vector3 currentMousePosition;

    public float scrollChangeMultiplier;
    public float minSize;
    public float maxSize;
    public float targetSize;
    public Vector3 defaultPosition;
    void Start()
    {
        SetCameraBounds();
    }

    public void LateUpdate()
    {
        if(Input.mouseScrollDelta.y != 0)
        {
            targetSize = mainCamera.orthographicSize - (Input.mouseScrollDelta.y * scrollChangeMultiplier);
            mainCamera.orthographicSize = GetCameraSize();
            SetCameraBounds();
            if (mainCamera.orthographicSize == maxSize)
            {
                transform.position = defaultPosition;
            }
            else
            {
                targetPosition = GetCameraBounds();
                transform.position = targetPosition;
            }
        }

        if (mainCamera.orthographicSize != maxSize)
        {
            if (Input.GetMouseButtonDown(0))
            {
                draggingMouse = true;
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

    private void SetCameraBounds()
    {
        var height = mainCamera.orthographicSize;
        var width = height * mainCamera.aspect;

        var minX = CameraGlobal.WorldBounds.min.x + width;
        var maxX = CameraGlobal.WorldBounds.max.x - width;

        var minY = CameraGlobal.WorldBounds.min.y + height;
        var maxY = CameraGlobal.WorldBounds.max.y - height;

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
