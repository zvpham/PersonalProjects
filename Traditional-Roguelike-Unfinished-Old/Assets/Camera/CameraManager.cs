using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera gameCamera;
    public Camera worldMapCamera;
    public static CameraManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Found more than one Camera Manager in the Scence");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }
}
