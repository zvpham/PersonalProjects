using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class DijkstraMap
{
    private GridHex<DijkstraMapNode> grid;
    private List<DijkstraMapNode> openList;
    private List<DijkstraMapNode> closedList;

    public DijkstraMap(int width, int height, float cellSize,  Vector3 orginPosition, bool debug)
    {
        grid = new GridHex<DijkstraMapNode>(width, height, cellSize, orginPosition, (GridHex<DijkstraMapNode> g, int x, int y) => new DijkstraMapNode(g, x, y), debug);
    }

    public DijkstraMapNode GetLowestNearbyNode(int x, int y)
    {
        DijkstraMapNode lowestNode = grid.GetGridObject(x, y);
        int lowestValue = lowestNode.value;
        foreach (DijkstraMapNode neighborNode in GetNeighborList(lowestNode))
        {
            if(neighborNode.value < lowestValue)
            {
                lowestNode = neighborNode;
                lowestValue = neighborNode.value;
            }
        }
        return lowestNode;
    }


    public DijkstraMapNode GetLowestNearbyNode(int x, int y, MoveModifier moveModifier, CombatGameManager gameManager)
    {
        DijkstraMapNode lowestNode;
        DijkstraMapNode currentNode = grid.GetGridObject(x, y);
        lowestNode = currentNode;
        int lowestValue = currentNode.value;
        foreach (DijkstraMapNode neighborNode in GetNeighborList(currentNode))
        {
            if (neighborNode.value < lowestValue && moveModifier.ValidMove(gameManager, currentNode, neighborNode))
            {
                lowestNode = neighborNode;
                lowestValue = neighborNode.value;
            }
        }
        return lowestNode;
    }

    // targetPosition is where the object moving Wants to go / Final end Posotiion
    public DijkstraMapNode GetLowestNearbyNode(int x, int y, Vector2Int targetPosition, MoveModifier moveModifier, 
        CombatGameManager gameManager)
    {
        DijkstraMapNode lowestNode;
        DijkstraMapNode currentNode = grid.GetGridObject(x, y);
        lowestNode = currentNode;
        int lowestValue = currentNode.value;
        foreach (DijkstraMapNode neighborNode in GetNeighborList(currentNode))
        {
            if (neighborNode.value < lowestValue && moveModifier.ValidMove(gameManager, currentNode, neighborNode) && (!neighborNode.endPositionOnly || 
                targetPosition == new Vector2Int(neighborNode.x, neighborNode.y)))
            {
                lowestNode = neighborNode;
                lowestValue = neighborNode.value;
            }
        }
        return lowestNode;
    }


    public List<DijkstraMapNode> GetNodesInMovementRangeNoChangeGrid(int x, int y, int initialMoveValue)
    {
        int startNodeValue = grid.GetGridObject(x, y).value;
        openList = new List<DijkstraMapNode>();
        closedList = new List<DijkstraMapNode>();
        List<DijkstraMapNode> nodesInRange = new List<DijkstraMapNode>();
        if(grid.GetGridObject(x, y).value == int.MaxValue)
        {
            Debug.Log("Startvalue is Max Value");
            return null;
        }

        openList.Add(grid.GetGridObject(x, y));
        while (openList.Count > 0)
        {
            DijkstraMapNode currentNode = openList[0];
            openList.Remove(currentNode);

            foreach (DijkstraMapNode neighborNode in GetNeighborList(currentNode))
            {
                int nodeDifference = Mathf.Abs(neighborNode.value - startNodeValue);
                if (nodeDifference <= initialMoveValue && !closedList.Contains(neighborNode) && !openList.Contains(neighborNode))
                {
                    nodesInRange.Add(neighborNode);     
                    openList.Add(neighborNode);
                }
            }
            closedList.Add(currentNode);
        }

        return nodesInRange;
    }

    // start value should be  is -1
    public List<DijkstraMapNode> GetNodesInMovementRange(int x, int y, int initialMoveValue, MoveModifier moveModifier,
        CombatGameManager gameManager, int walkCostOveride = -1, int[,] walkCostGridOveride = null)
    {
        openList = new List<DijkstraMapNode>();
        closedList = new List<DijkstraMapNode>();
        List<DijkstraMapNode> nodesInMovementRange = new List<DijkstraMapNode>();

        DijkstraMapNode newNode = grid.GetGridObject(x, y);
        if (newNode != null)
        {
            newNode.value = initialMoveValue;
            grid.SetGridObject(newNode.x, newNode.y, newNode);
            openList.Add(newNode);
        }

        int[,] walkCostGrid = walkCostGridOveride;
        if (walkCostGrid == null)
        {
            walkCostGrid = gameManager.moveCostMap;
        }

        if (walkCostOveride == -1)
        {
            while (openList.Count > 0)
            {
                DijkstraMapNode currentNode = GetHighestValue(openList);
                openList.Remove(currentNode);
                foreach (DijkstraMapNode neighborNode in GetNeighborList(currentNode))
                {
                    if (moveModifier.CheckIfHexIsInMovementRange(gameManager, currentNode, neighborNode,
                        walkCostGrid[neighborNode.y, neighborNode.x]))
                    {
                        neighborNode.value = currentNode.value - walkCostGrid[neighborNode.y, neighborNode.x];
                        openList.Add(neighborNode);
                    }
                }
                nodesInMovementRange.Add(currentNode);
                closedList.Add(currentNode);
            }
        }
        else
        {
            while (openList.Count > 0)
            {
                DijkstraMapNode currentNode = GetHighestValue(openList);
                openList.Remove(currentNode);
                foreach (DijkstraMapNode neighborNode in GetNeighborList(currentNode))
                {
                    if (moveModifier.CheckIfHexIsInMovementRange(gameManager, currentNode, neighborNode,
                        walkCostOveride))
                    {
                        neighborNode.value = currentNode.value - walkCostOveride;
                        openList.Add(neighborNode);
                    }
                }
                nodesInMovementRange.Add(currentNode);
                closedList.Add(currentNode);
            }
        }
        return nodesInMovementRange;
    }

    // creates a grid where starting position is highest nodeValue and value decreases as grid spreads to walkable tiles
    public List<DijkstraMapNode> GetNodesInMovementRange(int x, int y, int initialMoveValue, MoveModifier moveModifier,
    CombatGameManager gameManager, bool[,] badWalkinPassiveEffects, int walkCostOveride = -1, int[,] walkCostGridOveride = null)
    {
        openList = new List<DijkstraMapNode>();
        closedList = new List<DijkstraMapNode>();
        List<DijkstraMapNode> nodesInMovementRange = new List<DijkstraMapNode>();

        DijkstraMapNode newNode = grid.GetGridObject(x, y);
        if (newNode != null)
        {
            newNode.value = initialMoveValue;
            grid.SetGridObject(newNode.x, newNode.y, newNode);
            openList.Add(newNode);
        }

        int[,] walkCostGrid = walkCostGridOveride;
        if (walkCostGrid == null)
        {
            walkCostGrid = gameManager.moveCostMap;
        }

        if (walkCostOveride == -1)
        {
            while (openList.Count > 0)
            {
                DijkstraMapNode currentNode = GetHighestValue(openList);
                openList.Remove(currentNode);
                foreach (DijkstraMapNode neighborNode in GetNeighborList(currentNode))
                {
                    if (!currentNode.endPositionOnly && moveModifier.CheckIfHexIsInMovementRange(gameManager, currentNode, neighborNode,
                        walkCostGrid[neighborNode.y, neighborNode.x]))
                    {
                        neighborNode.value = currentNode.value - walkCostGrid[neighborNode.y, neighborNode.x];
                        if(badWalkinPassiveEffects[neighborNode.x, neighborNode.y])
                        {
                            neighborNode.endPositionOnly = true;
                        }
                        openList.Add(neighborNode);
                    }
                }
                nodesInMovementRange.Add(currentNode);
                closedList.Add(currentNode);
            }
        }
        else
        {
            while (openList.Count > 0)
            {
                DijkstraMapNode currentNode = GetHighestValue(openList);
                openList.Remove(currentNode);
                foreach (DijkstraMapNode neighborNode in GetNeighborList(currentNode))
                {
                    if (!currentNode.endPositionOnly && moveModifier.CheckIfHexIsInMovementRange(gameManager, currentNode, neighborNode,
                        walkCostOveride))
                    {
                        neighborNode.value = currentNode.value - walkCostOveride;
                        if (badWalkinPassiveEffects[neighborNode.x, neighborNode.y])
                        {
                            neighborNode.endPositionOnly = true;
                        }
                        openList.Add(neighborNode);
                    }
                }
                nodesInMovementRange.Add(currentNode);
                closedList.Add(currentNode);
            }
        }
        return nodesInMovementRange;
    }

    public void SetGoals(List<Vector2Int> goals)
    {
        if(goals.Count == 0)
        {
            Debug.LogWarning("Setting Goals without having any goals");
        }

        openList = new List<DijkstraMapNode> ();
        closedList = new List<DijkstraMapNode> ();

        for(int i = 0; i < goals.Count; i++)
        {
            DijkstraMapNode newNode = grid.GetGridObject(goals[i].x, goals[i].y);
            if(newNode != null)
            {
                newNode.value = 0;
                grid.SetGridObject(newNode.x, newNode.y, newNode);
                openList.Add(newNode);
            }

        }


        while(openList.Count > 0)
        {
            DijkstraMapNode currentNode =  GetLowestValue(openList);
            openList.Remove(currentNode);
            foreach(DijkstraMapNode neighborNode in GetNeighborList(currentNode))
            {
                if(currentNode.value + 1 <  neighborNode.value && neighborNode.walkable)
                {
                    neighborNode.value = currentNode.value + 1;
                    grid.SetGridObject(neighborNode.x, neighborNode.y, neighborNode);
                    openList.Add (neighborNode);
                }
            }
            closedList.Add(currentNode);
        }

        for (int i = 0; i < grid.GetHeight(); i++)
        {
            for (int j = 0; j < grid.GetWidth(); j++)
            {
                DijkstraMapNode tempNode = grid.GetGridObject(j, i);
                if(tempNode.walkable == false)
                {
                    tempNode.value = int.MaxValue;
                    grid.SetGridObject(j, i, tempNode);
                }
            }
        }
    }

    // goal is 0, set tiles to max before calling this
    public void SetGoalsNew(List<Vector2Int> goals, CombatGameManager gameManager, MoveModifier moveModifier,
        int walkCostOveride = -1, int[,] walkCostGridOveride = null, bool debug = false)
    {
        if (goals.Count == 0)
        {
            Debug.LogWarning("Setting Goals without having any goals");
        }

        openList = new List<DijkstraMapNode>();
        closedList = new List<DijkstraMapNode>();

        for (int i = 0; i < goals.Count; i++)
        {
            DijkstraMapNode newNode = grid.GetGridObject(goals[i].x, goals[i].y);
            if (newNode != null)
            {
                newNode.value = 0;
                grid.SetGridObject(newNode.x, newNode.y, newNode);
                openList.Add(newNode);
            }

        }

        int[,] walkCostGrid = walkCostGridOveride;
        if(walkCostGrid == null)
        {
            walkCostGrid = gameManager.moveCostMap;
        }
        if(walkCostOveride == -1)
        {
            while (openList.Count > 0)
            {   
                DijkstraMapNode currentNode = GetLowestValue(openList);
                openList.Remove(currentNode);
                foreach (DijkstraMapNode neighborNode in GetNeighborList(currentNode))
                {
                    if(debug)
                    {
                        Debug.Log(neighborNode.x + ", " + neighborNode.y + " : "  + moveModifier.ValidMovePosition(gameManager, currentNode, neighborNode,
                        walkCostGrid[neighborNode.y, neighborNode.x], true));
                    }
                    if (moveModifier.ValidMovePosition(gameManager, currentNode, neighborNode, 
                        walkCostGrid[neighborNode.y, neighborNode.x]))
                    {
                        neighborNode.value = currentNode.value + walkCostGrid[neighborNode.y, neighborNode.x];
                        grid.SetGridObject(neighborNode.x, neighborNode.y, neighborNode);
                        openList.Add(neighborNode);
                    }
                }
                closedList.Add(currentNode);
            }
        }
        else
        {
            while (openList.Count > 0)
            {
                DijkstraMapNode currentNode = GetLowestValue(openList);
                openList.Remove(currentNode);
                foreach (DijkstraMapNode neighborNode in GetNeighborList(currentNode))
                {
                    if (moveModifier.ValidMovePosition(gameManager, currentNode, neighborNode, walkCostOveride))
                    {
                        neighborNode.value = currentNode.value + walkCostOveride;
                        grid.SetGridObject(neighborNode.x, neighborNode.y, neighborNode);
                        openList.Add(neighborNode);
                    }
                }
                closedList.Add(currentNode);
            }
        }


        for (int i = 0; i < grid.GetHeight(); i++)
        {
            for (int j = 0; j < grid.GetWidth(); j++)
            {
                DijkstraMapNode tempNode = grid.GetGridObject(j, i);
                if (tempNode.walkable == false)
                {
                    tempNode.value = int.MaxValue;
                    grid.SetGridObject(j, i, tempNode);
                }
            }
        }
    }

    // normal Movement grid whre goal is 0 and hexes increase in cost based on walking cost (Reset nodes to max value)
    // don't use this with no change grid if you start at enemy hexes
    public void SetGoalsNew(List<Vector2Int> goals, CombatGameManager gameManager, MoveModifier moveModifier,
        bool[,] badWalkinPassiveEffects, int walkCostOveride = -1, int[,] walkCostGridOveride = null)
    {
        if (goals.Count == 0)
        {
            Debug.LogWarning("Setting Goals without having any goals");
        }

        openList = new List<DijkstraMapNode>();
        closedList = new List<DijkstraMapNode>();

        for (int i = 0; i < goals.Count; i++)
        {
            DijkstraMapNode newNode = grid.GetGridObject(goals[i].x, goals[i].y);
            if (newNode != null)
            {
                newNode.value = 0;
                //Debug.Log("Set Goal: " + newNode.x + ", " + newNode.y);
                grid.SetGridObject(newNode.x, newNode.y, newNode);
                openList.Add(newNode);
            }
        }

        int[,] walkCostGrid = walkCostGridOveride;
        if (walkCostGrid == null)
        {
            walkCostGrid = gameManager.moveCostMap;
        }

        if (walkCostOveride == -1)
        {
            while (openList.Count > 0)
            {
                DijkstraMapNode currentNode = GetLowestValue(openList);
                openList.Remove(currentNode);
                foreach (DijkstraMapNode neighborNode in GetNeighborList(currentNode))
                {
                    if (moveModifier.ValidMovePosition(gameManager, currentNode, neighborNode,
                        walkCostGrid[neighborNode.y, neighborNode.x]))
                    {
                        //Debug.Log("Changed VAllue of Node: " +  neighborNode.x + ", " + neighborNode.y);
                        neighborNode.value = currentNode.value + walkCostGrid[neighborNode.y, neighborNode.x];

                        if (!badWalkinPassiveEffects[neighborNode.x, neighborNode.y])
                        {
                            openList.Add(neighborNode);
                        }
                        else
                        {
                            neighborNode.endPositionOnly = true;
                            //neighborNode.value = int.MaxValue;
                        }
                        grid.SetGridObject(neighborNode.x, neighborNode.y, neighborNode);
                    }
                }
                closedList.Add(currentNode);
            }
        }
        else
        {
            while (openList.Count > 0)
            {
                DijkstraMapNode currentNode = GetLowestValue(openList);
                openList.Remove(currentNode);
                foreach (DijkstraMapNode neighborNode in GetNeighborList(currentNode))
                {
                    if (!badWalkinPassiveEffects[neighborNode.x, neighborNode.y])
                    {
                        openList.Add(neighborNode);
                    }
                    else
                    {
                        neighborNode.endPositionOnly = true;
                    }
                }
                closedList.Add(currentNode);
            }
        }

        for (int i = 0; i < grid.GetHeight(); i++)
        {
            for (int j = 0; j < grid.GetWidth(); j++)
            {
                DijkstraMapNode tempNode = grid.GetGridObject(j, i);
                if (tempNode.walkable == false)
                {
                    tempNode.value = int.MaxValue;
                    grid.SetGridObject(j, i, tempNode);
                }
            }
        }
    }

    //Don't set unwalkable before Calling this, permissable units will cover that by accounting for extra rage
    //Standard Get Nodes in Range but expand extra hexes regardless of elevation equal to range 
    public List<DijkstraMapNode> GetNodesInMeleeRange(List<Vector2Int> targetLocations, int initialMoveValue,
        List<Vector2Int> permissableUnits,CombatGameManager gameManager, MoveModifier moveModifier, int originalRange, int walkCostOveride = -1, 
        int[,] walkCostGridOveride = null)
    {
        int range = originalRange;
        if (range <= 0)
        {
            Debug.LogWarning("Melee Range is less than or equal to 0");
            range = 1;
        }

        List<DijkstraMapNode> nodesInMovementRange = new List<DijkstraMapNode>();
        openList = new List<DijkstraMapNode>();
        closedList = new List<DijkstraMapNode>();


        foreach(Vector2Int targetNode in targetLocations)
        {
            DijkstraMapNode newNode = grid.GetGridObject(targetNode.x, targetNode.y);
            if (newNode != null)
            {
                newNode.value = initialMoveValue;
                newNode.permissableMoves = range;
                newNode.amountOfFreeMoves = range;
                grid.SetGridObject(newNode.x, newNode.y, newNode);
                openList.Add(newNode);
            }
        }

        int[,] walkCostGrid = walkCostGridOveride;
        if (walkCostGrid == null)
        {
            walkCostGrid = gameManager.moveCostMap;
        }

        if (walkCostOveride == -1)
        {
            while (openList.Count > 0)
            {
                DijkstraMapNode currentNode = GetHighestValue(openList);
                openList.Remove(currentNode);
                nodesInMovementRange.Add(currentNode);
                closedList.Add(currentNode);
                foreach (DijkstraMapNode neighborNode in GetNeighborList(currentNode))
                {
                    if(currentNode.amountOfFreeMoves <= 0)
                    {
                        continue;
                    }

                    if (moveModifier.CheckIfHexIsInMovementRange(gameManager, currentNode, neighborNode,
                        walkCostGrid[neighborNode.y, neighborNode.x]) && !closedList.Contains(neighborNode))
                    {
                        if (permissableUnits.Contains(new Vector2Int(neighborNode.x, neighborNode.y)))
                        {
                            neighborNode.permissableMoves = currentNode.permissableMoves - 1;
                            if (neighborNode.permissableMoves <= 0)
                            {
                                closedList.Add(neighborNode);
                                nodesInMovementRange.Add(neighborNode);
                                grid.SetGridObject(neighborNode.x, neighborNode.y, neighborNode);
                                continue;
                            }
                        }
                        neighborNode.value = currentNode.value - walkCostGrid[neighborNode.y, neighborNode.x];
                        neighborNode.amountOfFreeMoves = currentNode.amountOfFreeMoves;
                        grid.SetGridObject(neighborNode.x, neighborNode.y, neighborNode);
                        openList.Add(neighborNode);
                    }
                    else if(currentNode.permissableMoves >= 1 && currentNode.amountOfFreeMoves > 0 && 
                        moveModifier.ValidMove(gameManager, currentNode, neighborNode) && !openList.Contains(neighborNode) &&
                        !closedList.Contains(neighborNode))
                    {
                        if (permissableUnits.Contains(new Vector2Int(neighborNode.x, neighborNode.y)))
                        {
                            neighborNode.permissableMoves = currentNode.permissableMoves - 1;
                            if (neighborNode.permissableMoves <= 0)
                            {
                                closedList.Add(neighborNode);
                                nodesInMovementRange.Add(neighborNode);
                                grid.SetGridObject(neighborNode.x, neighborNode.y, neighborNode);
                                continue;
                            }
                        }
                        neighborNode.value = currentNode.value;
                        neighborNode.amountOfFreeMoves = currentNode.amountOfFreeMoves - 1;
                        grid.SetGridObject(neighborNode.x, neighborNode.y, neighborNode);
                        openList.Add(neighborNode);
                    }
                }
            }
        }
        else
        {
            while (openList.Count > 0)
            {
                DijkstraMapNode currentNode = GetHighestValue(openList);
                openList.Remove(currentNode);
                nodesInMovementRange.Add(currentNode);
                closedList.Add(currentNode);
                foreach (DijkstraMapNode neighborNode in GetNeighborList(currentNode))
                {
                    if (moveModifier.CheckIfHexIsInMovementRange(gameManager, currentNode, neighborNode,
                        walkCostOveride) && !closedList.Contains(neighborNode))
                    {
                        if (permissableUnits.Contains(new Vector2Int(neighborNode.x, neighborNode.y)))
                        {
                            neighborNode.permissableMoves = currentNode.permissableMoves - 1;
                            if (neighborNode.permissableMoves <= 0)
                            {
                                closedList.Add(neighborNode);
                                nodesInMovementRange.Add(neighborNode);
                                grid.SetGridObject(neighborNode.x, neighborNode.y, neighborNode);
                                continue;
                            }
                        }
                        neighborNode.value = currentNode.value - walkCostOveride;
                        grid.SetGridObject(neighborNode.x, neighborNode.y, neighborNode);
                        openList.Add(neighborNode);
                    }
                    else if (currentNode.permissableMoves >= 1 && currentNode.amountOfFreeMoves > 0 &&
                        moveModifier.ValidMove(gameManager, currentNode, neighborNode) && !openList.Contains(neighborNode) &&
                        !closedList.Contains(neighborNode))
                    {
                        if (permissableUnits.Contains(new Vector2Int(neighborNode.x, neighborNode.y)))
                        {
                            neighborNode.permissableMoves = currentNode.permissableMoves - 1;
                            if (neighborNode.permissableMoves <= 0)
                            {
                                closedList.Add(neighborNode);
                                nodesInMovementRange.Add(neighborNode);
                                grid.SetGridObject(neighborNode.x, neighborNode.y, neighborNode);
                                continue;
                            }
                        }

                        neighborNode.value = currentNode.value;
                        neighborNode.amountOfFreeMoves = currentNode.amountOfFreeMoves - 1;
                        grid.SetGridObject(neighborNode.x, neighborNode.y, neighborNode);
                        if (neighborNode.amountOfFreeMoves > 0)
                        {
                            openList.Add(neighborNode);
                        }
                        else
                        {
                            closedList.Add(neighborNode);
                        }
                    }
                }
                nodesInMovementRange.Add(currentNode);
                closedList.Add(currentNode);
            }
        }
        return nodesInMovementRange;
    }

    //Don't set unwalkable before Calling this, permissable units will cover that by accounting for extra rage
    //Standard Get Nodes in Range but expand extra hexes regardless of elevation equal to range 
    // should be only used to get nodes in Ranged range
    public List<DijkstraMapNode> GetNodesInRangedRange(List<Vector2Int> targetLocations, int initialMoveValue,
        List<Vector2Int> permissableUnits, CombatGameManager gameManager, MoveModifier moveModifier, int originalRange, int walkCostOveride = -1,
        int[,] walkCostGridOveride = null)
    {
        int range = originalRange;
        if (range <= 0)
        {
            Debug.LogWarning("Melee Range is less than or equal to 0");
            range = 1;
        }

        List<DijkstraMapNode> nodesInMovementRange = new List<DijkstraMapNode>();
        openList = new List<DijkstraMapNode>();
        closedList = new List<DijkstraMapNode>();


        foreach (Vector2Int targetNode in targetLocations)
        {
            DijkstraMapNode newNode = grid.GetGridObject(targetNode.x, targetNode.y);
            if (newNode != null)
            {
                newNode.value = initialMoveValue;
                newNode.amountOfFreeMoves = range;
                grid.SetGridObject(newNode.x, newNode.y, newNode);
                openList.Add(newNode);
            }
        }

        int[,] walkCostGrid = walkCostGridOveride;
        if (walkCostGrid == null)
        {
            walkCostGrid = gameManager.moveCostMap;
        }

        if (walkCostOveride == -1)
        {
            while (openList.Count > 0)
            {
                DijkstraMapNode currentNode = GetHighestValue(openList);
                openList.Remove(currentNode);
                nodesInMovementRange.Add(currentNode);
                closedList.Add(currentNode);
                foreach (DijkstraMapNode neighborNode in GetNeighborList(currentNode))
                {   
                    if (currentNode.amountOfFreeMoves <= 0 )
                    {
                        continue;
                    }

                    if (moveModifier.CheckIfHexIsInMovementRange(gameManager, currentNode, neighborNode,
                        walkCostGrid[neighborNode.y, neighborNode.x]) && !closedList.Contains(neighborNode))
                    {
                        if (permissableUnits.Contains(new Vector2Int(neighborNode.x, neighborNode.y)))
                        {
                            neighborNode.value = -1;
                            neighborNode.amountOfFreeMoves = currentNode.amountOfFreeMoves - 1;
                            if (neighborNode.amountOfFreeMoves <= 0)
                            {
                                closedList.Add(neighborNode);
                                nodesInMovementRange.Add(neighborNode);
                                grid.SetGridObject(neighborNode.x, neighborNode.y, neighborNode);
                            }
                            else
                            {
                                grid.SetGridObject(neighborNode.x, neighborNode.y, neighborNode);
                                openList.Add(neighborNode);
                            }
                        }
                        else
                        {
                            neighborNode.value = currentNode.value - walkCostGrid[neighborNode.y, neighborNode.x];
                            neighborNode.amountOfFreeMoves = currentNode.amountOfFreeMoves;
                            grid.SetGridObject(neighborNode.x, neighborNode.y, neighborNode);
                            openList.Add(neighborNode);
                        }
                    }
                    else if (!openList.Contains(neighborNode) && !closedList.Contains(neighborNode))
                    {
                        neighborNode.value = -1;
                        neighborNode.amountOfFreeMoves = currentNode.amountOfFreeMoves - 1;
                        if (permissableUnits.Contains(new Vector2Int(neighborNode.x, neighborNode.y)) && neighborNode.amountOfFreeMoves <= 0)
                        {
                                closedList.Add(neighborNode);
                                nodesInMovementRange.Add(neighborNode);
                                grid.SetGridObject(neighborNode.x, neighborNode.y, neighborNode);
                        }
                        else
                        {
                            grid.SetGridObject(neighborNode.x, neighborNode.y, neighborNode);
                            openList.Add(neighborNode);
                        }                        
                    }
                }
            }
        }
        else
        {
            while (openList.Count > 0)
            {
                DijkstraMapNode currentNode = GetHighestValue(openList);
                openList.Remove(currentNode);
                nodesInMovementRange.Add(currentNode);
                closedList.Add(currentNode);
                foreach (DijkstraMapNode neighborNode in GetNeighborList(currentNode))
                {
                    if (currentNode.amountOfFreeMoves <= 0)
                    {
                        continue;
                    }

                    if (moveModifier.CheckIfHexIsInMovementRange(gameManager, currentNode, neighborNode,
                        walkCostOveride) && !closedList.Contains(neighborNode))
                    {
                        if (permissableUnits.Contains(new Vector2Int(neighborNode.x, neighborNode.y)))
                        {
                            neighborNode.value = -1;
                            neighborNode.amountOfFreeMoves = currentNode.amountOfFreeMoves - 1;
                            if (neighborNode.amountOfFreeMoves <= 0)
                            {
                                closedList.Add(neighborNode);
                                nodesInMovementRange.Add(neighborNode);
                                grid.SetGridObject(neighborNode.x, neighborNode.y, neighborNode);
                            }
                            else
                            {
                                grid.SetGridObject(neighborNode.x, neighborNode.y, neighborNode);
                                openList.Add(neighborNode);
                            }
                        }
                        else
                        {
                            neighborNode.value = currentNode.value - walkCostOveride;
                            neighborNode.amountOfFreeMoves = currentNode.amountOfFreeMoves;
                            grid.SetGridObject(neighborNode.x, neighborNode.y, neighborNode);
                            openList.Add(neighborNode);
                        }
                    }
                    else if (!openList.Contains(neighborNode) && !closedList.Contains(neighborNode))
                    {
                        neighborNode.value = -1;
                        neighborNode.amountOfFreeMoves = currentNode.amountOfFreeMoves - 1;
                        if (permissableUnits.Contains(new Vector2Int(neighborNode.x, neighborNode.y)) && neighborNode.amountOfFreeMoves <= 0)
                        {
                            closedList.Add(neighborNode);
                            nodesInMovementRange.Add(neighborNode);
                            grid.SetGridObject(neighborNode.x, neighborNode.y, neighborNode);
                        }
                        else
                        {
                            grid.SetGridObject(neighborNode.x, neighborNode.y, neighborNode);
                            openList.Add(neighborNode);
                        }
                    }
                }
            }
        }
        return nodesInMovementRange;
    }


    //Don't set unwalkable before Calling this, permissable units will cover that by accounting for extra rage
    //Set Values to -1 before, makes targetlocations the goal with values equal to original range than expand from goals by subtracting 1 until we reach 0
    //
    public void SetGoalForNodesInTargetRange(List<Vector2Int> targetLocations, int originalRange)
    {
        int range = originalRange;
        if (range <= 0)
        {
            Debug.LogWarning("Melee Range is less than or equal to 0");
            range = 1;
        }

        openList = new List<DijkstraMapNode>();
        closedList = new List<DijkstraMapNode>();

        foreach (Vector2Int targetNode in targetLocations)
        {
            DijkstraMapNode newNode = grid.GetGridObject(targetNode.x, targetNode.y);
            if (newNode != null)
            {
                newNode.value = range;
                grid.SetGridObject(newNode.x, newNode.y, newNode);
                openList.Add(newNode);
            }
        }

        while (openList.Count > 0)
        {
            DijkstraMapNode currentNode = GetHighestValue(openList);
            openList.Remove(currentNode);
            closedList.Add(currentNode);
            foreach (DijkstraMapNode neighborNode in GetNeighborList(currentNode))
            {
                if (currentNode.value <= 0 ||  neighborNode.value >= currentNode.value)
                {
                    continue;
                }

                neighborNode.value = currentNode.value - 1;
                grid.SetGridObject(neighborNode.x, neighborNode.y, neighborNode);
                openList.Add(neighborNode);
            }
        }
    }


    // make permissable units frinely units because goals start at all enemy units at 0 and trying to find distance to fiendlyies
    public void SetGoalsForThreatGrid(List<Vector2Int> goals, List<Vector2Int> permissableUnits, CombatGameManager gameManager, MoveModifier moveModifier,
       int walkCostOveride = -1, int[,] walkCostGridOveride = null, bool debug = false)
    {
        if (goals.Count == 0)
        {
            Debug.LogWarning("Setting Goals without having any goals");
        }

        openList = new List<DijkstraMapNode>();
        closedList = new List<DijkstraMapNode>();

        for (int i = 0; i < goals.Count; i++)
        {
            DijkstraMapNode newNode = grid.GetGridObject(goals[i].x, goals[i].y);
            if (newNode != null)
            {
                newNode.value = 0;
                newNode.permissableMoves = 1;
                grid.SetGridObject(newNode.x, newNode.y, newNode);
                openList.Add(newNode);
            }

        }

        int[,] walkCostGrid = walkCostGridOveride;
        if (walkCostGrid == null)
        {
            walkCostGrid = gameManager.moveCostMap;
        }
        if (walkCostOveride == -1)
        {
            while (openList.Count > 0)
            {
                DijkstraMapNode currentNode = GetLowestValue(openList);
                openList.Remove(currentNode);
                closedList.Add(currentNode);
                foreach (DijkstraMapNode neighborNode in GetNeighborList(currentNode))
                {
                    if (moveModifier.ValidMovePositionNoWalkable(gameManager, currentNode, neighborNode,
                        walkCostGrid[neighborNode.y, neighborNode.x]) && !closedList.Contains(neighborNode))
                    {
                        if (permissableUnits.Contains(new Vector2Int(neighborNode.x, neighborNode.y)))
                        {
                            neighborNode.permissableMoves = currentNode.permissableMoves - 1;
                            if (neighborNode.permissableMoves <= 0)
                            {
                                neighborNode.value = currentNode.value + walkCostGrid[neighborNode.y, neighborNode.x];
                                closedList.Add(neighborNode);
                                grid.SetGridObject(neighborNode.x, neighborNode.y, neighborNode);
                                continue;
                            }
                        }
                        neighborNode.value = currentNode.value + walkCostGrid[neighborNode.y, neighborNode.x];
                        grid.SetGridObject(neighborNode.x, neighborNode.y, neighborNode);
                        openList.Add(neighborNode);
                    }
                }
            }
        }
        else
        {
            while (openList.Count > 0)
            {
                DijkstraMapNode currentNode = GetLowestValue(openList);
                openList.Remove(currentNode);
                closedList.Add(currentNode);
                foreach (DijkstraMapNode neighborNode in GetNeighborList(currentNode))
                {
                    if (moveModifier.ValidMovePositionNoWalkable(gameManager, currentNode, neighborNode,
                        walkCostOveride) && !closedList.Contains(neighborNode))
                    {
                        if (permissableUnits.Contains(new Vector2Int(neighborNode.x, neighborNode.y)))
                        {
                            neighborNode.permissableMoves = currentNode.permissableMoves - 1;
                            if (neighborNode.permissableMoves <= 0)
                            {
                                neighborNode.value = currentNode.value + walkCostOveride;
                                closedList.Add(neighborNode);
                                grid.SetGridObject(neighborNode.x, neighborNode.y, neighborNode);
                                continue;
                            }
                        }
                        neighborNode.value = currentNode.value + walkCostOveride;
                        grid.SetGridObject(neighborNode.x, neighborNode.y, neighborNode);
                        openList.Add(neighborNode);
                    }
                }
                closedList.Add(currentNode);
            }
        }
    }

    public void SetUnwalkable(Vector2Int unwalkableHex)
    {
        SetUnwalkable(new List<Vector2Int>() {  unwalkableHex });
    }

    public void SetUnwalkable(List<Vector2Int> unwalkableHexes)
    {
        for(int i = 0; i < unwalkableHexes.Count; i++)
        {
            DijkstraMapNode tempNode = grid.GetGridObject(unwalkableHexes[i]);
            tempNode.walkable = false;
            grid.SetGridObject(unwalkableHexes[i], tempNode);
        }
    }

    public void SetWalkable(Vector2Int unwalkableHex)
    {
        SetWalkable(new List<Vector2Int>() { unwalkableHex });
    }

    public void SetWalkable(List<Vector2Int> unwalkableHexes)
    {
        for (int i = 0; i < unwalkableHexes.Count; i++)
        {
            DijkstraMapNode tempNode = grid.GetGridObject(unwalkableHexes[i]);
            tempNode.walkable = true;
            grid.SetGridObject(unwalkableHexes[i], tempNode);
        }
    }

    private List<DijkstraMapNode> GetNeighborList(DijkstraMapNode currentNode)
    {
        List<DijkstraMapNode> neighborList = new List<DijkstraMapNode>();


        List<Vector2Int> neighborNodes = new List<Vector2Int>();
        bool oddRow = currentNode.x % 2 == 1;
        Vector2Int currentPosition = new Vector2Int(currentNode.x, currentNode.y);
        neighborNodes.Add(currentPosition + new Vector2Int(0, 1));
        neighborNodes.Add(currentPosition + new Vector2Int(0, -1));

        neighborNodes.Add(currentPosition + new Vector2Int(-1, oddRow ? 1 : -1));
        neighborNodes.Add(currentPosition + new Vector2Int(-1, 0));
        neighborNodes.Add(currentPosition + new Vector2Int(1, oddRow ? 1 : -1));
        neighborNodes.Add(currentPosition + new Vector2Int(1, 0));

        for (int i = 0; i < neighborNodes.Count; i++)
        {
            DijkstraMapNode neighborNode = grid.GetGridObject(neighborNodes[i].x, neighborNodes[i].y);
            if (neighborNode != null)
            {
                neighborList.Add(neighborNode);
            }
        }
        return neighborList;
    }

    // Full Reset Resets Walkable Status on All Hexes 
    public void ResetMap(bool fullReset = false, bool resetToMaxValue = true)
    {
        if (resetToMaxValue)
        {
            if (fullReset)
            {
                for (int i = 0; i < grid.GetHeight(); i++)
                {
                    for (int j = 0; j < grid.GetWidth(); j++)
                    {
                        DijkstraMapNode tempNode = grid.GetGridObject(j, i);
                        tempNode.value = int.MaxValue;
                        tempNode.walkable = true;
                        tempNode.endPositionOnly = false;
                        tempNode.permissableMoves = 1;
                        grid.SetGridObject(j, i, tempNode);
                    }
                }
            }
            else
            {
                for (int i = 0; i < grid.GetHeight(); i++)
                {
                    for (int j = 0; j < grid.GetWidth(); j++)
                    {
                        DijkstraMapNode tempNode = grid.GetGridObject(j, i);
                        tempNode.value = int.MaxValue;
                        tempNode.permissableMoves = 1;
                        tempNode.endPositionOnly = false;
                        grid.SetGridObject(j, i, tempNode);
                    }
                }
            }
        }
        else
        {
            if (fullReset)
            {
                for (int i = 0; i < grid.GetHeight(); i++)
                {
                    for (int j = 0; j < grid.GetWidth(); j++)
                    {
                        DijkstraMapNode tempNode = grid.GetGridObject(j, i);
                        // make nodes negative one so movementValue can go to 0 and be added to nodes in range
                        tempNode.value = -1;
                        tempNode.walkable = true;
                        tempNode.endPositionOnly = false;
                        tempNode.permissableMoves = 1;
                        grid.SetGridObject(j, i, tempNode);
                    }
                }
            }
            else
            {
                for (int i = 0; i < grid.GetHeight(); i++)
                {
                    for (int j = 0; j < grid.GetWidth(); j++)
                    {
                        DijkstraMapNode tempNode = grid.GetGridObject(j, i);
                        // make nodes negative one so movementValue can go to 0 and be added to nodes in range
                        tempNode.value = -1;
                        tempNode.permissableMoves = 1;
                        tempNode.endPositionOnly = false;
                        grid.SetGridObject(j, i, tempNode);
                    }
                }
            }
        }
    }

    public DijkstraMapNode GetLowestValue(List<DijkstraMapNode> nodeList)
    {
        int lowestValue = nodeList[0].value;
        int index = 0;
        for(int i = 1; i < nodeList.Count; i++)
        {
            if (nodeList[i].value < lowestValue)
            {
                lowestValue = nodeList[i].value;
                index = i;
            }
        }

        return nodeList[index];
    }

    public DijkstraMapNode GetHighestValue(List<DijkstraMapNode> nodeList)
    {
        int highestValue = nodeList[0].value;
        int index = 0;
        for (int i = 1; i < nodeList.Count; i++)
        {
            if (nodeList[i].value > highestValue)
            {
                highestValue = nodeList[i].value;
                index = i;
            }
        }

        return nodeList[index];
    }

    public int[,] GetGridValues()
    {
        int[,] gridValues =  new int[grid.GetWidth(), grid.GetHeight()];
        for(int i = 0; i < grid.GetHeight(); i++)
        {
            for(int j = 0; j < grid.GetWidth(); j++)
            {
                gridValues[j, i] =  grid.GetGridObject(j, i).value;
            }
        }
        return gridValues;
    }

    public GridHex<DijkstraMapNode> getGrid()
    {
        return grid;
    }
}
