using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
using System.Xml;
using UnityEditor;

public class MapGenerator : MonoBehaviour
{
    public ResourceManager resourceManager;
    public GameManager gameManager;
    public Grid<NoiseMapObject> noiseMap;
    public OverlapWFC WFCGenerator;

    public List<GameObject> unitHolderForTesting = new List<GameObject>(); 
    public List<GameObject> structureWallHolderFortesting =  new List<GameObject>();
    public List<GameObject> setPieceHolderForTesting = new List<GameObject>();

    public int mapWidth;
    public int mapHeight;
    public int gridSize = 1;

    public TileBase TileBaseTest;
    public GameManager testingGameManager;

    public static MapGenerator Instance;

    public void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Found more than MapGenerator in the Scence");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    public void Start()
    {
        gameManager = GameManager.instance;
    }

    public void WaveFunctionCollapse(WFCTemplate structureTemplate, Structure structure, Vector3 structureStartLocation)
    {
        int WFCTemplateTrainerIndex = Random.Range(0, structure.validWFCTrainerTemplates.Count);
        WFCGenerator.training = resourceManager.WFCTrainerTemplates[
            structure.validWFCTrainerTemplates[WFCTemplateTrainerIndex]].GetComponent<Training>();
        WFCGenerator.width = structureTemplate.width + 1;
        WFCGenerator.depth = structureTemplate.height + 1;
        WFCGenerator.seed = System.DateTime.Now.Millisecond;
        WFCGenerator.N = 2;
        WFCGenerator.symmetry = 8;
        WFCGenerator.foundation = 0;
        WFCGenerator.iterations = 0;
        WFCGenerator.Generate();
    }

    public void CreateNoiseMap()
    {
        noiseMap = new Grid<NoiseMapObject>(mapWidth, mapHeight,
            1f, new Vector3(-0.5f, -0.5f, 0f), (Grid<NoiseMapObject> g, int x, int y) =>
            new NoiseMapObject(g, x, y, Mathf.PerlinNoise(x * 1f, y * 1f)));
    }

    public void RandomizeNoseMap()
    {
        if(noiseMap == null)
        {
            Debug.LogError("Yo Something is Wack YO");
        }
        for (int i = 0; i < noiseMap.GetHeight(); i++)
        {
            for (int j = 0; j < noiseMap.GetWidth(); j++)
            {
                noiseMap.GetGridObject(j, i).value = Mathf.PerlinNoise(j * 1f, i * 1f);
            }
        }
    }

    public void ResetTesting()
    {
        for(int i = 0; i < unitHolderForTesting.Count; i++)
        {
            DestroyImmediate(unitHolderForTesting[i]);
        }
        unitHolderForTesting = new List<GameObject> { };

        for (int i = 0; i < structureWallHolderFortesting.Count; i++)
        {
            DestroyImmediate(structureWallHolderFortesting[i]);
        }
        structureWallHolderFortesting = new List<GameObject> { };

        for (int i = 0; i < setPieceHolderForTesting.Count; i++)
        {
            DestroyImmediate(setPieceHolderForTesting[i]);
        }
        setPieceHolderForTesting = new List<GameObject> { };
    }

    public void GeneratePrefabTile(TilePrefabBase prefabTileBase, GameManager gameManager)
    {
        GameObject o;
        // TIleINfO - <x, y, WallIndex>
        Vector3Int tileInfo;
        for (int i = 0; i < prefabTileBase.mapTiles.Count; i++)
        {
            tileInfo = prefabTileBase.mapTiles[i];
            if (tileInfo.z == 0 || tileInfo.z == 1)
            {
                continue;
            }
            o = Instantiate(resourceManager.walls[tileInfo.z], new Vector3(), new Quaternion(0, 0, 0, 1f));
            o.GetComponent<Wall>().gameManager = gameManager;
            o.transform.position = gameManager.defaultGridPosition;
            o.transform.position += (new Vector3(tileInfo.x, tileInfo.y, 0) * gridSize);
        }

        // TIleINfO - <x, y, unitIndex>
        for (int i = 0; i < prefabTileBase.units.Count; i++)
        {
            tileInfo = prefabTileBase.units[i];
            if (tileInfo.z == 3)
            {
                continue;
            }
            o = Instantiate(resourceManager.unitPrefabs[tileInfo.z], new Vector3(), new Quaternion(0, 0, 0, 1f));
            o.GetComponent<Unit>().gameManager = gameManager;
            o.transform.position = gameManager.defaultGridPosition;
            o.transform.position += (new Vector3(tileInfo.x, tileInfo.y, 0) * gridSize);
        }

        if (prefabTileBase.haveMapGenerateEncounters)
        {

        }

        if (prefabTileBase.haveMapGenerateWalls)
        {

        }
    }

    public void GenerateTile(TileBase initialTileBase, GameManager initialGameManager, float extraDangerModifier,
        bool isTesting = false)
    {
        TileBase tileBase;
        GameManager gameManager;
        if (isTesting)
        {
            tileBase = TileBaseTest;
            gameManager = testingGameManager;
        }
        else
        {
            tileBase = initialTileBase;
            gameManager = initialGameManager;
        }
        unitHolderForTesting = new List<GameObject>();
        structureWallHolderFortesting = new List<GameObject>();
        setPieceHolderForTesting = new List<GameObject>();
        // Vector[bottomLeftCorner, topLeftCorner, bottomRightCorner, topRightCorner]
        List<Vector3[]> structureCorners = new List<Vector3[]>();
        List<Vector3> availableLocations = new List<Vector3>();
        List<Vector3> tempLocations =  new List<Vector3>();
        int[,] availableTiles;
        for (int i = 0; i < mapHeight; i++)
        {
            for(int j = 0; j < mapWidth; j++)
            {
                availableLocations.Add(new Vector3(j, i, 0));
            }
        }
        int numStructures =  tileBase.FindNumberOfStructures();
        for (int i = 0; i < numStructures; i++)
        {
            Random.InitState(System.DateTime.Now.Millisecond);
            int structureIndex =  Random.Range(0, tileBase.structures.Count);
            int WFCTemplateIndex = Random.Range(0, tileBase.structures[structureIndex].WFCTemplates.Count);
            Structure structure = tileBase.structures[structureIndex];
            WFCTemplate structureTemplate = structure.WFCTemplates[WFCTemplateIndex];
            tempLocations.Clear();
            for (int v = 0; v < availableLocations.Count; v++)
            {
                tempLocations.Add(new Vector3(availableLocations[v].x, availableLocations[v].y, 0));
            }

            // Remove Invalid Points At the Edge of Map
            for(int h  = mapHeight - 1; h > mapHeight - structureTemplate.height; h--)
            {
                for(int w = mapWidth - 1; w > mapWidth - structureTemplate.width; w--)
                {
                    tempLocations.Remove(new Vector3(w, h, 0));
                }
            }

            // Remove Invalid Points near already existing structures
            for(int s =  0; s < structureCorners.Count; s++)
            {
                //Remove invalid points to left of already existing structures
                for (int h = (int)structureCorners[i][0].y; h <= (int)structureCorners[i][1].y; h++)
                {
                    for (int w = (int)structureCorners[i][0].x - structureTemplate.width + 1;
                        w < (int)structureCorners[i][0].x; w++)
                    {
                        tempLocations.Remove(new Vector3(w, h, 0));
                    }
                }

                //Remove invalid points to BottomLeft of already existing structures
                for (int h = (int)structureCorners[i][0].y - structureTemplate.height + 1;
                    h < (int)structureCorners[i][0].y; h++)
                {
                    for (int w = (int)structureCorners[i][0].x - structureTemplate.width + 1;
                        w < (int)structureCorners[i][0].x; w++)
                    {
                        tempLocations.Remove(new Vector3(w, h, 0));
                    }
                }

                //Remove invalid points below already existing structures
                for (int h = (int)structureCorners[i][0].y - structureTemplate.height + 1;
                    h < (int)structureCorners[i][0].y; h++)
                {
                    for (int w = (int)structureCorners[i][0].x;
                        w <= (int)structureCorners[i][2].x; w++)
                    {
                        tempLocations.Remove(new Vector3(w, h, 0));
                    }
                }
            }
            Vector3 structureStartLocation = tempLocations[Random.Range(0, tempLocations.Count)];
            WaveFunctionCollapse(structureTemplate, tileBase.structures[structureIndex], structureStartLocation);
            WFCStates[,] tiles =
                new WFCStates[structureTemplate.width, structureTemplate.height];

            // Apply Structure Template Shape to Generated Structure
            // Rectangle -  All Spots valid no modification required
            if (structureTemplate.ifRectangle)
            {
                for (int h = 0; h < structureTemplate.height; h++)
                {
                    for (int w = 0; w < structureTemplate.width; w++)
                    {
                        Debug.Log(w + "," + h);
                        tiles[w, h] = WFCGenerator.rendering[w, h].GetComponent<WaveFunctionCollapseTile>().WFCState;
                        availableLocations.Remove(new Vector3(w, h, 0));
                        if (tiles[w, h] == WFCStates.Wall)
                        {
                            structureWallHolderFortesting.Add(Instantiate(structure.wallPrefabs[0], gameManager.defaultGridPosition +
                                structureStartLocation + new Vector3(w, h, 0), new Quaternion(0, 0, 0, 1f)));
                        }
                    }
                }
            }
            // nonRectangle - modification required (some spots invalid)
            else
            {
                for (int h = 0; h < structureTemplate.height; h++)
                {
                    for (int w = 0; w < structureTemplate.width; w++)
                    {
                        availableLocations.Remove(new Vector3(w, h, 0));
                        if (structureTemplate.templatePositions[w + h * structureTemplate.width].z == 1)
                        {
                            Debug.Log(w + "," + h);
                            tiles[w, h] = WFCGenerator.rendering[w, h].GetComponent<WaveFunctionCollapseTile>().WFCState;
                            if (tiles[w, h] == WFCStates.Wall)
                            {
                                structureWallHolderFortesting.Add(Instantiate(structure.wallPrefabs[0], gameManager.defaultGridPosition +
                                    structureStartLocation + new Vector3(w, h, 0), new Quaternion(0, 0, 0, 1f)));
                            }
                        }
                        else
                        {
                            tiles[w, h] = WFCStates.Open;
                        }
                    }
                }
            }

            availableTiles = new int[mapWidth, mapHeight];
            for (int h = 0; h < tiles.GetLength(1); h++)
            {
                for (int w = 0; w < tiles.GetLength(0); w++)
                {
                    if (tiles[w, h] == WFCStates.Room)
                    {
                        availableTiles[w + (int)structureStartLocation.x, h + (int)structureStartLocation.y] = 1;
                    }
                    else if (tiles[w, h] == WFCStates.Open)
                    {
                        availableTiles[w + (int)structureStartLocation.x, h + (int)structureStartLocation.y] = 2;
                    }

                }
            }
            Danger structureDangerRating = structure.encounter.GetDangerRating(extraDangerModifier);
            CreateEncounter(structure.encounter, availableTiles, structureStartLocation, structureDangerRating);
        }

        //DangerRating is for spawning units outside of structures
        Danger dangerRating = tileBase.encounter.GetDangerRating(extraDangerModifier);
        if (numStructures == 4)
        {
            if (dangerRating == Danger.Hard)
            {
                dangerRating = Danger.Easy;
            }
            else if (dangerRating == Danger.Medium)
            {
                dangerRating = Danger.None;
            }
        }
        else if (numStructures >= 2)
        {
            if (dangerRating == Danger.Hard)
            {
                dangerRating = Danger.Medium;
            }
            else if (dangerRating == Danger.Medium)
            {
                dangerRating = Danger.Easy;
            }
            else if (dangerRating == Danger.Easy)
            {
                dangerRating = Danger.None;
            }
        }

        CreateNoiseMap();
        RandomizeNoseMap();

        float setPieceFactor = tileBase.FindSetPieceFactor();
        List<Vector3> tempLocationList =  new List<Vector3>();
        for (int i = 0; i < availableLocations.Count; i++)
        {
            Vector3 location = availableLocations[i];
            if(noiseMap.GetGridObject((int)location.x, (int)location.y).value <= setPieceFactor)
            {;
                int setPieceChoice = Random.Range(0, tileBase.setPieces.Count);
                setPieceHolderForTesting.Add(Instantiate(tileBase.setPieces[setPieceChoice],
                    gameManager.defaultGridPosition + new Vector3((int)location.x, (int)location.y),
                    new Quaternion(0, 0, 0, 1f)));
                tempLocationList.Add(location);
            }
        }

        for(int i = 0; i < tempLocationList.Count; i++)
        {
            availableLocations.Remove(tempLocationList[i]);
        }

        availableTiles = new int[mapWidth, mapHeight];
        for(int i = 0; i < availableLocations.Count;  i++)
        {
            // the 2 is an equivilant to room for structures. higher priority than open 
            // will probably run faster 
            Vector3 location = availableLocations[i];
            availableTiles[(int)location.x, (int)location.y] = 2;
        }

        CreateEncounter(tileBase.encounter, availableTiles, gameManager.defaultGridPosition, dangerRating);
    }

    public void CreateEncounter(Encounter encounter, int[,] Tiles, Vector3 startingLocation, Danger dangerRating)
    {
        List<Vector2Int> availableTiles = new List<Vector2Int>();
        for (int h = 0; h < Tiles.GetLength(1); h++)
        {
            for (int w = 0; w < Tiles.GetLength(0); w++)
            {
                if (Tiles[w, h] == 1)
                {
                    availableTiles.Add(new Vector2Int(w, h));
                }
            }
        }

        int compositionChoice;
        EncounterComposition encounterComposition;
        switch (dangerRating)
        {
            case (Danger.Easy):
                compositionChoice = Random.Range(0, encounter.easyCompositions.Count);
                encounterComposition = encounter.easyCompositions[compositionChoice];
                PickEncounterComposition(encounter, encounterComposition, availableTiles, Tiles, startingLocation); 
                break;
            case (Danger.Medium):
                compositionChoice = Random.Range(0, encounter.mediumCompositions.Count);
                encounterComposition = encounter.mediumCompositions[compositionChoice];
                PickEncounterComposition(encounter, encounterComposition, availableTiles, Tiles, startingLocation);
                break;
            case (Danger.Hard):
                compositionChoice = Random.Range(0, encounter.hardCompositions.Count);
                encounterComposition = encounter.hardCompositions[compositionChoice];
                PickEncounterComposition(encounter, encounterComposition, availableTiles, Tiles, startingLocation);
                break;
            case (Danger.None):
                break;
        }
    }

    public void PickEncounterComposition(Encounter encounter, EncounterComposition encounterComposition,
        List<Vector2Int> availableTiles, int[,] FirstTiles, Vector3 startingLocation)
    {
        int[,] tiles = FirstTiles;
        int RandomUnitComposition;
        int numberOfUnits;
        GameObject unit;
        Tuple<int[,], List<Vector2Int>> updatedTileInfo;
        if (encounterComposition.dangerType.Count != encounterComposition.encounterAmount.Count)
        {
            Debug.LogError("Redo Encounter: " + encounter.name);
            Debug.LogError("Redo Encounter Composition: " + encounterComposition.name);
        }
        for (int k = 0; k < encounterComposition.dangerType.Count; k++)
        {
            switch (encounterComposition.dangerType[k])
            {
                case (Danger.Easy):
                    for (int u = 0; u < encounterComposition.encounterAmount[k]; u++)
                    {
                        RandomUnitComposition = Random.Range(0, encounter.easyUnitEncounters.Count);
                        numberOfUnits = encounter.easyUnitEncounters[RandomUnitComposition].numberOfUnits[u].GetRandomNumberInRange();
                        unit = encounter.easyUnitEncounters[RandomUnitComposition].units[u];
                        updatedTileInfo = PlaceUnits(numberOfUnits, availableTiles, tiles, unit, startingLocation);
                        tiles = updatedTileInfo.Item1;
                        availableTiles = updatedTileInfo.Item2;
                    }
                    break;
                case (Danger.Medium):
                    for (int u = 0; u < encounterComposition.encounterAmount[k]; u++)
                    {
                        RandomUnitComposition = Random.Range(0, encounter.mediumUnitEncounters.Count);
                        numberOfUnits = encounter.mediumUnitEncounters[RandomUnitComposition].numberOfUnits[u].GetRandomNumberInRange();
                        unit = encounter.mediumUnitEncounters[RandomUnitComposition].units[u];
                        updatedTileInfo = PlaceUnits(numberOfUnits, availableTiles, tiles, unit, startingLocation);
                        tiles = updatedTileInfo.Item1;
                        availableTiles = updatedTileInfo.Item2;
                    }
                    break;
                case (Danger.Hard):
                    for (int u = 0; u < encounterComposition.encounterAmount[k]; u++)
                    {
                        RandomUnitComposition = Random.Range(0, encounter.hardUnitEncounters.Count);
                        numberOfUnits = encounter.hardUnitEncounters[RandomUnitComposition].numberOfUnits[u].GetRandomNumberInRange();
                        unit = encounter.hardUnitEncounters[RandomUnitComposition].units[u];
                        updatedTileInfo = PlaceUnits(numberOfUnits, availableTiles, tiles, unit, startingLocation);
                        tiles = updatedTileInfo.Item1;
                        availableTiles = updatedTileInfo.Item2;
                    }
                    break;
            }
        }
    }

    public Tuple<int[,], List<Vector2Int>> PlaceUnits(int numberOfUnits, List<Vector2Int> availableTiles, int[,] Tiles
        , GameObject unit, Vector3 startingLocation)
    {
        int[,] modifiedTiles = Tiles;
        Vector2Int firstUnitSpawnLocation;
        List<Vector2Int> unitPlacements;
        List<Vector2Int> modifiedAvailableTiles = availableTiles;

        firstUnitSpawnLocation = availableTiles[Random.Range(0, availableTiles.Count)];
        unitPlacements = FindNearestOpenSpaceMapGeneration.
            FindEmptySpace(numberOfUnits, firstUnitSpawnLocation, modifiedTiles, false);
        if (unitPlacements == null)
        {
            unitPlacements = FindNearestOpenSpaceMapGeneration.
            FindEmptySpace(numberOfUnits, firstUnitSpawnLocation, modifiedTiles, true);
            if (unitPlacements == null)
            {
                Debug.LogError("Hey Somethings messedup Fix Plz");
            }
        }
        for (int i = 0; i < unitPlacements.Count; i++)
        {
            Vector3 unitSpawnLocation = startingLocation + new Vector3(unitPlacements[i].x, unitPlacements[i].y, 0);
            modifiedTiles[unitPlacements[i].x, unitPlacements[i].y] = 3;
            modifiedAvailableTiles.Remove(unitPlacements[i]);
            unitHolderForTesting.Add(Instantiate(unit, unitSpawnLocation, new Quaternion(0, 0, 0, 1f)));
        }
        return new Tuple<int[,], List<Vector2Int>>(modifiedTiles, modifiedAvailableTiles);
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorTestor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerator me = (MapGenerator)target;
        if (GUILayout.Button("Test"))
        {
            me.ResetTesting();
            me.GenerateTile(null, null, 0, true);
        }
        DrawDefaultInspector();
    }
}
#endif