using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Darkness : MonoBehaviour
{
    public GameManager gameManager;
    public SpriteRenderer spriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        gameManager.spriteGrid.GetGridObject(transform.position).sprites[0] = spriteRenderer;
    }
}
