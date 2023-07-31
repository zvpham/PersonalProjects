/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyTest : MonoBehaviour
{
    [SerializeField]
    private Tilemap groundTilemap;

    [SerializeField]
    private Tilemap collisionTilemap;

    private Vector2 newPosition = new Vector2(0.0f, 0.0f);
    private GameManager gameManager;

    public int strength = 10;
    public int speed = 10;
    public int endurance = 10;
    public int will = 10;
    public int intelligence = 10;
    public int charisma = 10;

    public int index;
    public static EnemyTest instance;
    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }

        else if (instance != this)
        {
            Destroy(this);
        }
        DontDestroyOnLoad(this);


        gameManager = GameManager.instance;
        gameManager.speeds.Add(this.speed);
        gameManager.priority.Add(this.speed * gameManager.baseTurnTime);
        gameManager.scripts.Add(this);
        gameManager.enemies.Add(this);
        gameManager.locations.Add(transform.position);

        index = gameManager.speeds.Count;
        Debug.Log("Player Start");
        enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        newPosition.Set(-1f, 0f);
        Move(newPosition);
        TurnEnd();
    }

    public void Move(Vector2 direction)
    {
        if (CanMove(direction))
        {
            transform.position += (Vector3)direction;
            gameManager.locations[0] = transform.position;
        }
    }

    public bool CanMove(Vector2 direction)
    {
        Vector3Int gridPosition = groundTilemap.WorldToCell(transform.position + (Vector3)direction);
        if (!groundTilemap.HasTile(gridPosition) || collisionTilemap.HasTile(gridPosition))
        {
            return false;
        }
        return true;
    }

    public void TurnEnd()
    {
        enabled = false;
    }
}
*/

/*
 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class Player : Unit
{
    private GameManager gameManager;
    private InputManager inputManager;
    // Start is called before the first frame update
    void Start()
    {

        else if (instance != this)
        {
            Destroy(this);
        }
        DontDestroyOnLoad(this);
        
gameManager = GameManager.instance;
gameManager.speeds.Insert(0, this.speed);
gameManager.priority.Insert(0, this.speed * gameManager.baseTurnTime);
gameManager.scripts.Insert(0, this);
gameManager.locations.Insert(0, transform.position);
inputManager = InputManager.instance;
Debug.Log("Player Start");
enabled = false;
    }

    // Update is called once per frame
    void Update()
{
    if (inputManager.GetKeyDown("Move_Northeast"))
    {
        newPosition.Set(1f, 1f);
        Movement.Move(newPosition);
        TurnEnd();
    }

    else if (inputManager.GetKeyDown("Move_North"))
    {
        newPosition.Set(0f, 1f);
        Movement.Move(newPosition);
        TurnEnd();
    }

    else if (inputManager.GetKeyDown("Move_Northwest"))
    {
        newPosition.Set(-1f, 1f);
        Movement.Move(newPosition);
        TurnEnd();
    }

    else if (inputManager.GetKeyDown("Move_West"))
    {
        newPosition.Set(-1f, 0f);
        Movement.Move(newPosition);
        TurnEnd();
    }

    else if (inputManager.GetKeyDown("Move_Southwest"))
    {
        newPosition.Set(-1f, -1f);
        Movement.Move(newPosition);
        TurnEnd();
    }

    else if (inputManager.GetKeyDown("Move_South"))
    {
        newPosition.Set(0f, -1f);

        Movement.Move(newPosition);
        TurnEnd();
    }

    else if (inputManager.GetKeyDown("Move_Southeast"))
    {
        newPosition.Set(1f, -1f);
        Movement.Move(newPosition);
        TurnEnd();
    }
    else if (inputManager.GetKeyDown("Move_East"))
    {
        newPosition.Set(1f, 0f);
        Movement.Move(newPosition);
        TurnEnd();
    }
}
}
*/

/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyTest : Unit
{

    private GameManager gameManager;
    // Start is called before the first frame update
    void Start()
    {


        else if (instance != this)
        {
            Destroy(this);
        }
        DontDestroyOnLoad(this);

gameManager = GameManager.instance;
gameManager.speeds.Add(this.speed);
gameManager.priority.Add(this.speed * gameManager.baseTurnTime);
gameManager.scripts.Add(this);
gameManager.locations.Add(transform.position);

Debug.Log("Player Start");
enabled = false;
    }

    // Update is called once per frame
    void Update()
{
    newPosition.Set(-1f, 0f);
    Movement.Move(newPosition);
    TurnEnd();
}

}

*/