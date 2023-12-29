using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class AnimatedField : MonoBehaviour
{
    public int animatedFieldTypeIndex;
    public int animatedFieldPriority;
    public float createdFieldQuickness = 1;

    public List<Node> slowedNodeList = new List<Node>();

    public List<Vector3> openListLocation;
    public List<Vector3> closedListLocation;

    public List<Node> openList = new List<Node>();
    public List<Node> closedList = new List<Node>();

    public Vector3 startPosition;
    public Vector3 initialDirection;
    public int range;
    public float angle;

    public int maxUnitBlastValueAbsorbtion;
    public int maxObstacleBlastValueAbsorbtion;

    public bool affectFlying;
    public bool ignoreWalls;

    public bool IsSlowInTimeFlow = false;

    public CreatedField createdField;
    public CreatedField createdFieldType;

    public List<GameObject> markerList = new List<GameObject>();
    public List<Node> currentRow = new List<Node>();

    public int currentRowIndex = 0;

    public Dictionary<Vector3, List<Node>> possibleConeStates = new Dictionary<Vector3, List<Node>>();
    public List<List<Node>> fullExpectedConePath = new List<List<Node>>();

    //Variables that are set in Prefab
    public NodeState nodeState;

    public int debugTries;
    public int maxDebugTries;

    public float secEmenateSpeed;
    public float currentTime;

    public GameManager gameManager;

    public event Action<int> animationEnd;

    public struct Node
    {
        public Vector3 position;
        public Vector3 direction;
        public int priority;
        public int blastValue;
        public float blastSpeed;
        public float affectedTimeFlow;
        public Sprite createdObjectSprite;

        public Node(Vector3 position, Vector3 direction, int priority, int blastValue, float blastSpeed, float affectedTimeFlow, Sprite createdObjectSprite)
        {
            this.position = position;
            this.direction = direction;
            this.priority = priority;
            this.blastValue = blastValue;
            this.blastSpeed = blastSpeed;
            this.affectedTimeFlow = affectedTimeFlow;
            this.createdObjectSprite = createdObjectSprite;

        }

        public Node(animatedFieldNodeData node, Sprite createdObjectSprite)
        {
            this.position = node.position;
            this.direction = node.direction;
            this.priority = node.priority;
            this.blastValue = node.blastValue;
            this.blastSpeed = node.blastSpeed;
            this.affectedTimeFlow = node.affectedTimeFlow;
            this.createdObjectSprite = createdObjectSprite;
        }

        public override string ToString()
        {
            return position.ToString() + " " + priority + " " + blastSpeed;
        }
    }

    public enum NodeState
    {
        wall,
        unit,
        emptySpace
    }

    abstract public void SetParameters(GameManager gameManager, Vector3 startPosition, Vector3 direction, float angle, CreatedField createdField, int range,
        int initialConeBlastValue, int maxObstacleBlastValueAbsorbtion, int maxUnitBlastValueAbsorbtion,
        float secEmenateSpeed = .035f, bool isAffectFlying = true, bool ignoreWalls = true);

    abstract public void GetAnimation(bool isLoading = false);

    abstract public void Activate();

    public void AnimationEnd()
    {
        animationEnd?.Invoke(0);
    }

    public void onLoad(animatedFieldData animatedFieldData, ResourceManager resourceManager, GameManager gameManager, Vector3 newDefaultGridPosition)
    {
        this.createdFieldQuickness = animatedFieldData.createdFieldQuickness;
        Debug.LogError(animatedFieldData.startPosition);
        this.startPosition = animatedFieldData.startPosition + (Vector2)newDefaultGridPosition;
        this.initialDirection = animatedFieldData.initialDirection;
        this.angle = animatedFieldData.angle;
        this.range = animatedFieldData.range;
        this.createdFieldType = resourceManager.createdFields[animatedFieldData.animatedCreatedFieldTypeIndex];
        this.createdField = Instantiate(createdFieldType);
        this.affectFlying = animatedFieldData.affectFlying;
        this.ignoreWalls = animatedFieldData.ignoreWalls;
        this.maxObstacleBlastValueAbsorbtion = animatedFieldData.maxObstacleBlastValueAbsorbtion;
        this.maxUnitBlastValueAbsorbtion = animatedFieldData.maxUnitBlastValueAbsorbtion;
        this.IsSlowInTimeFlow = animatedFieldData.IsSlowInTimeFlow;
        this.currentTime = 0;
        this.gameManager = gameManager;
        foreach(animatedFieldNodeData node in animatedFieldData.slowedNodeList)
        {
            Node slowedNode = new Node(node, this.createdField.createdObjectPrefab.GetComponent<SpriteRenderer>().sprite);
            slowedNode.position = slowedNode.position + newDefaultGridPosition;
            slowedNodeList.Add(slowedNode);
        }
        GetAnimation(true);
    }

    abstract public void DestroySelf();
}
