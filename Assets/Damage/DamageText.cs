using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    float currentTime = 0;
    float maxTimeDuration = 1.5f;
    void Update()
    {
        currentTime += Time.deltaTime;
        if(currentTime >= maxTimeDuration)
        {
            Destroy(gameObject);
        }
        gameObject.transform.position += new Vector3(0, .005f, 0);
    }
}
