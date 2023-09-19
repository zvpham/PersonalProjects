/*
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.Table;

[CreateAssetMenu(menuName = "CreatedField/CreatedFieldEmenateFromCenter")]
public class CreatedFieldEmenateFromCenter : CreatedField
{
    public EmenateFromCenter animation;

    public Grid<CreatedObject> ConeGrid;
    public List<EmenateFromCenter.Node> slowedNodeList;

    public List<Vector3> openListLocation;
    public List<Vector3> closedListLocation;

    public List<EmenateFromCenter.Node> openList = new List<EmenateFromCenter.Node>();
    public List<EmenateFromCenter.Node> closedList = new List<EmenateFromCenter.Node>();
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

    public Vector3 startPosition;
    public Vector3 initialDirection;
    public int range;
    public float angle;

    public int maxUnitBlastValueAbsorbtion;
    public int maxObstacleBlastValueAbsorbtion;

    public int currentRowIndex = 0;
    public Dictionary<Vector3, List<EmenateFromCenter.Node>> possibleConeStates = new Dictionary<Vector3, List<EmenateFromCenter.Node>>();
    public List<List<EmenateFromCenter.Node>> fullExpectedConePath = new List<List<EmenateFromCenter.Node>>();
    public List<EmenateFromCenter.Node> currentRow = new List<EmenateFromCenter.Node>();

    public bool IsSlowInTimeFlow = false;

    public CreatedFieldEmenateFromCenter createdField;

    public event Action<int> animationEnd;

    public override void CreateGridOfObjects(GameManager gameManager, Vector3 originPosition, int fieldRadius, int fieldDuration)
    {
        throw new System.NotImplementedException();
    }

    public void CreateSlowedConeEmenateFromCenter(GameManager gameManager, Vector3 originPosition, float angle, int range, CreatedObject createdObject, List<EmenateFromCenter.Node> slowedNodeList, List<Vector3> closedListLocation)
    {
        List<Vector3> validLocations = new List<Vector3>();
        currentRowIndex = slowedNodeList[0].priority;
        List<EmenateFromCenter.Node> adjustedList = new List<EmenateFromCenter.Node>();
        foreach (EmenateFromCenter.Node node in slowedNodeList)
        {
            validLocations.Add(node.position);
            if (node.priority < currentRowIndex)
            {
                currentRowIndex = node.priority;
            }
            EmenateFromCenter.Node temp = node;
            temp.blastSpeed += range + 1;
            adjustedList.Add(temp);
        }

        this.openListLocation = validLocations;
        this.openList = adjustedList;
        this.closedListLocation = closedListLocation;

        this.gameManager = gameManager;
        this.startPosition = originPosition;
        this.angle = angle;
        this.range = range;
        this.slowedNodeList = slowedNodeList;
        this.closedListLocation = closedListLocation;
        this.ConeGrid = new Grid<CreatedObject>(range * 2 - 1, range * 2 - 1, 1f, originPosition + new Vector3(-range - 1, -range - 1, 0), (Grid<CreatedObject> g, int x, int y) => createdObject.CreateObject(g, x, y, validLocations));

        for (int i  = currentRowIndex; i < range; i++)
        {
            possibleConeStates.Clear();
            FindPath(false);
            currentRowIndex += 1;
            CoalesceExpectedField();
            List<EmenateFromCenter.Node> temp = currentRow;
            fullExpectedConePath.Add(new List<EmenateFromCenter.Node>(currentRow.Count()));
            foreach(EmenateFromCenter.Node node in currentRow)
            {
                fullExpectedConePath[fullExpectedConePath.Count - 1].Add(node);
            }
            currentRow.Clear();
            if (openList.Count <= 0)
            {
                break;
            }
        }
        Debug.Log("This is Row Count " + fullExpectedConePath.Count());
        for(int i = 0; i < fullExpectedConePath.Count(); i++)
        {
            Debug.Log("Row Number " + i);
            foreach (EmenateFromCenter.Node node in fullExpectedConePath[i])
            {
                Debug.Log(node.position);
            }
        }
        this.gameManager.expectedBlastPaths.Add(fullExpectedConePath);
        this.gameManager.expectedBlastRowNumber.Add(0);
    }

    public enum NodeState
    {
        wall,
        unit,
        emptySpace
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
            EmenateFromCenter.Node currentNode = openList[0];
            bool foundNode = false;
            foreach (EmenateFromCenter.Node node in openList)
            {
                if (node.priority == currentRowIndex)
                {
                    currentNode = node;
                    foundNode = true;
                    break;
                }
            }

            if (!foundNode)
            {
                break;
            }
            openList.Remove(currentNode);
            openListLocation.Remove(currentNode.position);

            closedList.Add(currentNode);
            closedListLocation.Add(currentNode.position);
            foreach (EmenateFromCenter.Node neighborNode in GetNeighborList(currentNode))
            {
                //Debug.Log("HI is anytone there 4 " + neighborNode.position);
                if (possibleConeStates.Keys.Contains(neighborNode.position)) 
                {
                    possibleConeStates[neighborNode.position].Add(neighborNode);
                }
                else
                {
                    possibleConeStates[neighborNode.position] = new List<EmenateFromCenter.Node>() { neighborNode };
                }

            }
        }
        return false;
    }

    private List<EmenateFromCenter.Node> GetNeighborList(EmenateFromCenter.Node currentNode)
    {
        List<EmenateFromCenter.Node> neighborList = new List<EmenateFromCenter.Node>();
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (!(i == 0 && j == 0))
                {
                    EmenateFromCenter.Node newNode = new EmenateFromCenter.Node(new Vector3(currentNode.position.x + j, currentNode.position.y + i, 0), currentNode.direction, currentNode.priority + 1, currentNode.blastValue, currentNode.blastSpeed, currentNode.affectedTimeFlow, currentNode.createdObject);
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

    private void CoalesceExpectedField()
    {
        bool isSpeeedChange = false;
        EmenateFromCenter.Node currentNode;
        int lowestBlastValue;
        float highestBlastSpeed;
        foreach (Vector3 location in possibleConeStates.Keys)
        {
            isSpeeedChange = false;
            currentNode = possibleConeStates[location][0];
            lowestBlastValue = possibleConeStates[location][0].blastValue;
            highestBlastSpeed = possibleConeStates[location][0].blastSpeed;

            foreach (EmenateFromCenter.Node node in possibleConeStates[location])
            {
                if (isSpeeedChange)
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

            if (timeFlow > 1)
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
                AddUnitSpaceAnimation(currentNode);
            }
            else if (nodeState == NodeState.emptySpace)
            {
                AddEmptySpaceAnimation(currentNode);
            }
        }
    }

    private void AddUnitSpaceAnimation(EmenateFromCenter.Node neighborNode)
    {;
        currentRow.Add(neighborNode);
        if (neighborNode.blastValue - maxUnitBlastValueAbsorbtion >= 0)
        {
            neighborNode.blastValue -= maxUnitBlastValueAbsorbtion;
            openList.Add(neighborNode);
            openListLocation.Add(neighborNode.position);

        }
        else
        {
            neighborNode.blastValue = 0;
            closedList.Add(neighborNode);
            closedListLocation.Add(neighborNode.position);
        }
    }

    private void AddEmptySpaceAnimation(EmenateFromCenter.Node neighborNode)
    {
        openList.Add(neighborNode);
        openListLocation.Add(neighborNode.position);
        currentRow.Add(neighborNode);
    }

    private void Coalesce()
    {
        bool isSpeeedChange = false;
        EmenateFromCenter.Node currentNode;
        int lowestBlastValue;
        float highestBlastSpeed;
        foreach (Vector3 location in possibleConeStates.Keys)
        {
            isSpeeedChange = false;
            currentNode = possibleConeStates[location][0];
            lowestBlastValue = possibleConeStates[location][0].blastValue;
            highestBlastSpeed = possibleConeStates[location][0].blastSpeed;

            foreach (EmenateFromCenter.Node node in possibleConeStates[location])
            {
                if (isSpeeedChange)
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

            if (timeFlow > 1)
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

    private void AddUnitSpace(EmenateFromCenter.Node neighborNode)
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

    private void AddEmptySpace(EmenateFromCenter.Node neighborNode)
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
        List<EmenateFromCenter.Node> slowedNodeList = new List<EmenateFromCenter.Node>();
        if (IsSlowInTimeFlow)
        {
            foreach (EmenateFromCenter.Node node in closedList)
            {
                if (node.affectedTimeFlow == 1)
                {
                    continue;
                }
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (!(i == 0 && j == 0))
                        {
                            EmenateFromCenter.Node newNode = new EmenateFromCenter.Node(new Vector3(node.position.x + j, node.position.y + i, 0), node.direction, node.priority + 1, node.blastValue, node.blastSpeed, node.affectedTimeFlow, node.createdObject);
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

            foreach (EmenateFromCenter.Node node in slowedNodeList)
            {
                closedListLocation.Remove(node.position);
                GameObject marker = Instantiate(createdObject, node.position, new Quaternion(0, 0, 0, 1f));
            }
            CreatedFieldEmenateFromCenter temp = Instantiate(createdField);
            temp.CreateSlowedConeEmenateFromCenter(gameManager, this.startPosition, this.angle, this.range, createdObject.GetComponent<CreatedObject>(), slowedNodeList, closedListLocation);
        }

        foreach (GameObject marker in markerList)
        {
            Destroy(marker);
        }
        markerList.Clear();

    }

}
*/