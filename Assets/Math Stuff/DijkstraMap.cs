using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    public void SetGoals(List<Vector2Int> goals, CombatGameManager gameManager, MoveModifier moveModifier)
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


        while (openList.Count > 0)
        {
            DijkstraMapNode currentNode = GetLowestValue(openList);
            openList.Remove(currentNode);
            foreach (DijkstraMapNode neighborNode in GetNeighborList(currentNode))
            {
                if (moveModifier.ValidMovePosition(gameManager, currentNode, neighborNode))
                {
                    neighborNode.value = currentNode.value + 1;
                    grid.SetGridObject(neighborNode.x, neighborNode.y, neighborNode);
                    openList.Add(neighborNode);
                }
            }
            closedList.Add(currentNode);
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
    public void ResetMap(bool fullReset = false)
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
                    grid.SetGridObject(j, i, tempNode);
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

    public GridHex<DijkstraMapNode> getGrid()
    {
        return grid;
    }
}
