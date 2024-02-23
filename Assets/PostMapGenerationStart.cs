using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostMapGenerationStart : MonoBehaviour
{
    public GameManager gameManager;
    public MainGameManger mainGameManger;
    public int indexofUnitEnabled;

    public void Awake()
    {
        indexofUnitEnabled = -1;
    }
    // Update is called once per frame
    void Update()
    {
        gameManager.StartRender();
        for (int i = 0; i < gameManager.tileVisibilityStates.GetLength(1); i++)
        {
            for (int j = 0; j < gameManager.tileVisibilityStates.GetLength(0); j++)
            {
                Vector3 tilePosition = new Vector3(j + (int)gameManager.defaultGridPosition.x, i + (int)gameManager.defaultGridPosition.y, 0);
                switch (gameManager.tileVisibilityStates[j, i])
                {
                    case 1:
                        gameManager.spriteGrid.GetGridObject(tilePosition).sprites[0].color = new Color(0, 0, 0, gameManager.VisitedSpaceAlpha);
                        break;
                    case 2:
                        gameManager.spriteGrid.GetGridObject(tilePosition).sprites[0].color = new Color(0, 0, 0, gameManager.KnownSpaceAlpha);
                        gameManager.tilesBeingActivelySeen.Add(new Vector2Int( (int) tilePosition.x, (int) tilePosition.y));
                        break;
                }
            }
        }
        Debug.Log(indexofUnitEnabled);
        if(indexofUnitEnabled != -1)
        {
            mainGameManger.units[indexofUnitEnabled].enabled = true;
            mainGameManger.units[indexofUnitEnabled].OnTurnStart();
        }
        Destroy(this);
        Destroy(gameObject);
    }
}
