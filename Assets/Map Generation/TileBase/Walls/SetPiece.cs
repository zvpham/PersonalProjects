using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPiece : Wall
{
    // Start is called before the first frame update
    public int setPieceIndex;
    public override void Start()
    {
        gameManager.setPieces.Add(this);
        gameManager.ChangeWalls(transform.position, this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
