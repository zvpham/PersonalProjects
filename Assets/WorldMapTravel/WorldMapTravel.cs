using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;

public class WorldMapTravel : MonoBehaviour
{
    public Vector2Int currentMapPosition;
    public Vector2Int startingMapPosition;
    public GameObject playerMapModel;

    public Player player;
    public AllDirections allDirections;
    public PlayerWorldMap playerWorldMap;
    public MapManager mapManager;
    public InputManager inputManager;
    private KeyBindings keybinds;
    public static WorldMapTravel Instance;

    public void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Found more than one WorldMapTravel in the Scence");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    public void OnEnable()
    {
        Debug.Log("Who is enablikng me BRo");
    }

    public void OnDisable()
    {
        Debug.Log("Who is disabling me BRo");
    }
    void Start()
    {
        inputManager = InputManager.instance;
        keybinds = KeyBindings.instance;
        mapManager = MapManager.Instance;
        player = Player.Instance;
        enabled = false;
    }

    void Update()
    {
        //Moving On the World map
        for (int i = 0; i < allDirections.Directions.Length; i++)
        {
            if (inputManager.GetKeyDownTargeting(allDirections.Directions[i].directionName))
            {
                Vector3 moveDirection =  allDirections.Directions[i].GetDirection();
                Vector2Int moveDirectionModified = new Vector2Int((int)moveDirection.x,(int) moveDirection.y);
                if (mapManager.AttemptToMoveWorldMapPosition(moveDirectionModified))
                {
                    Vector2Int newPosition = currentMapPosition + moveDirectionModified;
                    currentMapPosition = newPosition;
                    Vector2 currentMapPositionModified = new Vector2(currentMapPosition.x, currentMapPosition.y);
                    playerMapModel.transform.localPosition = currentMapPositionModified;
                }
            }
        }

        for(int i = 0; i < keybinds.worldMapTravelKeyBinds.Count; i++)
        {

        }
        if (inputManager.GetKeyDownWorldMapTravel(WorldMapTravelIntputName.EnterTile))
        {
            EnterTile();
        }
    }

    public void EnterTile()
    {
        if(currentMapPosition == startingMapPosition)
        {
            player.UseWorldMap();
        }
        else
        {
            player.UseWorldMap();
            mapManager.EnterTile();
        }
    }

    public void StartWorldMapTravel(Sprite playerSprite)
    {
        Debug.Log("Starting World Map");
        playerMapModel.GetComponent<SpriteRenderer>().sprite = playerSprite;
        currentMapPosition = mapManager.currentMapPosition;
        startingMapPosition = currentMapPosition;
        Vector2 currentMapPositionModified = new Vector2(currentMapPosition.x, currentMapPosition.y);
        playerMapModel.transform.localPosition = currentMapPositionModified;
        enabled = true;
    }

    public void EndWorldMapTravel()
    {
        enabled = false;
    }
}
