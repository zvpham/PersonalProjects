using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Marker/BlastRadius")]
public class BlastRadiusMarker : MonoBehaviour
{
    public Grid<BlastRadiusMarker> grid;
    public int x;
    public int y;
    public GameObject blastMarker;
    private float dist;
    public BlastRadiusMarker(Grid<BlastRadiusMarker> grid, int x, int y, GameObject blastMarkerPrefab, Vector3 originPosition, float radius)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        this.dist = radius;
        Debug.Log("Start Location" + (originPosition + new Vector3(grid.GetWidth() / 2, grid.GetHeight() / 2, 0)));
        if (Vector3.Distance(originPosition + new Vector3(grid.GetWidth() / 2, grid.GetHeight() / 2, 0), originPosition + new Vector3(x, y, 0)) <= dist)
            this.blastMarker = Instantiate(blastMarkerPrefab, originPosition + new Vector3(x, y, 0), new Quaternion(0, 0, 0, 1f));
    }   

    public void DestroySelf()
    {
        Destroy(blastMarker);
        Destroy(this);
    }
    public override string ToString()   
    {
        return x + "," + y;
    }
}
