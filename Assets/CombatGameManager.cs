using CodeMonkey.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Tilemaps;

public class CombatGameManager : MonoBehaviour
{
    public ResourceManager resourceManager;
    public SpriteManager spriteManager;
    public DijkstraMap map;
    public GridHex<GridPosition> grid;
    public GameObject player;
    public List<AStarPathNode> movement;

    public List<Unit> units = new List<Unit>();
    public List<IInititiave> allinitiativeGroups = new List<IInititiave>();
    public List<IInititiave> initiativeOrder = new List<IInititiave>();

    public PlayerTurn playerTurn;
    public bool playerTurnActive = false;

    public Vector3 defaultGridAdjustment = Vector3.zero;
    public float cellSize;
    // Start is called before the first frame update
    void Awake()
    {
        OnStartGame(10, 10);
    }

    public void Update()
    {
        NextTurn();
        enabled = false;
    }

    public void NextTurn()
    {
        if(initiativeOrder.Count == 0)
        {
            setInitiativeOrder();
        }
        initiativeOrder[initiativeOrder.Count - 1].StartTurn();
    }

    public void OnStartGame(int mapWidth, int mapHeight)
    {
        grid = new GridHex<GridPosition>(mapWidth, mapHeight, cellSize, defaultGridAdjustment, (GridHex<GridPosition> g, int x, int y) => new GridPosition(g, x, y), true);
        map = new DijkstraMap(10, 10, cellSize,  defaultGridAdjustment, false);
    }

    public void StartPlayerTurn(Unit unit)
    {
        StartPlayerTurn(new List<Unit>() { unit });
    }

    public void StartPlayerTurn(List<Unit> unitGroup)
    {
        playerTurn.OnTurnStart(unitGroup);
        playerTurnActive = true;
    }

    public void TurnEnd(IInititiave initiativeGroup)
    {
        Debug.Log("End Turn");
        initiativeOrder.Remove(initiativeGroup);
        NextTurn();
    }


    public void setInitiativeOrder()
    {
        List<int> initiatives = new List<int>();
        initiativeOrder.Clear();
        for (int i = 0; i < allinitiativeGroups.Count; i++)
        {
            initiatives.Add(allinitiativeGroups[i].CalculateInititive());
            initiativeOrder.Add(allinitiativeGroups[i]);
        }

        Quicksort(initiatives, 0, initiatives.Count - 1);
    }

    public void SetGridObject(Unit unit, Vector3 unitPosition)
    {
        GridPosition gridPosition = grid.GetGridObject(unitPosition);
        gridPosition.unit = unit;
        grid.SetGridObject(unitPosition, gridPosition);
    }

    private void Quicksort(List<int> array, int left, int right)
    {
        if (left < right)
        {
            int pivotIndex = Partition(array, left, right);
            Quicksort(array, left, pivotIndex - 1);
            Quicksort(array, pivotIndex + 1, right);
        }
    }

    private int Partition(List<int> array, int left, int right)
    {
        int pivot = array[right];
        int i = left - 1;

        for (int j = left; j < right; j++)
        {
            if (array[j] < pivot)
            {
                i++;
                Swap(array, i, j);
            }
        }

        Swap(array, i + 1, right);
        return i + 1;
    }
    private void Swap(List<int> array, int i, int j)
    {
        int temp = array[i];
        array[i] = array[j];
        array[j] = temp;

        IInititiave tempi = initiativeOrder[i];
        initiativeOrder[i] = initiativeOrder[j];
        initiativeOrder[j] = tempi;
    }
}
