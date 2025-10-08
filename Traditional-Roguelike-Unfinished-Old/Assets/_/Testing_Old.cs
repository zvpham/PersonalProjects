/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public class Testing_Old : MonoBehaviour {

    [SerializeField] private HeatMapVisual heatMapGenericVisual;
    private Grid<HeatMapGridObject> grid;

    private void Start() {
        grid = new Grid<HeatMapGridObject>(20, 10, 8f, Vector3.zero, (Grid<HeatMapGridObject> g, int x, int y) => new HeatMapGridObject(g, x, y));

        heatMapGenericVisual.SetGrid(grid);

        /*
        SaveObject saveObject = new SaveObject {
            //goldAmount = 15,
            //playerPosition = new Vector3(100, 0),
            gridObjectArray = new SaveObject.GridObject[] {
                new SaveObject.GridObject { _i = 5 },
                new SaveObject.GridObject { _i = 15 },
                new SaveObject.GridObject { _i = 56 },
            },
        };
        Debug.Log(saveObject);
        SaveSystem.SaveObject(saveObject);
        //*/

        SaveObject saveObject = SaveSystem.LoadMostRecentObject<SaveObject>();
        Debug.Log(saveObject);
    }

    private void Update() {
        Vector3 position = UtilsClass.GetMouseWorldPosition();

        if (Input.GetMouseButtonDown(0)) {
            HeatMapGridObject heatMapGridObject = grid.GetGridObject(position);
            if (heatMapGridObject != null) {
                heatMapGridObject.AddValue(5);
            }
        }
    }

    private class SaveObject {

        public GridObject[] gridObjectArray;

        [System.Serializable]
        public class GridObject {
            public int _i;

            public override string ToString() {
                return _i.ToString();
            }
        }

        public override string ToString() {
            string str = "";
            foreach (GridObject gridObject in gridObjectArray) {
                str += gridObject.ToString() + ", ";
            }
            return str;
        }
    }
}