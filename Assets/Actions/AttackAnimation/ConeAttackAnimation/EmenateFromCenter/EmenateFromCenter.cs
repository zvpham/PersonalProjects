using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UIElements;
using System;
using System.Linq;

public class EmenateFromCenter : MonoBehaviour
{
    public List<Vector3> openListLocation;
    public List<Vector3> closedListLocation;

    public List<Node> openList = new List<Node>();
    public List<Node> closedList = new List<Node>();
    public List<GameObject> markerList = new List<GameObject>();

    public int x;
    public int y;
    public int debugTries;
    public int maxDebugTries;

    public string debugWord;

    public GameObject createdObject;
    public List<Vector3> markerLocations;

    public float secEmenateSpeed;
    public float currentTime;

    public NodeState nodeState;

    public bool isAffectFlying;
    public bool ignoreWalls;

    public GameManager gameManager;

    public Vector3 startPosition;
    public Vector3 initialDirection;
    public int range;
    public float angle;

    public int maxUnitBlastValueAbsorbtion;
    public int maxObstacleBlastValueAbsorbtion;

    public int currentRow = 0;
    public Dictionary<Vector3, List<Node>> possibleConeStates = new Dictionary<Vector3, List<Node>>();

    public bool IsSlowInTimeFlow = false;

    //public CreatedFieldEmenateFromCenter createdField;

    public event Action<int> animationEnd;

    public enum NodeState
    {
        wall,
        unit,
        emptySpace
    }

    public struct Node
    {
        public Vector3 position;
        public Vector3 direction;
        public int priority;
        public int blastValue;
        public float blastSpeed;
        public float affectedTimeFlow;
        public GameObject createdObject;

        public Node(Vector3 position, Vector3 direction, int priority, int blastValue, float blastSpeed, float affectedTimeFlow, GameObject createdobject)
        {
            this.position = position;
            this.direction = direction;
            this.priority = priority;
            this.blastValue = blastValue;
            this.blastSpeed = blastSpeed;
            this.affectedTimeFlow = affectedTimeFlow;
            this.createdObject = createdobject;
        }

        public override string ToString()
        {
            return position.ToString() + " " + priority + " " + blastSpeed;
        }
    }

    public void SetParameters(Vector3 startPosition, Vector3 direction, float angle, GameObject createdObject, int range,  List<Vector3> markerLocations, int initialConeBlastValue, int maxObstacleBlastValueAbsorbtion, int maxUnitBlastValueAbsorbtion, float secEmenateSpeed = .035f, bool isAffectFlying = true, bool ignoreWalls = true)
    {
        this.startPosition = startPosition;
        this.initialDirection = direction;
        this.angle = angle;
        this.range = range + 1;
        this.createdObject = createdObject;
        this.markerLocations = markerLocations;
        this.secEmenateSpeed = secEmenateSpeed;
        this.isAffectFlying = isAffectFlying;
        this.ignoreWalls = ignoreWalls;
        this.maxObstacleBlastValueAbsorbtion = maxObstacleBlastValueAbsorbtion;
        this.maxUnitBlastValueAbsorbtion = maxUnitBlastValueAbsorbtion;
        this.openList.Add(new Node(startPosition,  direction, 0, initialConeBlastValue, range + 1, 1, createdObject));
        this.openListLocation.Add(startPosition);
        this.currentTime = 0;
    }
    public void Start()
    {
        gameManager = GameManager.instance;
    }

    public void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= secEmenateSpeed)
        {
            if (currentRow >= 30)
            {
                animationEnd?.Invoke(0);
            }
            else
            {
                possibleConeStates.Clear();
                FindPath(ignoreWalls);
                currentRow += 1;
                Coalesce();
                if (openList.Count <= 0)
                {
                    animationEnd?.Invoke(0);
                }

            }
            currentTime = 0;
        }
    }

    private bool FindPath(bool ignoreWalls)
    {
        debugTries = 0;
        while (openList.Count > 0)  
        {
            debugTries += 1;
            if (debugTries > maxDebugTries)
            {
                break;
            }

            Node currentNode = openList[0];
            bool foundNode = false;
            foreach (Node node in openList)
            {
                if (node.priority == currentRow)
                {
                    currentNode = node;
                    foundNode = true;
                    break;
                }
            }

            if(!foundNode)
            {
                break;
            }

            openList.Remove(currentNode);
            openListLocation.Remove(currentNode.position);

            closedList.Add(currentNode);
            closedListLocation.Add(currentNode.position);
            foreach (Node neighborNode in GetNeighborList(currentNode))
            {
                if (possibleConeStates.Keys.Contains(neighborNode.position))
                {
                    possibleConeStates[neighborNode.position].Add(neighborNode);
                }
                else
                {
                    possibleConeStates[neighborNode.position] = new List<Node>() { neighborNode };
                }

            }
        }
        return false;
    }

    private List<Node> GetNeighborList(Node currentNode)
    {
        List<Node> neighborList = new List<Node>();
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (!(i == 0 && j == 0))
                {
                    Node newNode = new Node(new Vector3(currentNode.position.x + j, currentNode.position.y + i, 0), currentNode.direction, currentNode.priority + 1, currentNode.blastValue, currentNode.blastSpeed, currentNode.affectedTimeFlow, currentNode.createdObject);
                    Vector3 dirTowardsOtherObject = (newNode.position - this.startPosition).normalized;
                    float dotProduct = Vector3.Dot(dirTowardsOtherObject, newNode.direction);
                    if (!closedListLocation.Contains(newNode.position) && !openListLocation.Contains(newNode.position) && dotProduct >= this.angle && Vector3.Distance(startPosition, newNode.position) < this.range)
                    {
                        neighborList.Add(newNode);
                    }
                }
            }
        }

        return neighborList;
    }

    private void Coalesce()
    {
        bool isSpeeedChange = false;
        Node currentNode;
        int lowestBlastValue;
        float highestBlastSpeed;
        foreach(Vector3 location in possibleConeStates.Keys)
        {
            isSpeeedChange = false;
            currentNode = possibleConeStates[location][0];
            lowestBlastValue = possibleConeStates[location][0].blastValue;
            highestBlastSpeed = possibleConeStates[location][0].blastSpeed;

            foreach (Node node in possibleConeStates[location])
            {
                if(isSpeeedChange)
                {
                    if (node.blastSpeed > highestBlastSpeed)
                    {
                        highestBlastSpeed = node.blastSpeed;
                        currentNode = node;
                        isSpeeedChange = true;

                    }
                }
                else
                {
                    if (node.blastSpeed > highestBlastSpeed)
                    {
                        highestBlastSpeed = node.blastSpeed;
                        currentNode = node;
                        isSpeeedChange = true;
                        continue;

                    }
                    if (node.blastValue < lowestBlastValue)
                    {
                        lowestBlastValue = node.blastValue;
                        currentNode = node;
                    }
                }
            }

            float timeFlow = 1;
            if (gameManager.StatusFields.Count > 0)
            {
                foreach (CreatedField field in gameManager.StatusFields)
                {
                    try
                    {
                        timeFlow *= field.grid.GetGridObject(currentNode.position).timeflow;
                    }
                    catch
                    {

                    }
                }
            }

            if(timeFlow > 1)    
            {
                IsSlowInTimeFlow = true;
            }
            currentNode.blastSpeed -= 1 * timeFlow;
            currentNode.affectedTimeFlow *= timeFlow;

            if (currentNode.blastSpeed <= 0)
            {
                continue;
            }

            //detecting What the created object is hitting and how it responds to it
            nodeState = NodeState.wall;
            if (!ignoreWalls)
            {
                nodeState = IsClearToMoveToPosition(currentNode.position);
            }
            else
            {
                nodeState = IsClearToMoveToPositionIgnoreWalls(currentNode.position);
            }

            if (nodeState == NodeState.unit)
            {
                AddUnitSpace(currentNode);
            }
            else if (nodeState == NodeState.emptySpace)
            {
                AddEmptySpace(currentNode);
            }
        }
    }

    private void AddUnitSpace(Node neighborNode)
    {
        GameObject marker = Instantiate(createdObject, neighborNode.position, new Quaternion(0, 0, 0, 1f));

        if (neighborNode.blastValue - maxUnitBlastValueAbsorbtion >= 0)
        {
            neighborNode.blastValue -= maxUnitBlastValueAbsorbtion;
            marker.GetComponent<CreatedObject>().ApplyObject(1, gameManager);
            openList.Add(neighborNode);
            openListLocation.Add(neighborNode.position);
        }
        else
        {
            marker.GetComponent<CreatedObject>().ApplyObject(((float)neighborNode.blastValue / (float)maxUnitBlastValueAbsorbtion), gameManager);
            neighborNode.blastValue = 0;
            closedList.Add(neighborNode);
            closedListLocation.Add(neighborNode.position);
        }
        markerList.Add(marker);
    }

    private void AddEmptySpace(Node neighborNode)
    {
        GameObject marker = Instantiate(createdObject, neighborNode.position, new Quaternion(0, 0, 0, 1f));
        openList.Add(neighborNode);
        openListLocation.Add(neighborNode.position);
        markerList.Add(marker);
    }

    public NodeState IsClearToMoveToPosition(Vector3 position)
    {
        Vector3Int gridPosition = gameManager.groundTilemap.WorldToCell(position);
        if (!gameManager.groundTilemap.HasTile(gridPosition) || gameManager.collisionTilemap.HasTile(gridPosition))
        {
            return NodeState.wall;
        }

        Unit unit = gameManager.grid.GetGridObject((int)position.x, (int)position.y);
        if (unit != null)
        {
            return NodeState.unit;
        }

        if (isAffectFlying)
        {
            unit = gameManager.flyingGrid.GetGridObject((int)position.x, (int)position.y);
            if (unit != null)
            {
                return NodeState.unit;
            }
        }

        return NodeState.emptySpace;
    }

    public NodeState IsClearToMoveToPositionIgnoreWalls(Vector3 position)
    {
        Vector3Int gridPosition = gameManager.groundTilemap.WorldToCell(position);
        if (!gameManager.groundTilemap.HasTile(gridPosition))
        {
            return NodeState.wall;
        }

        Unit unit = gameManager.grid.GetGridObject((int)position.x, (int)position.y);
        if (unit != null)
        {
            return NodeState.unit;
        }

        if (isAffectFlying)
        {
            unit = gameManager.flyingGrid.GetGridObject((int)position.x, (int)position.y);
            if (unit != null)
            {
                return NodeState.unit;
            }
        }

        return NodeState.emptySpace;
    }

    public void DestroySelf()
    {
        List<Node> slowedNodeList = new List<Node>();
        if (IsSlowInTimeFlow)
        {
            foreach (Node node in closedList)
            {
                if(node.affectedTimeFlow == 1)
                {
                    continue;
                }
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (!(i == 0 && j == 0))
                        {
                            Node newNode = new Node(new Vector3(node.position.x + j, node.position.y + i, 0), node.direction, node.priority + 1, node.blastValue, node.blastSpeed, node.affectedTimeFlow, node.createdObject);
                            Vector3 dirTowardsOtherObject = (newNode.position - this.startPosition).normalized;
                            float dotProduct = Vector3.Dot(dirTowardsOtherObject, newNode.direction);
                            if (!closedListLocation.Contains(newNode.position) && dotProduct >= this.angle && Vector3.Distance(startPosition, newNode.position) < this.range)
                            {
                                slowedNodeList.Add(node);
                                break;
                            }
                        }
                    }
                }
            }

            foreach (Node node in slowedNodeList)
            {
                closedListLocation.Remove(node.position);
                //GameObject marker = Instantiate(createdObject, node.position, new Quaternion(0, 0, 0, 1f));
            }
            //CreatedFieldEmenateFromCenter temp = Instantiate(createdField);
            //temp.CreateSlowedConeEmenateFromCenter(gameManager, this.startPosition, this.angle, this.range, createdObject.GetComponent<CreatedObject>(), slowedNodeList, closedListLocation);
        }

        foreach(GameObject marker in markerList)
        {
            Destroy(marker);
        }
        markerList.Clear();
        
    }
}
