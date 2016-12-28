using UnityEngine;
using System.Collections;

public class RoomGen : MonoBehaviour
{
    // The type of tile that will be laid in a specific position.
    public enum TileType
    {
        Wall, Floor
    }

    // The type of Event that will be laid in a specific position.
    public enum EventType
    {
        None, Enemy, Friendly
    }

    //All Fields are Set up in Hierarchy Through RoomGenEditor.cs

    public int columns = 10, rows = 10; //The number of rows and columns for the tiles (How many Tiles)

    public int w_min, w_max; //width
    public int h_min, h_max; //height
    public int e_min, e_max; //enemies

    public RangeAttribute roomWidth, roomHeight, enemyCount;
    public int friendlyCount;

    public bool hasFlatTop = true;
    public float cellSize = 1.0f;

    public Sprite[] floorTiles;                           // An array of floor tile prefabs.
    public Sprite[] wallTiles;                            // An array of wall tile prefabs.
    public GameObject[] enemyUnits;                              // An array of the random enemies that can appear
    public GameObject[] friendlyUnits;
    public HexGrid room;
    public CameraScript cam;

    private TileType[][] tiles;                               // A jagged array of tile types representing the board, like a grid.
    private EventType[][] events;                               // A jagged array of tile types representing the board, like a grid.
    private GameObject boardHolder;                           // GameObject that acts as a container for all other tiles.
    private GameObject eventHolder;
    private GameObject enemyHolder;
    private GameObject friendlyHolder;
    private HexGridBoard gridMap;

    private int width, height; //The saved width and heights on runtime
    private int xpos, ypos; //where the 0,0 point of teh room is relative to the entire map

    private void Start()
    {
        roomWidth = new RangeAttribute(w_min, w_max);
        roomHeight = new RangeAttribute(h_min, h_max);
        enemyCount = new RangeAttribute(e_min, e_max);

        boardHolder = new GameObject("Board Holder");
        gridMap = boardHolder.AddComponent<HexGridBoard>();

        enemyHolder = new GameObject("Enemy Holder");
        eventHolder = new GameObject("Event Holder");
        friendlyHolder = new GameObject("Friendly Holder");

        SetupTilesAndEventsArray();

        CreateRoom();

        SetEventValues();

        SetTileValues();

        InstantiateTiles();

        InstantiateEvents();

        createCombatManager();
    }

    /// <summary>
    /// Creates an empty Tile array
    /// </summary>
    void SetupTilesAndEventsArray()
    {
        // Set the tiles jagged array to the correct width.
        tiles = new TileType[columns + 1][];
        events = new EventType[columns + 1][];

        // Go through all the tile arrays...
        for (int i = 0; i < tiles.Length; i++)
        {
            // ... and set each tile array is the correct height.
            tiles[i] = new TileType[rows + 1];
            events[i] = new EventType[rows + 1];
        }
    }

    /// <summary>
    /// Creates and Sets up the Room
    /// </summary>
    void CreateRoom()
    {
        room = ScriptableObject.CreateInstance<HexGrid>();
        room.cells = new System.Collections.Generic.List<Cell>();
        room.hasFlatTop = hasFlatTop;
        room.cellSize = cellSize;

        width = (int)Random.Range(roomWidth.min, roomWidth.max);
        height = (int)Random.Range(roomWidth.min, roomHeight.max);
        room.createCellGroup(0, 0, columns, rows);
        room.linkCells();
        gridMap.GenerateMap(room);

        xpos = Mathf.RoundToInt(columns / 2f - width / 2f);
        ypos = Mathf.RoundToInt(rows / 2f - height / 2f);

    }

    void SetTileValues()
    {
        // ... and for each room go through it's width.
        for (int i = 0; i < width; i++)
        {
            int xCoord = xpos + i;

            // For each horizontal tile, go up vertically through the room's height.
            for (int j = 0; j < height; j++)
            {
                int yCoord = ypos + j;

                // The coordinates in the jagged array are based on the room's position and it's width and height.
                tiles[xCoord][yCoord] = TileType.Floor;
            }
        }
    }

    void SetEventValues()
    {
        setEnemyTiles();
        setFriendlyTiles();
    }

    void InstantiateTiles()
    {
        // Go through all the tiles in the jagged array...
        for (int i = 0; i < tiles.Length; i++)
        {
            for (int j = 0; j < tiles[i].Length; j++)
            {
                if (tiles[i][j] == TileType.Floor)
                {
                    MapCell floor = InstantiateMapCell(floorTiles, i, j);
                    floor.passable = true;
                }

                if (tiles[i][j] == TileType.Wall)
                {
                    MapCell wall = InstantiateMapCell(wallTiles, i, j);
                    wall.passable = false;
                }
            }
        }
    }

    void InstantiateEvents()
    {
        // Go through all the tiles in the jagged array...
        for (int i = 0; i < events.Length; i++)
        {
            for (int j = 0; j < events[i].Length; j++)
            {
                // If the tile type is Wall...
                if (events[i][j] == EventType.Enemy)
                {
                    setupEnemy(i - (columns / 2), j - (rows / 2));
                }
                if (events[i][j] == EventType.Friendly)
                {
                    setupFriendly(i - (columns / 2), j - (rows / 2));
                }
            }
        }
    }

    /// <summary>
    /// Instantiates a random Prefab from a prefab array
    /// </summary>
    /// <param name="prefabs">The prefab array</param>
    /// <param name="xCoord"></param>
    /// <param name="yCoord"></param>
    /// <param name="parent"></param>
    GameObject InstantiateFromArray(GameObject[] prefabs, int xCoord, int yCoord, GameObject parent)
    {
        // Create a random index for the array.
        int randomIndex = Random.Range(0, prefabs.Length);

        // The position to be instantiated at is based on the coordinates.
        MapCell cell = gridMap.getCellAtPos(xCoord, yCoord);
        Vector3 position = new Vector3(cell.transform.position.x, cell.transform.position.y, 0f);

        // Create an instance of the prefab from the random index of the array.
        GameObject instance = Instantiate(prefabs[randomIndex], position, Quaternion.identity) as GameObject;

        // Set the tile's parent to the board holder.
        instance.transform.parent = parent.transform;
        return instance;
    }

    MapCell InstantiateMapCell(Sprite[] sprites, int xCoord, int yCoord)
    {
        // Create a random index for the array.
        int randomIndex = Random.Range(0, sprites.Length);

        MapCell cell = gridMap.getCellAtPos(xCoord - columns / 2, yCoord - rows / 2);
        cell.GetComponent<SpriteRenderer>().sprite = sprites[randomIndex];

        return cell;
    }

    void createCombatManager()
    {
        GameObject cm = new GameObject("Combat Manager");
        CombatManager manager = cm.AddComponent<CombatManager>();

        manager.cam = cam;
        manager.board = gridMap;
    }

    void setupEnemy(int x, int y)
    {
        GameObject unit = InstantiateFromArray(enemyUnits, x, y, enemyHolder);
        MapUnit m_unit = unit.GetComponent<MapUnit>();
        m_unit.isPlayerControlled = false;
        m_unit.isEnemy = true;
        m_unit.startPosX = x;
        m_unit.startPosY = y;
        m_unit.pos = gridMap.getCellAtPos(x, y);
    }

    void setupFriendly(int x, int y)
    {
        GameObject unit = InstantiateFromArray(friendlyUnits, x, y, friendlyHolder);
        MapUnit m_unit = unit.GetComponent<MapUnit>();
        m_unit.isPlayerControlled = true;
        m_unit.isEnemy = false;
        m_unit.startPosX = x;
        m_unit.startPosY = y;
        m_unit.pos = gridMap.getCellAtPos(x, y);
    }

    void setEnemyTiles()
    {
        int enemiesPlaced = 0;

        int enemies = (int)Random.Range(enemyCount.min, enemyCount.max);

        while (enemiesPlaced < enemies)
        {
            int posx_min = xpos;
            int posx_max = xpos + (int)((width) * (1f / enemies));

            int posy_min = ypos;
            int posy_max = xpos + (int)((height) * (1f / enemies));

            int epos_x, epos_y;

            epos_x = (int)(Random.Range(posx_min, posx_max));
            epos_y = (int)(Random.Range(posy_min, posy_max));


            if (events[epos_x][epos_y] == EventType.None)
            {
                events[epos_x][epos_y] = EventType.Enemy;
                enemiesPlaced++;
            }
        }
    }

    void setFriendlyTiles()
    {
        int friendliesPlaced = 0;

        while (friendliesPlaced < friendlyCount)
        {
            int posx_min = (int)((width + xpos) * ((1 - (1f / friendlyCount))));
            int posx_max = xpos + width;

            int posy_min = (int)((height + ypos) * ((1 - (1f / friendlyCount))));
            int posy_max = ypos + height;

            int epos_x, epos_y;

            epos_x = (int)(Random.Range(posx_min, posx_max));
            epos_y = (int)(Random.Range(posy_min, posy_max));


            if (events[epos_x][epos_y] == EventType.None)
            {
                events[epos_x][epos_y] = EventType.Friendly;
                friendliesPlaced++;
            }
        }
    }
}
