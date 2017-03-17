using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    public int outerRadius;
    public int radius;

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
    public ControlScript[] enemyScripts;
    public ControlScript playerScript;
    public ControlScript[] friendlyScripts;
    public bool isAuto;
    public bool radial;

    //TileType + Coord(based on tile)
    private Dictionary<Cell, TileType> tileMap;
    private Dictionary<Cell, EventType> eventMap;

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

        CreateRoom();

        SetEventValues();

        SetTileValues();

        InstantiateTiles();

        InstantiateEvents();

        createCombatManager();
    }

    /// <summary>
    /// Creates and Sets up the Room
    /// </summary>
    void CreateRoom()
    {
        room = ScriptableObject.CreateInstance<HexGrid>();
        room.cells = new List<Cell>();
        room.hasFlatTop = hasFlatTop;
        room.cellSize = cellSize;

        if (radial)
        {
            room.createCellGroup(0, 0, outerRadius);
            width = radius * 2;
            height = radius * 2;
        }
        else
        {
            width = (int)Random.Range(roomWidth.min, roomWidth.max);
            height = (int)Random.Range(roomWidth.min, roomHeight.max);
            room.createCellGroup(0, 0, columns, rows);
        }

        room.linkCells();
        gridMap.cells = new List<MapCell>();
        gridMap.GenerateMap(room);

        xpos = Mathf.RoundToInt(columns / 2f - width / 2f);
        ypos = Mathf.RoundToInt(rows / 2f - height / 2f);

    }

    void SetTileValues()
    {
        tileMap = new Dictionary<Cell, TileType>();
        //Assign Each Cell to default Floor Type
        foreach (Cell cell in room.cells)
        {
            if (radial)
            {
                if (Cell.getDist(cell, room.getCellAtPos(0, 0)) <= radius)
                {
                    tileMap.Add(cell, TileType.Floor);
                }
                else
                {
                    tileMap.Add(cell, TileType.Wall);
                }
            }
            else
            {
                if (cell.x <= width / 2 && cell.y <= height / 2 &&
                   cell.x >= -width / 2 && cell.y >= -height / 2)
                {
                    tileMap.Add(cell, TileType.Floor);
                }
                else
                {
                    tileMap.Add(cell, TileType.Wall);
                }
            }

        }

    }

    void SetEventValues()
    {
        eventMap = new Dictionary<Cell, EventType>();
        foreach (Cell cell in room.cells)
        {
            eventMap.Add(cell, EventType.None);
        }

        setEnemyTiles();
        setFriendlyTiles();
    }

    void InstantiateTiles()
    {
        List<Cell> keys = new List<Cell>(tileMap.Keys);
        foreach (Cell cell in keys.ToArray())
        {
            if (tileMap[cell] == TileType.Floor)
            {
                MapCell floor = InstantiateMapCell(floorTiles, cell);
                floor.passable = true;
            }

            if (tileMap[cell] == TileType.Wall)
            {
                MapCell wall = InstantiateMapCell(wallTiles, cell);
                wall.passable = false;
            }

        }
    }

    void InstantiateEvents()
    {
        List<Cell> keys = new List<Cell>(eventMap.Keys);
        foreach (Cell cell in keys.ToArray())
        {
            // If the tile type is Wall...
            if (eventMap[cell] == EventType.Enemy)
            {
                setupEnemy(cell);
            }
            if (eventMap[cell] == EventType.Friendly)
            {
                setupFriendly(cell);
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
    GameObject InstantiateFromArray(GameObject[] prefabs, Cell cell, GameObject parent)
    {
        // Create a random index for the array.
        int randomIndex = Random.Range(0, prefabs.Length);

        MapCell m_cell = gridMap.getCellAtPos(cell.x, cell.y);
        // The position to be instantiated at is based on the coordinates.
        Vector3 position = new Vector3(m_cell.transform.position.x, m_cell.transform.position.y, 0f);

        // Create an instance of the prefab from the random index of the array.
        GameObject instance = Instantiate(prefabs[randomIndex], position, Quaternion.identity) as GameObject;

        // Set the tile's parent to the board holder.
        instance.transform.parent = parent.transform;
        return instance;
    }

    MapCell InstantiateMapCell(Sprite[] sprites, Cell cell)
    {
        // Create a random index for the array.
        int randomIndex = Random.Range(0, sprites.Length);

        MapCell m_cell = gridMap.getCellAtPos(cell.x, cell.y);
        m_cell.GetComponent<SpriteRenderer>().sprite = sprites[randomIndex];

        return m_cell;
    }

    void createCombatManager()
    {
        GameObject cm = new GameObject("Combat Manager");
        CombatManager manager = cm.AddComponent<CombatManager>();

        manager.cam = cam;
        manager.board = gridMap;
        manager.auto = isAuto;
    }

    void setupEnemy(Cell cell)
    {
        GameObject unit = InstantiateFromArray(enemyUnits, cell, enemyHolder);
        MapUnit m_unit = unit.GetComponent<MapUnit>();
        //Add the Component for Control Script Randomly

        m_unit.isPlayerControlled = false;

        int randomIndex = Random.Range(0, enemyScripts.Length);
        m_unit.controlScript = enemyScripts[randomIndex];

        m_unit.isEnemy = true;
        m_unit.startPosX = cell.x;
        m_unit.startPosY = cell.y;
        m_unit.pos = gridMap.getCellAtPos(cell.x, cell.y);
    }

    void setupFriendly(Cell cell)
    {
        GameObject unit = InstantiateFromArray(friendlyUnits, cell, friendlyHolder);
        MapUnit m_unit = unit.GetComponent<MapUnit>();
        m_unit.isPlayerControlled = !isAuto;
        if (!isAuto)
            m_unit.controlScript = playerScript;
        else
        {
            //add random script from arr

            int randomIndex = Random.Range(0, friendlyScripts.Length);
            m_unit.controlScript = friendlyScripts[randomIndex];
        }
        m_unit.isEnemy = false;
        m_unit.startPosX = cell.x;
        m_unit.startPosY = cell.y;
        m_unit.pos = gridMap.getCellAtPos(cell.x, cell.y);
    }

    void setEnemyTiles()
    {
        int enemiesPlaced = 0;

        int enemies = (int)Random.Range(enemyCount.min, enemyCount.max);
        int posx_min, posx_max, posy_min, posy_max;

        if (radial)
        {
            posx_min = radius - (radius / 4);
            posx_max = radius;

            posy_min = -radius;
            posy_max = radius;
        }
        else
        {
            posx_min = -(width / 2);
            posx_max = -((width / 2) - (width / 4));

            posy_min = -(height / 2);
            posy_max = -((height / 2) - (height / 4));
        }

        while (enemiesPlaced < enemies)
        {

            int posx = (int)Random.Range(posx_min, posx_max);
            int posy = (int)Random.Range(posy_min, posy_max);

            List<Cell> keys = new List<Cell>(eventMap.Keys);
            foreach (Cell cell in keys.ToArray())
            {

                if (/* cell.x <= posx_max && cell.x >= posx_min
                    && cell.y <= posy_max && cell.y >= posy_min */
                    cell.x == posx && cell.y == posy)
                {
                    if (!radial || Cell.getDist(cell, room.getCellAtPos(0, 0)) <= radius)
                    {
                        if (eventMap[cell] == EventType.None)
                        {
                            eventMap[cell] = EventType.Enemy;
                            enemiesPlaced++;
                            break;
                        }
                    }
                }
            }

        }
    }

    void setFriendlyTiles()
    {
        int friendliesPlaced = 0;

        int posx_min, posx_max, posy_min, posy_max;

        if (radial)
        {
            posx_min = -radius;
            posx_max = -radius + (radius / 4);

            posy_min = -radius;
            posy_max = radius;
        }
        else
        {
            posx_min = (width / 2) - (width / 4);
            posx_max = (width / 2);

            posy_min = (height / 2) - (height / 4);
            posy_max = (height / 2);
        }

        while (friendliesPlaced < friendlyCount)
        {

            int posx = (int)Random.Range(posx_min, posx_max);
            int posy = (int)Random.Range(posy_min, posy_max);

            List<Cell> keys = new List<Cell>(eventMap.Keys);
            foreach (Cell cell in keys.ToArray())
            {

                if (/* cell.x <= posx_max && cell.x >= posx_min
                    && cell.y <= posy_max && cell.y >= posy_min */
                    cell.x == posx && cell.y == posy)
                {
                    if (!radial || Cell.getDist(cell, room.getCellAtPos(0, 0)) <= radius)
                    {
                        if (eventMap[cell] == EventType.None)
                        {
                            eventMap[cell] = EventType.Friendly;
                            friendliesPlaced++;
                            break;
                        }
                    }
                }
            }
        }
    }
}
