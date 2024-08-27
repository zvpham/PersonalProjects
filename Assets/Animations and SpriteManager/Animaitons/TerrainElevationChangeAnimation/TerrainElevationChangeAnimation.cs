using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class TerrainElevationChangeAnimation : CustomAnimations
{
    public CombatGameManager gameManager;
    public SpriteRenderer terrainTile;
    public int initalelevation;
    public int endElevation;
    public Vector2 originalPosition;
    public Vector2 newPosition;
    public Vector2 movementAmount;
    public float moveSpeed;
    public int moveSpeedPartitions = 24;
    public int positionindex;
    public int xpos, ypos;
    public List<GameObject> masksUsed = new List<GameObject>();

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        moveSpeed = totalTime / moveSpeedPartitions;
        movementAmount = (newPosition - originalPosition) / moveSpeedPartitions;
        positionindex = 0;

    }

    public override void PlayAnimation()
    {
        base.PlayAnimation();
        TerrainHolder terrain = gameManager.spriteManager.terrain[xpos, ypos];

        terrainTile = terrain.sprite;

        if(terrainTile == null)
        {
            EndAnimation();
        }

        spriteManager.ChangeTile(new Vector2Int(xpos, ypos), 0, 0);

        //Decrease Elevation
        if (endElevation < initalelevation)
        {
            for(int i = 0; i < initalelevation - endElevation; i++)
            {
                int wallIndex = terrain.walls.Count - 1 - i;
                if (wallIndex < 0)  
                {
                    break;
                }
                terrain.walls[wallIndex].sprite.sortingOrder = terrain.sprite.sortingOrder;
            }

            if(endElevation < gameManager.defaultElevation)
            {
                int topElevation = initalelevation;
                if(initalelevation > gameManager.defaultElevation)
                {
                    topElevation = gameManager.defaultElevation;
                }

                Vector3Int cubePos = spriteManager.spriteGrid.OffsetToCube(xpos, ypos);
                Vector3Int TLCube = cubePos + spriteManager.spriteGrid.cubeDirectionVectors[4];
                Vector3Int TCube = cubePos + spriteManager.spriteGrid.cubeDirectionVectors[5];
                Vector3Int TRCube = cubePos + spriteManager.spriteGrid.cubeDirectionVectors[0];

                Vector2Int TlOffset = spriteManager.spriteGrid.CubeToOffset(TLCube);
                Vector2Int TOffset = spriteManager.spriteGrid.CubeToOffset(TCube);
                Vector2Int TROffset = spriteManager.spriteGrid.CubeToOffset(TRCube);

                if (ValidGridPlacement(TlOffset) && !spriteManager.terrainIsChangingElevation[TlOffset.x, TlOffset.y] &&
                    spriteManager.elevationOfHexes[TlOffset.x, TlOffset.y] > endElevation)
                {
                    PlaceWallIfBelowGround(TlOffset);
                }

                if (ValidGridPlacement(TOffset) && !spriteManager.terrainIsChangingElevation[TOffset.x, TOffset.y] &&
                    spriteManager.elevationOfHexes[TOffset.x, TOffset.y] > endElevation)
                {
                    PlaceWallIfBelowGround(TOffset);
                }

                if (ValidGridPlacement(TROffset) && !spriteManager.terrainIsChangingElevation[TROffset.x, TROffset.y] &&
                    spriteManager.elevationOfHexes[TROffset.x, TROffset.y] > endElevation)
                {
                    PlaceWallIfBelowGround(TROffset);
                }
            }
        }
        //Increase Elevation
        else if(initalelevation <  endElevation)
        {
            Vector3Int cubePos = spriteManager.spriteGrid.OffsetToCube(xpos, ypos);
            Vector3Int BLCube = cubePos + spriteManager.spriteGrid.cubeDirectionVectors[3];
            Vector3Int BCube = cubePos + spriteManager.spriteGrid.cubeDirectionVectors[2];
            Vector3Int BRCube = cubePos + spriteManager.spriteGrid.cubeDirectionVectors[1];

            Vector2Int BlOffset = spriteManager.spriteGrid.CubeToOffset(BLCube);
            Vector2Int BOffset = spriteManager.spriteGrid.CubeToOffset(BCube);
            Vector2Int BROffset = spriteManager.spriteGrid.CubeToOffset(BRCube);

            if(initalelevation < gameManager.defaultElevation)
            {
                bool thereIsATileInFrontAndBelowThisTile = false;
                int lowestFrontElevation = 10;
                if (ValidGridPlacement(BlOffset) && !spriteManager.terrainIsChangingElevation[BlOffset.x, BlOffset.y] &&
                    spriteManager.elevationOfHexes[BlOffset.x, BlOffset.y] < gameManager.defaultElevation)
                {
                    thereIsATileInFrontAndBelowThisTile = true;
                    lowestFrontElevation = initalelevation;
                    if (spriteManager.elevationOfHexes[BlOffset.x, BlOffset.y] < lowestFrontElevation)
                    {
                        lowestFrontElevation = spriteManager.elevationOfHexes[BlOffset.x, BlOffset.y];
                    }
                }

                if (ValidGridPlacement(BOffset) && !spriteManager.terrainIsChangingElevation[BOffset.x, BOffset.y] &&
                    spriteManager.elevationOfHexes[BOffset.x, BOffset.y] < gameManager.defaultElevation)
                {
                    thereIsATileInFrontAndBelowThisTile = true;
                    if(spriteManager.elevationOfHexes[BOffset.x, BOffset.y] < lowestFrontElevation)
                    {
                        lowestFrontElevation = spriteManager.elevationOfHexes[BOffset.x, BOffset.y];
                    }
                }

                if (ValidGridPlacement(BROffset) && !spriteManager.terrainIsChangingElevation[BROffset.x, BROffset.y] &&
                    spriteManager.elevationOfHexes[BROffset.x, BROffset.y] < gameManager.defaultElevation)
                {
                    thereIsATileInFrontAndBelowThisTile = true;
                    if (spriteManager.elevationOfHexes[BROffset.x, BROffset.y] < lowestFrontElevation)
                    {
                        lowestFrontElevation = spriteManager.elevationOfHexes[BROffset.x, BROffset.y];
                    }
                }

                if(thereIsATileInFrontAndBelowThisTile)
                {
                    int bottomElevation = lowestFrontElevation;
                    int initialWallAmount = terrain.walls.Count;
                    for (int i = bottomElevation; i < endElevation; i++)
                    {
                        TerrainHolder newWall = gameManager.spriteManager.UseOpenWall();
                        newWall.transform.position = terrain.transform.position -
                            new Vector3(0, gameManager.terrainHeightDifference * (i - bottomElevation + initialWallAmount));
                        newWall.x = xpos;
                        newWall.y = ypos;
                        newWall.transform.parent = terrain.gameObject.transform;
                        newWall.sprite.sortingOrder = terrain.sprite.sortingOrder;
                        terrain.walls.Add(newWall);
                        //newWall.sprite.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
                    }
                }
                else
                {
                    int bottomElevation = initalelevation;
                    if (gameManager.defaultElevation > initalelevation)
                    {
                        bottomElevation = gameManager.defaultElevation;
                    }
                    int initialWallAmount = terrain.walls.Count;
                    for (int i = bottomElevation; i < endElevation; i++)
                    {
                        TerrainHolder newWall = gameManager.spriteManager.UseOpenWall();
                        newWall.transform.position = terrain.transform.position -
                            new Vector3(0, gameManager.terrainHeightDifference * (i - bottomElevation + initialWallAmount));
                        newWall.x = xpos;
                        newWall.y = ypos;
                        newWall.transform.parent = terrain.gameObject.transform;
                        newWall.sprite.sortingOrder = terrain.sprite.sortingOrder;
                        terrain.walls.Add(newWall);
                        //newWall.sprite.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
                    }
                }
            }
            else
            {
                int bottomElevation = initalelevation;
                if (gameManager.defaultElevation > initalelevation)
                {
                    bottomElevation = gameManager.defaultElevation;
                }

                int initialWallAmount = terrain.walls.Count;
                for (int i = bottomElevation; i < endElevation; i++)
                {
                    TerrainHolder newWall = gameManager.spriteManager.UseOpenWall();
                    newWall.transform.position = terrain.transform.position -
                        new Vector3(0, gameManager.terrainHeightDifference * (i - bottomElevation + initialWallAmount));
                    newWall.x = xpos;
                    newWall.y = ypos;
                    newWall.transform.parent = terrain.gameObject.transform;
                    newWall.sprite.sortingOrder = terrain.sprite.sortingOrder;
                    terrain.walls.Add(newWall);
                    //newWall.sprite.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
                }
            }
        }
    }


    public void PlaceWallIfBelowGround(Vector2Int hexPosition)
    {
        int newTerrainHeight = spriteManager.elevationOfHexes[hexPosition.x, hexPosition.y];
        int topHeight = initalelevation;
        if(initalelevation > gameManager.defaultElevation)
        {
            topHeight = gameManager.defaultElevation;
        }
        if (spriteManager.terrain[hexPosition.x, hexPosition.y] == null)
        {
            spriteManager.CreateElevationSprite(hexPosition.x, hexPosition.y, newTerrainHeight, false);
        }
        TerrainHolder newTerrain = spriteManager.terrain[hexPosition.x, hexPosition.y];
        for (int i = 0; i < topHeight - endElevation; i++)
        {
            if(newTerrain.walls.Count >= newTerrainHeight - endElevation)
            {
                return;
            }
            TerrainHolder newWall = gameManager.spriteManager.UseOpenWall();
            newWall.transform.position = newTerrain.transform.position -
                new Vector3(0, gameManager.terrainHeightDifference * (newTerrain.walls.Count));
            newWall.x = hexPosition.x;
            newWall.y = hexPosition.y;
            newWall.transform.parent = newTerrain.gameObject.transform;
            newWall.sprite.sortingOrder = newTerrain.sprite.sortingOrder;
            newTerrain.walls.Add(newWall);
        }
    }

    public void RemoveWallIfGoingAboveGround(Vector2Int hexPosition)
    {
        int newTerrainHeight = spriteManager.elevationOfHexes[hexPosition.x, hexPosition.y];
        int topHeight = endElevation;
        if (endElevation > gameManager.defaultElevation)
        {
            topHeight = gameManager.defaultElevation;
        }

        TerrainHolder newTerrain = spriteManager.terrain[hexPosition.x, hexPosition.y];
        for (int i = 0; i < topHeight - initalelevation; i++)
        {

            spriteManager.DisableWall(newTerrain.walls[newTerrain.walls.Count - 1]);
            newTerrain.walls.RemoveAt(newTerrain.walls.Count - 1);
        }
    }

    public bool ValidGridPlacement(Vector2Int offset)
    {
        return offset.x >= 0 && offset.y >= 0 && offset.x < spriteManager.spriteGrid.GetWidth() && offset.y < spriteManager.spriteGrid.GetHeight();
    }
    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= moveSpeed)
        {
            terrainTile.transform.position += (Vector3)movementAmount;
            positionindex += 1;
            if (positionindex >= moveSpeedPartitions)
            {
                EndAnimation();
            }
            currentTime = 0;
        }
    }

    public void SetParameters(CombatGameManager gameManager, Vector2 originalPosition, Vector2 newPosition, int xpos, int ypos, int initialElevation,
        int endElevation, bool partofAGroup)
    {
        this.gameManager = gameManager;
        this.originalPosition = originalPosition;
        this.newPosition = newPosition;
        this.xpos = xpos;   
        this.ypos = ypos;
        this.initalelevation = initialElevation;
        this.endElevation = endElevation;
        int animationIndex = gameManager.spriteManager.animations.Count;
        if (partofAGroup)
        {
            animationIndex = gameManager.spriteManager.animations.Count - 1;
        }
        gameManager.spriteManager.AddAnimations(this, animationIndex);
    }

    public override void EndAnimation()
    {
        /*
        TerrainHolder currentTerrainTile = spriteManager.terrain[initalelevation][xpos, ypos];
        if (endElevation == gameManager.defaultElevation)
        {
            for(int i = 0; i < currentTerrainTile.walls.Count; i++)
            {
                
            }
            Destroy(terrainTile.gameObject);
            spriteManager.terrain[initalelevation][xpos, ypos] = null;
            spriteManager.elevationOfHexes[xpos, ypos] = endElevation;
            spriteManager.ChangeTile(new Vector2Int(xpos, ypos), 0, spriteManager.terrainSprites[xpos, ypos]);
        }
        else
        {
            spriteManager.terrain[endElevation][xpos, ypos] = currentTerrainTile;
            spriteManager.terrain[initalelevation][xpos, ypos] = null;
            spriteManager.elevationOfHexes[xpos, ypos] = endElevation;

        }
        */

        TerrainHolder currentTerrainTile = spriteManager.terrain[xpos, ypos];
        spriteManager.elevationOfHexes[xpos, ypos] = endElevation;
        spriteManager.terrainIsChangingElevation[xpos, ypos] = false;

        // Decreased Elevation
        if(endElevation < initalelevation)
        {
            for(int i = 0; i < initalelevation - endElevation; i++)
            {
                if(currentTerrainTile.walls.Count == 0)
                {
                    break;
                }
                gameManager.spriteManager.DisableWall(currentTerrainTile.walls[currentTerrainTile.walls.Count - 1]);
                currentTerrainTile.walls.RemoveAt(currentTerrainTile.walls.Count - 1);
            }
        }
        // Increased elevation
        else if(initalelevation < endElevation)
        {
            if(initalelevation < gameManager.defaultElevation)
            {
                
            }
        }
        
        for(int i = 0; i < endElevation - gameManager.defaultElevation; i++)
        {
            currentTerrainTile.walls[i].sprite.sortingOrder = currentTerrainTile.sprite.sortingOrder;
        }

        for(int i = 0; i < masksUsed.Count; i++)
        {
            spriteManager.DisableMask(masksUsed[i]);
        }
        base.EndAnimation();
    }
}
