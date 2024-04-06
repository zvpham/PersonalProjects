using Inventory.Model;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class TileData
{
    //public long lastUpdated;

    // Save Data
    public string saveToDelete;

    // Game Manager Data
    public int least;
    public int index;
    public bool aUnitActed;
    public int duringTurn;
    public bool notNewTile = false;
    public Vector2Int gameManagerPosition;

    // Visibilty Data
    public List<int> visibilityStates;

    // Wall Data
    public List<int> wallIndexes;
    public List<int> wallHealths;
    public List<Vector2> wallPositions;

    // SetPiece Data
    public List<int> setPieceIndexes;
    public List<int> setPieceHealths;
    public List<Vector2> setPiecePositions;

    // Item Data
    public List<int> itemIndexes;
    public List<int> itemQuantities;
    public List<Vector2> itemLocations;

    // Unit Data
    public int numberOfUnits;
    public List<unitPrefabData> unitPrefabDatas;

    // Forced MovementData;
    public List<ForcedMovementPathData> forcedMovementDatas;
    public List<int> indexofUnitWithForcedMovement;
    public List<int> indexofStatusWithForcedMovement;

    // Status Data
    public List<int> statusPriority;
    public List<int> statusDuration;
    public List<int> statusPrefabIndex;
    public List<int> indexOfUnitThatHasStatus;
    public List<int> indexOfActionThatHasActiveStatus;
    public List<int> statusIntData;
    public List<string> statusStringData;
    public List<bool> statusBoolData;

    // Created Field Data
    public List<createdFieldData> createdFieldsData;
    public List<int> createdFieldPriority;
    public List<int> createdFieldDuration;

    // Animated Field Data
    public List<animatedFieldData> animatedFields;
    public List<int> animatedFieldPriority;
}

[System.Serializable]
public struct ForcedMovementPathData
{
    public List<Vector2> forcedMovementPath;
    public float forcedMovementSpeed;
    public float excessForcedMovementSpeed;
    public float previousForcedMovementIterrationRate;
    public int forcedPathIndex;
    public int currentPathIndex;
    public int forcedPathPriority;
    public bool isFlying;

    public ForcedMovementPathData(List<Vector2> forcedMovementPath, float forcedMovementSpeed, float excessForcedMovementSpeed,
        float previousForcedMovementIterrationRate, int forcedPathIndex, int currentPathIndex, int forcedPathPriority, bool isFlying)
    {
        this.forcedMovementPath = forcedMovementPath;
        this.forcedMovementSpeed = forcedMovementSpeed;
        this.excessForcedMovementSpeed = excessForcedMovementSpeed;
        this.previousForcedMovementIterrationRate = previousForcedMovementIterrationRate;
        this.forcedPathIndex = forcedPathIndex;
        this.currentPathIndex = currentPathIndex;
        this.forcedPathPriority = forcedPathPriority;
        this.isFlying = isFlying;
    }
}

[System.Serializable]
public struct unitPrefabData
{
    public Vector2 position;
    public int priority;
    public int unitPrefabIndex;
    public List<int> classIndexes;
    public List<int> classLevels;
    public int health;
    public List<int> actionCooldowns;
    public List<ActionName> actionNames;

    public unitPrefabData(Vector2 position, int priority, int unitPrefabIndex, List<int> classIndexes, List<int> classLevels, int health,
        List<int> actionCooldowns, List<ActionName> actionNames)
    {
        this.position = position;
        this.priority = priority;
        this.unitPrefabIndex = unitPrefabIndex;
        this.classIndexes = classIndexes;
        this.classLevels = classLevels;
        this.health = health;
        this.actionCooldowns = actionCooldowns;
        this.actionNames = actionNames;
    }

}

[System.Serializable]
public struct createdFieldData
{
    public int createdFieldTypeIndex;
    public int originUnitIndex;

    public float createdFieldQuickness;

    public bool nonStandardDuration;
    public bool fromAnimatedField;
    public bool createdWithBlastRadius;

    public Vector2 originPosition;
    public int fieldRadius;
    public List<Vector2> createdObjectPositions;

    public createdFieldData(int createdFieldTypeIndex, int originUnitIndex, float createdFieldQuickness, bool nonStandardDuration,
        bool fromAnimatedField, bool createdWithBlastRadius, Vector2 originPosition, int fieldRadius, List<Vector2> createdObjectPositions)
    {
        this.createdFieldTypeIndex = createdFieldTypeIndex;
        this.originUnitIndex = originUnitIndex;
        this.createdFieldQuickness = createdFieldQuickness;
        this.nonStandardDuration = nonStandardDuration;
        this.fromAnimatedField = fromAnimatedField;
        this.createdWithBlastRadius = createdWithBlastRadius;
        this.originPosition = originPosition;
        this.fieldRadius = fieldRadius;
        this.createdObjectPositions = createdObjectPositions;
    }
}

[System.Serializable]
public struct animatedFieldData
{
    public int animatedFieldTypeIndex;
    public int animatedCreatedFieldTypeIndex;

    public float createdFieldQuickness;

    public List<animatedFieldNodeData> slowedNodeList;

    public Vector2 startPosition;
    public Vector2 initialDirection;
    public int range;
    public float angle;

    public int maxUnitBlastValueAbsorbtion;
    public int maxObstacleBlastValueAbsorbtion;

    public bool affectFlying;
    public bool ignoreWalls;

    public bool IsSlowInTimeFlow;

    public animatedFieldData(int animatedFieldTypeIndex, int animatedCreatedFieldTypeIndex, float createdFieldQuickness, List<animatedFieldNodeData> slowedNodeList,
        Vector2 startPosition, Vector2 initialDirection, int range, float angle, int maxUnitBlastValueAbsorbtion,
        int maxObstacleBlastValueAbsorbtion, bool affectFlying, bool ignoreWalls, bool isSlowInTimeFlow)
    {
        this.animatedFieldTypeIndex = animatedFieldTypeIndex;
        this.animatedCreatedFieldTypeIndex = animatedCreatedFieldTypeIndex;
        this.createdFieldQuickness = createdFieldQuickness;
        this.slowedNodeList = slowedNodeList;
        this.startPosition = startPosition;
        this.initialDirection = initialDirection;
        this.range = range;
        this.angle = angle;
        this.maxUnitBlastValueAbsorbtion = maxUnitBlastValueAbsorbtion;
        this.maxObstacleBlastValueAbsorbtion = maxObstacleBlastValueAbsorbtion;
        this.affectFlying = affectFlying;
        this.ignoreWalls = ignoreWalls;
        IsSlowInTimeFlow = isSlowInTimeFlow;
    }
}

[System.Serializable]
public struct animatedFieldNodeData
{
    public Vector2 position;
    public Vector2 direction;
    public int priority;
    public int blastValue;
    public float blastSpeed;
    public float affectedTimeFlow;

    public animatedFieldNodeData(Vector2 position, Vector2 direction, int priority, int blastValue, float blastSpeed, float affectedTimeFlow)
    {
        this.position = position;
        this.direction = direction;
        this.priority = priority;
        this.blastValue = blastValue;
        this.blastSpeed = blastSpeed;
        this.affectedTimeFlow = affectedTimeFlow;
    }
}
    
