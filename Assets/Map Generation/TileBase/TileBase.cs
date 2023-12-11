using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TileBase/TileBase")]
public class TileBase : ScriptableObject
{
    public int tileIndex;
    public TileType tileType;
    public bool isUnderground;
    public string tileNameAbbreviation;
    public List<Structure> structures;
    public List<GameObject> setPieces;
    public Encounter encounter;
    [SerializeField]
    private float setPieceFactorMin;
    [SerializeField]
    private float setPieceFactorMax;
    [SerializeField]
    private float oneStructureChance;
    [SerializeField]
    private float twoStructureChance;
    [SerializeField]
    private float threeStructureChance;
    [SerializeField]
    private float fourStructureChance;

    public int FindNumberOfStructures()
    {
        if(oneStructureChance + twoStructureChance + threeStructureChance + fourStructureChance > 1)
        {
            Debug.LogError("Redo Values");
            return 0;
        }
        Random.InitState(System.DateTime.Now.Millisecond);
        float probability = Random.Range(0f, 1f);
        if(probability <= oneStructureChance)
        {
            return 1;
        }
        else if(probability <= oneStructureChance + twoStructureChance)
        {
            return 2;
        }
        else if(probability <= oneStructureChance + twoStructureChance + threeStructureChance)
        {
            return 3;
        }
        else if (probability <= oneStructureChance + twoStructureChance + threeStructureChance + fourStructureChance)
        {
            return 4;
        }
        else
        {
            return 0;
        }
    }

    //For percentage of map taken up by setpieces like trees and cacti
    public float FindSetPieceFactor()
    {
        if(setPieceFactorMax < setPieceFactorMin)
        {
            Debug.LogError("Redo Setpiece Max and Min numbers for: " + this.name);
        }
        Random.InitState(System.DateTime.Now.Millisecond);
        float probability = Random.Range(setPieceFactorMin, setPieceFactorMax);
        return probability;
    }
}
    