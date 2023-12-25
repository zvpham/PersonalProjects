using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;
using static UnityEditor.FilePathAttribute;

public class EmenateFromCenterField : AnimatedField
{
    public override void SetParameters(GameManager gameManager, Vector3 startPosition, Vector3 direction, float angle, CreatedObject createdObject,
        CreatedField createdField, int range, int initialConeBlastValue, int maxObstacleBlastValueAbsorbtion,
        int maxUnitBlastValueAbsorbtion, float secEmenateSpeed = 0.035F, bool isAffectFlying = true, bool ignoreWalls = true)
    {
        this.gameManager = gameManager;
        this.startPosition = startPosition;
        this.initialDirection = direction;
        this.angle = angle;
        this.range = range + 1;
        this.createdField = Instantiate(createdField);
        this.createdObject = createdObject;
        this.createdFieldType = createdField;
        this.secEmenateSpeed = secEmenateSpeed;
        this.affectFlying = isAffectFlying;
        this.ignoreWalls = ignoreWalls;
        this.maxObstacleBlastValueAbsorbtion = maxObstacleBlastValueAbsorbtion;
        this.maxUnitBlastValueAbsorbtion = maxUnitBlastValueAbsorbtion;
        this.openList.Add(new Node(startPosition, direction, 0, initialConeBlastValue, range + 1, 1, createdObject, this.createdField.createdObjectPrefab.GetComponent<SpriteRenderer>().sprite));
        this.openListLocation.Add(startPosition);
        this.currentTime = 0;
    }

    public void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= secEmenateSpeed)
        {
            if (currentRowIndex >= 30)
            {
                AnimationEnd();
            }
            else
            {
                possibleConeStates.Clear();
                FindPath(ignoreWalls);
                currentRowIndex += 1;
                Coalesce();
                if (openList.Count <= 0)
                {
                    AnimationEnd();
                    DestroySelf();
                }

            }
            currentTime = 0;
        }
    }

    public override void Activate()
    {
        enabled = true;
        List<Vector3> validLocations = new List<Vector3>();
        currentRowIndex = slowedNodeList[0].priority;
        List<Node> adjustedList = new List<Node>();
        foreach (Node node in slowedNodeList)
        {
            validLocations.Add(node.position);
            if (node.priority < currentRowIndex)
            {
                currentRowIndex = node.priority;
            }
            Node temp = node;
            temp.blastSpeed += range + 1;
            adjustedList.Add(temp);
        }
            
        this.openListLocation = validLocations;
        this.openList = adjustedList;
        this.closedList.Clear();
        this.closedListLocation.Clear();
    }

    public override void GetAnimation(bool isLoading = false)
    {
        enabled = false;
        List<Vector3> validLocations = new List<Vector3>();
        currentRowIndex = slowedNodeList[0].priority;
        List<Node> adjustedList = new List<Node>();
        foreach (Node node in slowedNodeList)
        {
            validLocations.Add(node.position);
            if (node.priority < currentRowIndex)
            {
                currentRowIndex = node.priority;
            }
            Node temp = node;
            temp.blastSpeed += range + 1;
            adjustedList.Add(temp);
        }

        this.openListLocation = validLocations;
        this.openList = adjustedList;
        this.createdField.CreateGridOfObjects(gameManager, new Grid<CreatedObject>(range * 2 - 1, range * 2 - 1, 1f,
            startPosition + new Vector3(-range - 1, -range - 1, 0), (Grid<CreatedObject> g, int x, int y) => 
            this.createdField.CreateCreatedObject(g, x, y, validLocations)), 10, isLoading);
        this.createdField.fromAnimatedField = true;
        for (int i = currentRowIndex; i < range; i++)
        {
            possibleConeStates.Clear();
            FindPath(false);
            currentRowIndex += 1;
            CoalesceExpectedField();
            List<Node> temp = currentRow;
            fullExpectedConePath.Add(new List<Node>(currentRow.Count()));
            foreach (Node node in currentRow)
            {
                fullExpectedConePath[fullExpectedConePath.Count - 1].Add(node);
            }
            currentRow.Clear();
            if (openList.Count <= 0)
            {
                break;
            }
        }
                if(!gameManager.animatedFields.Contains(this))
        {
            if (!isLoading)
            {
                animatedFieldPriority = (int)(createdFieldQuickness * gameManager.baseTurnTime) + gameManager.mainGameManger.least;
            }
            this.gameManager.animatedFields.Add(this);
            this.gameManager.mainGameManger.animatedFields.Add(this);
            this.gameManager.expectedBlastPaths.Add(fullExpectedConePath);
            this.gameManager.expectedBlastRowNumber.Add(0);
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
            foreach (Node neighborNode in GetNeighborList(currentNode))
            {
                //Debug.Log("HI is anytone there 4 " + neighborNode.position);
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
                    Node newNode = new Node(new Vector3(currentNode.position.x + j, currentNode.position.y + i, 0), currentNode.direction, currentNode.priority + 1, currentNode.blastValue, currentNode.blastSpeed, currentNode.affectedTimeFlow, currentNode.createdObject, currentNode.createdObjectSprite);
                    Vector3 dirTowardsOtherObjectFromStartPosition = (newNode.position - this.startPosition).normalized;
                    float dotProduct = Vector3.Dot(dirTowardsOtherObjectFromStartPosition, newNode.direction);

                    Vector3 dirTowardsOtherObjectFromCurrentPosition = (newNode.position - currentNode.position).normalized;
                    float dotProduct2 = Vector3.Dot(dirTowardsOtherObjectFromCurrentPosition, newNode.direction);

                    Vector3Int gridPosition = gameManager.groundTilemap.WorldToCell(newNode.position);
                    if (!closedListLocation.Contains(newNode.position) && !openListLocation.Contains(newNode.position) &&
                        dotProduct >= this.angle && dotProduct2 >= .350f 
                        && Vector3.Distance(startPosition, newNode.position) < this.range &&
                        gameManager.groundTilemap.HasTile(gridPosition))
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
        Node currentNode;
        int lowestBlastValue;
        float highestBlastSpeed;
        foreach (Vector3 location in possibleConeStates.Keys)
        {
            isSpeeedChange = false;
            currentNode = possibleConeStates[location][0];
            lowestBlastValue = possibleConeStates[location][0].blastValue;
            highestBlastSpeed = possibleConeStates[location][0].blastSpeed;

            foreach (Node node in possibleConeStates[location])
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
            if (gameManager.createdFields.Count > 0)
            {
                foreach (CreatedField field in gameManager.createdFields)
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
            else
            {
                AddWallSpaceAnimation(currentNode);
            }
        }
    }

    private void AddUnitSpaceAnimation(Node neighborNode)
    {
        currentRow.Add(neighborNode);
        if (neighborNode.blastValue - maxUnitBlastValueAbsorbtion > 0)
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

    private void AddWallSpaceAnimation(Node neighborNode)
    {
        currentRow.Add(neighborNode);
        if (neighborNode.blastValue - maxObstacleBlastValueAbsorbtion > 0)
        {
            neighborNode.blastValue -= maxObstacleBlastValueAbsorbtion;
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

    private void AddEmptySpaceAnimation(Node neighborNode)
    {
        openList.Add(neighborNode);
        openListLocation.Add(neighborNode.position);
        currentRow.Add(neighborNode);
    }

    private void Coalesce()
    {
        bool isSpeeedChange = false;
        Node currentNode;
        int lowestBlastValue;
        float highestBlastSpeed;
        foreach (Vector3 location in possibleConeStates.Keys)
        {
            isSpeeedChange = false;
            currentNode = possibleConeStates[location][0];
            lowestBlastValue = possibleConeStates[location][0].blastValue;
            highestBlastSpeed = possibleConeStates[location][0].blastSpeed;

            foreach (Node node in possibleConeStates[location])
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
            if (gameManager.createdFields.Count > 0)
            {
                foreach (CreatedField field in gameManager.createdFields)
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
            else
            {
                AddWallSpace(currentNode);
            }
        }
    }

    private void AddUnitSpace(Node neighborNode)
    {
        GameObject marker = Instantiate(createdField.createdObjectPrefab, neighborNode.position, new Quaternion(0, 0, 0, 1f));
        Debug.Log("Fire Damage Unit: " + neighborNode.position);
        if (neighborNode.blastValue - maxUnitBlastValueAbsorbtion > 0)
        {
            neighborNode.blastValue -= maxUnitBlastValueAbsorbtion;
            createdField.ApplyObject(1, gameManager, neighborNode.position);
            openList.Add(neighborNode);
            openListLocation.Add(neighborNode.position);
        }
        else
        {
            createdField.ApplyObject(((float)neighborNode.blastValue / (float)maxUnitBlastValueAbsorbtion), gameManager, neighborNode.position);
            neighborNode.blastValue = 0;
            closedList.Add(neighborNode);
            closedListLocation.Add(neighborNode.position);
        }
        markerList.Add(marker);
    }

    private void AddEmptySpace(Node neighborNode)
    {
        Debug.Log("Fire Damage Empty: " + neighborNode.position);
        GameObject marker = Instantiate(createdField.createdObjectPrefab, neighborNode.position, new Quaternion(0, 0, 0, 1f));
        openList.Add(neighborNode);
        openListLocation.Add(neighborNode.position);
        markerList.Add(marker);
    }

    private void AddWallSpace(Node neighborNode)
    {
        GameObject marker =  Instantiate(createdField.createdObjectPrefab, neighborNode.position, new Quaternion(0, 0, 0, 1f));
        Debug.Log("Fire Damage Wall: " + neighborNode.position);
        if (neighborNode.blastValue - maxObstacleBlastValueAbsorbtion > 0)
        {
            neighborNode.blastValue -= maxObstacleBlastValueAbsorbtion;
            createdField.ApplyObject(1, gameManager, neighborNode.position);
            openList.Add(neighborNode);
            openListLocation.Add(neighborNode.position);
        }
        else
        {
            createdField.ApplyObject(((float)neighborNode.blastValue / (float)maxObstacleBlastValueAbsorbtion), gameManager, neighborNode.position);
            neighborNode.blastValue = 0;
            closedList.Add(neighborNode);
            closedListLocation.Add(neighborNode.position);
        }
        markerList.Add(marker);
    }

    public NodeState IsClearToMoveToPosition(Vector3 position)
    {
        Vector3Int gridPosition = gameManager.groundTilemap.WorldToCell(position);
        if (!gameManager.groundTilemap.HasTile(gridPosition) || (gameManager.obstacleGrid.GetGridObject(position) != null && gameManager.obstacleGrid.GetGridObject(position).blockMovement == true))
        {
            return NodeState.wall;
        }

        Unit unit = gameManager.grid.GetGridObject(position);
        if (unit != null)
        {
            return NodeState.unit;
        }

        if (affectFlying)
        {
            unit = gameManager.flyingGrid.GetGridObject(position);
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

        Unit unit = gameManager.grid.GetGridObject(position);
        if (unit != null)
        {
            return NodeState.unit;
        }

        if (affectFlying)
        {
            unit = gameManager.flyingGrid.GetGridObject(position);
            if (unit != null)
            {
                return NodeState.unit;
            }
        }

        return NodeState.emptySpace;
    }

    public override void DestroySelf()
    {
        bool isFinished = true;
        if (IsSlowInTimeFlow)
        {
            slowedNodeList.Clear();
            foreach (Node node in closedList)
            {
                if (node.affectedTimeFlow == 1 || node.blastValue <= 0)
                {
                    continue;
                }
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (!(i == 0 && j == 0))
                        {
                            Node newNode = new Node(new Vector3(node.position.x + j, node.position.y + i, 0), node.direction, node.priority + 1, node.blastValue, node.blastSpeed, node.affectedTimeFlow, node.createdObject, node.createdObjectSprite);
                            
                            Vector3 dirTowardsOtherObject = (newNode.position - this.startPosition).normalized;
                            float dotProduct = Vector3.Dot(dirTowardsOtherObject, newNode.direction);

                            Vector3 dirTowardsOtherObjectFromCurrentPosition = (newNode.position - node.position).normalized;
                            float dotProduct2 = Vector3.Dot(dirTowardsOtherObjectFromCurrentPosition, newNode.direction);

                            Vector3Int gridPosition = gameManager.groundTilemap.WorldToCell(newNode.position);
                            if (!closedListLocation.Contains(newNode.position) && dotProduct >= this.angle && dotProduct2 >= .350f && Vector3.Distance(startPosition, newNode.position) < this.range && gameManager.groundTilemap.HasTile(gridPosition))
                            {
                                slowedNodeList.Add(node);
                                break;
                            }       
                        }
                    }
                }
            }
            if(slowedNodeList.Count > 0)
            {
                foreach (Node node in slowedNodeList)
                {
                    closedListLocation.Remove(node.position);
                }
                GetAnimation();
                isFinished = false;
                IsSlowInTimeFlow = false;
            }
        }

        foreach (GameObject marker in markerList)
        {
            Destroy(marker);
        }
        markerList.Clear();
        if(isFinished)
        {
            if (gameManager.animatedFields.Contains(this))
            {
                int index = gameManager.animatedFields.IndexOf(this);
                gameManager.animatedFields.RemoveAt(index);
                index = gameManager.mainGameManger.animatedFields.IndexOf(this);
                gameManager.mainGameManger.animatedFields.RemoveAt(index);
                gameManager.expectedBlastPaths.RemoveAt(index);
                gameManager.expectedBlastRowNumber.RemoveAt(index);

                if(this.createdField != null)
                {
                    this.createdField.RemoveStatusOnDeletion();
                }
            }
            Destroy(this.gameObject);
        }

    }

}
