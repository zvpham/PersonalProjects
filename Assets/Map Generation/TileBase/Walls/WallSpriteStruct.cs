using System;
using System.IO;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "WallSpriteStruct")]
public class WallSpriteStruct : ScriptableObject
{
    public Texture2D texture;
    public Sprite
        UpperVerticalWall,
        NorthModifiedRightSmallConnector,
        NorthWallThreeWay,
        NorthModifiedLeftSmallConnector,
        CenterNorthSmallRightConnectorWall,
        CenterNorthThreeWay,
        CenterNorthSmallLeftConnectorWall,
        WestThreeWay,
        SouthModifiedRightSmallConnector,
        SouthWallThreeWay,
        SouthModifiedLeftSmallConnector,
        CenterSouthSmallRightConnectorWall,
        CenterSouthThreeWay,
        CenterSouthSmallLeftConnectorWall,
        LowerVerticalWall,
        NorthWestWallModified,
        CenterHorzontalWall,
        NorthEastWallModified,
        NorthWestWall,
        NorthWall,
        NorthEastWall,
        LeftHorzontalWall,
        EastThreeWay,
        CenterVerticalWall,
        WestWall,
        CenterWall,
        EastWall,
        SoleWall,
        SouthWestWallModified,
        RightHorzontalWall,
        SouthEastWallModified,
        SouthWestWall,
        SouthWall,
        SouthEastWall,
        WestModifiedUpperConnector,
        EastModifiedUpperConnector,
        CenterWestThreeWay,
        CenterEastThreeWay,
        CenterNorthEastToSouthWest,
        CenterNorthWestToSouthEast,
        CenterFourWay,
        WestModifiedLowerConnector,
        EastModifiedLowerConnector,
        CenterNorthWestConnector,
        CenterSouthWestConnector,
        CenterSouthEastConnector,
        CenterNorthEastConnector;

        
}
