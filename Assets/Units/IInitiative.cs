using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInititiave
{
    public int CalculateInititive();

    public void StartTurn();

    public void Quicksort(List<int> array, List<IInititiave> initiativeOrder, int left, int right)
    {
        if (left < right)
        {
            int pivotIndex = Partition(array, initiativeOrder, left, right);
            Quicksort(array, initiativeOrder, left, pivotIndex - 1);
            Quicksort(array, initiativeOrder,  pivotIndex + 1, right);
        }
    }

    private int Partition(List<int> array, List<IInititiave> initiativeOrder, int left, int right)
    {
        int pivot = array[right];
        int i = left - 1;

        for (int j = left; j < right; j++)
        {
            if (array[j] < pivot)
            {
                i++;
                Swap(array, initiativeOrder, i, j);
            }
        }

        Swap(array, initiativeOrder, i + 1, right);
        return i + 1;
    }
    private void Swap(List<int> array, List<IInititiave> initiativeOrder, int i, int j)
    {
        int temp = array[i];
        array[i] = array[j];
        array[j] = temp;

        IInititiave tempi = initiativeOrder[i];
        initiativeOrder[i] = initiativeOrder[j];
        initiativeOrder[j] = tempi;
    }
}
