using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstWallRender : MonoBehaviour
{
    public GameManager gameManger;
    public MainGameManger mainGameManger;

    // Update is called once per frame
    void Update()
    {
        gameManger.StartRender();
        //mainGameManger.CreateGrid();
        Destroy(this);
        Destroy(gameObject);
    }
}
