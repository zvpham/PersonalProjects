using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Encounter")]
public class Encounter : MonoBehaviour
{
    public int dangerRating;
    public List<Unit> units;
    public List<int> numberOfUnits;
}
