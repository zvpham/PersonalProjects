using Inventory.Model;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class TileData
{
    public long lastUpdated;

    // Save Data
    public string saveToDelete;

    // Game Manager Data
    public int numberOfStatusRemoved = 0;
    public int least;
    public int index;
    public bool aUnitActed;
    public int duringTurn;

    // Unit Data
    public int numberOfUnits;
    public List<unitPrefabData> unitPrefabDatas;
    public List<double> speeds;
    public List<int> priority;

    // Player Specific Data
    public List<int> soulSlotIndexes;
    public List<int> soulIndexes;

    // Item Data
    public List<int> itemIndexes;
    public List<int> itemQuantities;
    public List<Vector3> itemLocations;

    // Status Data
    public List<int> statusPriority;
    public List<int> statusDuration;
    public List<int> statusPrefabIndex;
    public List<int> indexOfUnitThatHasStatus;
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
public struct unitForcedMovementPathData
{
    public List<Vector3> forcedMovementPath;
    public float forcedMovementSpeed;
    public float excessForcedMovementSpeed;
    public float previousForcedMovementIterrationRate;
    public int forcedPathIndex;
    public int currentPathIndex;

    public unitForcedMovementPathData(List<Vector3> forcedMovementPath, float forcedMovementSpeed, float excessForcedMovementSpeed,
        float previousForcedMovementIterrationRate, int forcedPathIndex, int currentPathIndex)
    {
        this.forcedMovementPath = forcedMovementPath;
        this.forcedMovementSpeed = forcedMovementSpeed;
        this.excessForcedMovementSpeed = excessForcedMovementSpeed;
        this.previousForcedMovementIterrationRate = previousForcedMovementIterrationRate;
        this.forcedPathIndex = forcedPathIndex;
        this.currentPathIndex = currentPathIndex;
    }
}

[System.Serializable]
public struct unitPrefabData
{
    public Vector3 position;
    public int unitPrefabIndex;
    public int health;
    public List<int> actionCooldowns;
    public List<ActionName> actionNames;
    public unitForcedMovementPathData forcedMovementPathData;

    public unitPrefabData(Vector3 position, int unitPrefabIndex, int health, List<int> actionCooldowns, List<ActionName> actionNames,
        unitForcedMovementPathData forcedMovementPathData)
    {
        this.position = position;
        this.unitPrefabIndex = unitPrefabIndex;
        this.health = health;
        this.actionCooldowns = actionCooldowns;
        this.actionNames = actionNames;
        this.forcedMovementPathData = forcedMovementPathData;
    }

}

[System.Serializable]
public struct createdFieldData
{
    public int createdFieldTypeIndex;

    public float createdFieldQuickness;

    public bool nonStandardDuration;
    public bool fromAnimatedField;
    public bool createdWithBlastRadius;

    public Vector3 originPosition;
    public int fieldRadius;
    public List<Vector3> createdObjectPositions;

    public createdFieldData(int createdFieldTypeIndex, float createdFieldQuickness, bool nonStandardDuration,
        bool fromAnimatedField, bool createdWithBlastRadius, Vector3 originPosition, int fieldRadius, List<Vector3> createdObjectPositions)
    {
        this.createdFieldTypeIndex = createdFieldTypeIndex;
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
    public int createdObjectIndex;
    public int createdObjectHolderIndex;

    public float createdFieldQuickness;

    public List<animatedFieldNodeData> slowedNodeList;

    public Vector3 startPosition;
    public Vector3 initialDirection;
    public int range;
    public float angle;

    public int maxUnitBlastValueAbsorbtion;
    public int maxObstacleBlastValueAbsorbtion;

    public bool affectFlying;
    public bool ignoreWalls;

    public bool IsSlowInTimeFlow;

    public animatedFieldData(int animatedFieldTypeIndex, int animatedCreatedFieldTypeIndex, int createdObjectIndex, int createdObjectHolderIndex, float createdFieldQuickness, List<animatedFieldNodeData> slowedNodeList,
        Vector3 startPosition, Vector3 initialDirection, int range, float angle, int maxUnitBlastValueAbsorbtion,
        int maxObstacleBlastValueAbsorbtion, bool affectFlying, bool ignoreWalls, bool isSlowInTimeFlow)
    {
        this.animatedFieldTypeIndex = animatedFieldTypeIndex;
        this.animatedCreatedFieldTypeIndex = animatedCreatedFieldTypeIndex;
        this.createdObjectIndex = createdObjectIndex;
        this.createdObjectHolderIndex = createdObjectHolderIndex;
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
    public Vector3 position;
    public Vector3 direction;
    public int priority;
    public int blastValue;
    public float blastSpeed;
    public float affectedTimeFlow;

    public animatedFieldNodeData(Vector3 position, Vector3 direction, int priority, int blastValue, float blastSpeed, float affectedTimeFlow)
    {
        this.position = position;
        this.direction = direction;
        this.priority = priority;
        this.blastValue = blastValue;
        this.blastSpeed = blastSpeed;
        this.affectedTimeFlow = affectedTimeFlow;
    }
}
    
