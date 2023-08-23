using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlastRadiusMarkerPrefab : MonoBehaviour
{
    public GameObject gameObject;
    // Update is called once per frame
    void Update()
    {
        Destroy(gameObject);
        Destroy(this);
    }
}
