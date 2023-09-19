using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpectedLocationMarker : MonoBehaviour
{
    public float selfDestructionTimer;
    public float currentTime = 0;
    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        if(currentTime >= selfDestructionTimer)
        {
            Destroy(this.gameObject);
        }
    }
}
