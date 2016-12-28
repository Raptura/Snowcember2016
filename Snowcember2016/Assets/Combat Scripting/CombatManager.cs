using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public CameraScript cam;
    public HexGridBoard board;

    //[HideInInspector]
    public List<MapUnit> units;
    private List<MapUnit> turnOrder;
    private int turnPlayer = 0;

    [HideInInspector]
    public int turn;

    private Vector2 cursorPos;
    private Cell.Direction direction;

    float lastMoved;
    float moveDelay = 0.25f;

    private enum HighlightMode
    {
        Standard,
        Attack,
        Movement
    }
    private HighlightMode highlightMode;

    void Start()
    {
        lastMoved = Mathf.NegativeInfinity;
        turn = 1;
        foreach (MapUnit m_unit in FindObjectsOfType<MapUnit>())
        {
            units.Add(m_unit);
        }
        turnOrder = getTurnOrder();
        cursorPos = new Vector2(0, 0);
        turnPlayer = 0;

        foreach (MapUnit unit in units)
        {
            unit.pos = board.getCellAtPos(unit.startPosX, unit.startPosY);
        }
        switchTarget(getTurnPlayer());
        getTurnPlayer().StartTurn(this);
        highlightMode = HighlightMode.Standard;
    }

    void Update()
    {
        manageTurns();
        if (getTurnPlayer().isPlayerControlled)
        {
            handleInput();
        }
        else
        {
            switchTarget(getTurnPlayer());
        }
        manageHighlights();
    }

    void OnGUI()
    {
        if (getTurnPlayer().isPlayerControlled)
        {
            if (GUI.Button(new Rect(0, 0, 200, 100), "View Move Range"))
            {
                highlightMode = HighlightMode.Movement;
            }
            if (getTurnPlayer().canMove)
            {
                if (GUI.Button(new Rect(200, 0, 100, 100), "Move"))
                {
                    getTurnPlayer().Move(getCurrCell());
                }
            }

            if (GUI.Button(new Rect(0, 100, 200, 100), "Check Attack Range"))
            {
                highlightMode = HighlightMode.Attack;
            }

            if (getTurnPlayer().canAttack)
            {
                if (GUI.Button(new Rect(200, 100, 100, 100), "Attack"))
                {
                    getTurnPlayer().Attack(getCurrCell(), this);
                }
            }
            if (GUI.Button(new Rect(0, 200, 100, 100), "End Turn"))
            {
                highlightMode = HighlightMode.Standard;
                getTurnPlayer().EndTurn();
            }
            if (GUI.Button(new Rect(0, 300, 100, 100), "Center Cursor"))
            {
                switchTarget(getTurnPlayer());
            }
            string UIText = "Health " + getTurnPlayer().health + "/" + getTurnPlayer().unitScript.maxHealth;
            GUI.TextField(new Rect(0, 400, 200, 100), UIText);
        }
    }

    /// <summary>
    /// Gets the turn order in order of the speed stat. 
    /// Higher speed stats have lower indecies
    /// </summary>
    /// <returns></returns>
    private List<MapUnit> getTurnOrder()
    {
        List<MapUnit> result = new List<MapUnit>();
        MapUnit[] arr = units.ToArray();

        bool sorted = false;
        while (!sorted)
        {
            sorted = true;
            for (int i = 1; i < arr.Length; i++)
            {
                if (arr[i].unitScript.speed > arr[i - 1].unitScript.speed)
                {
                    MapUnit temp = arr[i];
                    MapUnit temp2 = arr[i - 1];
                    arr[i - 1] = temp;
                    arr[i] = temp2;

                    sorted = false;
                }
            }
        }

        for (int i = 0; i < arr.Length; i++)
        {
            result.Add(arr[i]);
        }

        return result;

    }

    void manageHighlights()
    {
        //Always highlight current turn player
        highlightCurrentTurnPlayer();

        if (highlightMode == HighlightMode.Movement)
            highlightCurrentPlayerMovement();
        if (highlightMode == HighlightMode.Attack)
            highlightCurrentPlayerAttack();

        highlightCurrentCell();
    }

    void highlightCurrentCell()
    {
        MapCell highlightCell = board.getCellAtPos((int)cursorPos.x, (int)cursorPos.y);
        highlightCell.GetComponent<SpriteRenderer>().color = Color.yellow;
    }

    void highlightCurrentTurnPlayer()
    {
        getTurnPlayer().GetComponent<SpriteRenderer>().color = Color.magenta;
    }

    void highlightCurrentPlayerMovement()
    {
        MapUnit highlightUnit = getTurnPlayer();
        MapCell centric = board.getCellAtPos(highlightUnit.pos.cellData.x, highlightUnit.pos.cellData.y);
        List<MapCell> cellsToHighlight = board.getAllInRadius(highlightUnit.unitScript.mov, centric);

        foreach (MapCell m_cell in cellsToHighlight)
        {
            m_cell.GetComponent<SpriteRenderer>().color = new Color(150f / 255f, 255f / 255f, 0f);
        }
    }

    void highlightCurrentPlayerAttack()
    {
        if (getTurnPlayer().unitScript.attackType == Unit.AttackType.Point || getTurnPlayer().unitScript.attackType == Unit.AttackType.Spread)
        {
            MapCell highlightCell = getCurrCell();
            MapUnit highlightUnit = getTurnPlayer();
            MapCell centric = board.getCellAtPos(highlightUnit.pos.cellData.x, highlightUnit.pos.cellData.y);
            List<MapCell> cellsToHighlight = board.getAllInRadius(highlightUnit.unitScript.range, centric);

            foreach (MapCell m_cell in cellsToHighlight)
            {
                m_cell.GetComponent<SpriteRenderer>().color = new Color(255f / 255f, 192f / 255f, 203f / 255f);
            }
        }

        if (getTurnPlayer().unitScript.attackType == Unit.AttackType.Line)
        {
            MapCell cell = getTurnPlayer().pos; //starting pos, at the player's pos
            MapCell cursorCell = getCurrCell();

            int dist = Cell.getDist(cell.cellData, cursorCell.cellData);
            Cell.Direction dir = cell.cellData.getDirection(cursorCell.cellData);

            for (int i = 0; i < dist; i++)
            {
                if (cell.cellData.getNeighbor(dir) != null)
                {
                    cell = board.getCellAtPos(cell.cellData.getNeighbor(dir).x, cell.cellData.getNeighbor(dir).y);

                    if (dist - Cell.getDist(cell.cellData, cursorCell.cellData) < getTurnPlayer().unitScript.range)
                        cell.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
                    else
                        cell.gameObject.GetComponent<SpriteRenderer>().color = Color.cyan;
                }
                else
                    break;
            }
        }

        if (getTurnPlayer().unitScript.attackType == Unit.AttackType.Spread)
        {
            MapCell playerPos = getTurnPlayer().pos; //starting pos, at the player's pos
            MapCell cursorCell = getCurrCell();

            int dist = Cell.getDist(playerPos.cellData, cursorCell.cellData);

            foreach (Cell surroundingcell in cursorCell.cellData.getNeighbors())
            {
                MapCell obj = board.getCellAtPos(surroundingcell.x, surroundingcell.y);

                if (obj != null)
                {
                    if (dist - Cell.getDist(obj.cellData, cursorCell.cellData) < getTurnPlayer().unitScript.range)
                        obj.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
                    else
                        obj.gameObject.GetComponent<SpriteRenderer>().color = Color.cyan;
                }
            }
        }
    }

    void manageTurns()
    {
        if (getTurnPlayer().conductedTurn == false && !getTurnPlayer().isDead)
        {
            getTurnPlayer().TurnUpdate();
        }
        else
        {
            if (turnPlayer != turnOrder.Count - 1)
                turnPlayer++;
            else
            {
                turnPlayer = 0;
                turn++;
            }
            getTurnPlayer().StartTurn(this);
            switchTarget(getTurnPlayer());
        }
    }

    /// <summary>
    /// Input Handler
    /// </summary>
    void handleInput()
    {
        if (Time.time - lastMoved > moveDelay)
        {
            if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.D))
            {
                direction = Cell.Direction.NorthEast;
                moveForward();
                lastMoved = Time.time;
            }

            else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A))
            {
                direction = Cell.Direction.NorthWest;
                moveForward();
                lastMoved = Time.time;
            }

            else if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D))
            {
                direction = Cell.Direction.SouthEast;
                moveForward();
                lastMoved = Time.time;
            }

            else if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.A))
            {
                direction = Cell.Direction.SouthWest;
                moveForward();
                lastMoved = Time.time;
            }

            else if (Input.GetKey(KeyCode.W))
            {
                direction = Cell.Direction.North;
                moveForward();
                lastMoved = Time.time;
            }

            else if (Input.GetKey(KeyCode.S))
            {
                direction = Cell.Direction.South;
                moveForward();
                lastMoved = Time.time;
            }


            else if (Input.GetKey(KeyCode.A))
            {
                direction = Cell.Direction.West;
                moveForward();
                lastMoved = Time.time;
            }


            else if (Input.GetKey(KeyCode.D))
            {
                direction = Cell.Direction.East;
                moveForward();
                lastMoved = Time.time;
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    void moveForward()
    {
        MapCell m_cell = board.getCellAtPos((int)cursorPos.x, (int)cursorPos.y);
        Cell neighbor = m_cell.cellData.getNeighbor(direction);
        if (neighbor != null)
            m_cell = board.getCellAtPos(neighbor.x, neighbor.y);
        else
            return;

        cursorPos = new Vector2(m_cell.cellData.x, m_cell.cellData.y);
        cam.target = m_cell.transform;
    }

    /// <summary>
    /// Switches the target of the camera as well as the cursor position to a desired MapUnit obj
    /// </summary>
    /// <param name="unit">The target Map Unit</param>
    void switchTarget(MapUnit unit)
    {
        //Change the cursor position to the current 
        cursorPos = new Vector2(unit.pos.cellData.x, unit.pos.cellData.y);
        cam.target = unit.pos.transform;
        highlightMode = HighlightMode.Standard;
    }

    private MapUnit getTurnPlayer()
    {
        return turnOrder.ToArray()[turnPlayer];
    }

    private MapCell getCurrCell()
    {
        return board.getCellAtPos((int)cursorPos.x, (int)cursorPos.y);
    }
}
