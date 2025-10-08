using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SetPiece : Wall
{
    // Start is called before the first frame update
    public int setPieceIndex;
    public override void Start()
    {
        gameManager.setPieces.Add(this);
        gameManager.ChangeWalls(transform.position, this);
    }

    public override bool IsSetPiece()
    {
        return true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Death()
    {
        continueDeath = true;
        Died();
        if (!continueDeath)
        {
            return;
        }

        //gameManager.scripts.RemoveAt(index);
        gameManager.ChangeWalls(gameObject.transform.position, null);
        gameManager.setPieces.Remove(this);
        Destroy(this);
        Destroy(gameObject);
    }
}
