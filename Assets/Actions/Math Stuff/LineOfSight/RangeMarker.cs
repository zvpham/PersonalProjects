using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Marker/Range")]
public class RangeMarker : MonoBehaviour    
{
    public Grid<RangeMarker> grid;
    public int x;
    public int y;
    public GameObject rangeMarker;          

    public RangeMarker(Grid<RangeMarker> grid, int x, int y, GameObject rangeMarkerPrefab, Vector3 originPosition)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        rangeMarker = Instantiate(rangeMarkerPrefab, originPosition + new Vector3(x, y, 0), new Quaternion(0, 0, 0, 1f));
    }

    public void DestroySelf()
    {
        Destroy(rangeMarker);
        Destroy(this);
    }

    public override string ToString()
    {
        return x + "," + y;
    }
}
