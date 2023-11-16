using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.GraphicsBuffer;

public class MapCreatorHelper : MonoBehaviour
{
    public Texture2D texture;
    public WallSpriteStruct wallSprite;
    void Start()
    {
        
    }

    public void Compile()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>(texture.name);
        wallSprite.UpperVerticalWall = sprites[0];
        wallSprite.NorthModifiedRightSmallConnector = sprites[1];
        wallSprite.NorthWallThreeWay = sprites[2];
        wallSprite.NorthModifiedLeftSmallConnector = sprites[3];
        wallSprite.CenterNorthSmallRightConnectorWall = sprites[4];
        wallSprite.CenterNorthThreeWay = sprites[5];
        wallSprite.CenterNorthSmallLeftConnectorWall = sprites[6];
        wallSprite.WestThreeWay = sprites[7];
        wallSprite.SouthModifiedRightSmallConnector = sprites[8];
        wallSprite.SouthWallThreeWay = sprites[9];
        wallSprite. SouthModifiedLeftSmallConnector = sprites[10];
        wallSprite.CenterSouthSmallRightConnectorWall = sprites[11];
        wallSprite.CenterSouthThreeWay = sprites[12];
        wallSprite.CenterSouthSmallLeftConnectorWall = sprites[13];
        wallSprite.LowerVerticalWall = sprites[14];
        wallSprite.NorthWestWallModified = sprites[15];
        wallSprite.CenterHorzontalWall = sprites[16];
        wallSprite.NorthEastWallModified = sprites[17];
        wallSprite.NorthWestWall = sprites[18];
        wallSprite.NorthWall = sprites[19];
        wallSprite.NorthEastWall = sprites[20];
        wallSprite.LeftHorzontalWall = sprites[21];
        wallSprite.EastThreeWay = sprites[22];
        wallSprite.CenterVerticalWall = sprites[23];
        wallSprite.WestWall = sprites[24];
        wallSprite.CenterWall = sprites[25];
        wallSprite.EastWall = sprites[26];
        wallSprite.SoleWall = sprites[27];
        wallSprite.SouthWestWallModified = sprites[28];
        wallSprite.RightHorzontalWall = sprites[29];
        wallSprite.SouthEastWallModified = sprites[30];
        wallSprite.SouthWestWall = sprites[31];
        wallSprite.SouthWall = sprites[32];
        wallSprite.SouthEastWall = sprites[33];
        wallSprite.WestModifiedUpperConnector = sprites[34];
        wallSprite.EastModifiedUpperConnector = sprites[35];
        wallSprite.CenterWestThreeWay = sprites[36];
        wallSprite.CenterEastThreeWay = sprites[37];
        wallSprite.CenterNorthEastToSouthWest = sprites[38];
        wallSprite.CenterNorthWestToSouthEast = sprites[39];
        wallSprite.CenterFourWay = sprites[40];
        wallSprite.WestModifiedLowerConnector = sprites[41];
        wallSprite.EastModifiedLowerConnector = sprites[42];
        wallSprite.CenterNorthWestConnector = sprites[43];
        wallSprite.CenterSouthWestConnector = sprites[44];
        wallSprite.CenterSouthEastConnector = sprites[45];
        wallSprite.CenterNorthEastConnector = sprites[46];
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MapCreatorHelper))]
public class WallSpriteEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapCreatorHelper me = (MapCreatorHelper)target;
        if (GUILayout.Button("Compile"))
        {
            me.Compile();
        }
        DrawDefaultInspector();
    }
}
#endif
