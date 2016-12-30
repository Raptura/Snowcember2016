using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatManager : MonoBehaviour
{
    public CameraScript cam;
    public HexGridBoard board;

    //[HideInInspector]
    public List<MapUnit> units;
    private List<MapUnit> turnOrder;
    private int turnPlayerIndex = 0;

    [HideInInspector]
    public int turn;

    private Vector2 cursorPos;
    private Cell.Direction direction;

    private float lastMoved;
    private float moveDelay = 0.25f;

    bool gameOverWin, gameOverLose;

    public bool auto = false;

    public enum HighlightMode
    {
        Standard,
        Attack,
        Movement
    }
    public HighlightMode highlightMode;

    void Start()
    {
        lastMoved = Mathf.NegativeInfinity;
        turn = 1;
        gameOverWin = gameOverLose = false;
        units = new List<MapUnit>();
        foreach (MapUnit m_unit in FindObjectsOfType<MapUnit>())
        {
            units.Add(m_unit);
        }
        turnOrder = getTurnOrder();
        cursorPos = new Vector2(0, 0);
        turnPlayerIndex = 0;

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
        manageHighlights();
    }

    void OnGUI()
    {
        if (!auto)
        {
            if (!gameOverWin && !gameOverLose)
            {
                gameplayUI();
            }
            else
            {
                gameOverUI();
            }
        }
    }

    void gameplayUI()
    {
        GUIStyle turnText = new GUIStyle(GUI.skin.box);
        turnText.fontSize = 30;
        turnText.normal.textColor = Color.black;

        GUI.Box(new Rect(Screen.width - 100, 0, 100, 50), "Turn: " + turn, turnText);

        string UIText = "";
        UIText += "Unit Name: " + getTurnPlayer().unitName + "\n";
        UIText += "Health: " + getTurnPlayer().health + "/" + getTurnPlayer().unitScript.maxHealth + "\n";
        UIText += "Attack Type: " + getTurnPlayer().unitScript.attackType.ToString() + "\n";
        UIText += "Movement: " + getTurnPlayer().unitScript.mov + "\n";
        UIText += "Range: " + getTurnPlayer().unitScript.range + "\n";
        UIText += "Damage: " + getTurnPlayer().unitScript.damage + "\n";

        GUI.Box(new Rect(Screen.width - 200, 50, 200, 100), UIText);

        if (getCursorUnit() != null && getCursorUnit() != getTurnPlayer())
        {
            MapUnit cursorUnit = getCursorUnit();
            string cursorText = "";
            cursorText += "Unit Name: " + cursorUnit.unitName + "\n";
            cursorText += "Health: " + cursorUnit.health + "/" + cursorUnit.unitScript.maxHealth + "\n";
            cursorText += "Attack Type: " + cursorUnit.unitScript.attackType.ToString() + "\n";
            cursorText += "Movement: " + cursorUnit.unitScript.mov + "\n";
            cursorText += "Range: " + cursorUnit.unitScript.range + "\n";
            cursorText += "Damage: " + cursorUnit.unitScript.damage + "\n";

            GUI.Box(new Rect(Screen.width - 200, 150, 200, 100), cursorText);

        }

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
                    getTurnPlayer().Move(getCurrCell(), this);
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
        }
    }

    void gameOverUI()
    {

        GUIStyle styling = new GUIStyle(GUI.skin.box);
        styling.fontSize = 30;
        styling.normal.textColor = Color.black;
        styling.alignment = TextAnchor.MiddleCenter;
        styling.wordWrap = true;

        int width = 300;
        int height = 300;

        Rect bounds = new Rect(new Vector2((Screen.width / 2) - (width / 2), (Screen.height / 2) - (height / 2)), new Vector2(width, height));

        string infoText = "";
        infoText += "Turn: " + turn;
        if (gameOverWin)
        {
            string congratulation = "Congratulations! You've Won! \n";
            GUI.Box(bounds, congratulation + infoText, styling);
        }
        else if (gameOverLose)
        {
            string sorry = "Sorry! Try Again? \n";
            GUI.Box(bounds, sorry + infoText, styling);
        }

        if (GUI.Button(new Rect(bounds.x, bounds.yMax, 100, 100), "Replay?"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        if (GUI.Button(new Rect(bounds.x + 100, bounds.yMax, 100, 100), "Main Menu"))
        {
            SceneManager.LoadScene((int)GameManager.SceneBuild.MainMenu);
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

    void highlightCurrentPlayerMovement()
    {
        MapUnit highlightUnit = getTurnPlayer();
        MapCell centric = board.getCellAtPos(highlightUnit.pos.cellData.x, highlightUnit.pos.cellData.y);
        List<MapCell> cellsToHighlight = board.getAllInRadius(highlightUnit.unitScript.mov, centric);

        foreach (MapCell m_cell in cellsToHighlight)
        {
            if (m_cell.passable)
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
                if (m_cell.passable)
                    m_cell.GetComponent<SpriteRenderer>().color = new Color(255f / 255f, 192f / 255f, 203f / 255f);
            }
        }

        if (getTurnPlayer().unitScript.attackType == Unit.AttackType.Line)
        {
            MapCell cell = getTurnPlayer().pos; //starting pos, at the player's pos
            MapCell cursorCell = getCurrCell();

            int dist = Cell.getDist(cell.cellData, cursorCell.cellData);
            Cell.Direction dir = cell.cellData.getDirection(cursorCell.cellData);

            if (dir != Cell.Direction.None)
            {
                for (int i = 0; i < dist; i++)
                {
                    if (cell.cellData.getNeighbor(dir) != null)
                    {
                        cell = board.getCellAtPos(cell.cellData.getNeighbor(dir).x, cell.cellData.getNeighbor(dir).y);

                        if (cell.passable)
                        {
                            if (dist - Cell.getDist(cell.cellData, cursorCell.cellData) < getTurnPlayer().unitScript.range)
                                cell.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
                            else
                                cell.gameObject.GetComponent<SpriteRenderer>().color = Color.cyan;
                        }
                    }
                    else
                        break;
                }
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
            checkGameOver();
            if (turnPlayerIndex != turnOrder.Count - 1)
                turnPlayerIndex++;
            else
            {
                turnPlayerIndex = 0;
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
    public void switchTarget(MapUnit unit)
    {
        //Change the cursor position to the current 
        cursorPos = new Vector2(unit.pos.cellData.x, unit.pos.cellData.y);
        cam.target = unit.pos.transform;
        highlightMode = HighlightMode.Standard;
    }

    private MapUnit getTurnPlayer()
    {
        return turnOrder.ToArray()[turnPlayerIndex];
    }

    private MapCell getCurrCell()
    {
        return board.getCellAtPos((int)cursorPos.x, (int)cursorPos.y);
    }

    public void setCurrCell(MapCell cell)
    {
        cursorPos.x = cell.cellData.x;
        cursorPos.y = cell.cellData.y;
        MapCell m_cell = board.getCellAtPos((int)cursorPos.x, (int)cursorPos.y);
        cam.target = m_cell.transform;
    }

    public bool isEmptyCell(MapCell cell)
    {
        foreach (MapUnit unit in units)
        {
            if (unit.pos == cell)
                return false;
        }
        return true;
    }

    private MapUnit getCursorUnit()
    {
        MapCell cell = getCurrCell();
        foreach (MapUnit unit in units)
        {
            if (unit.pos == cell)
                return unit;
        }
        return null;
    }

    private void checkGameOver()
    {
        gameOverWin = checkPlayerWin();
        gameOverLose = checkEnemyWin();

        if (auto)
        {
            if (gameOverWin || gameOverLose)
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private bool checkPlayerWin()
    {
        bool result = true;
        foreach (MapUnit unit in units)
        {
            if (!unit.isDead && unit.isEnemy == true)
            {
                result = false;
            }

        }

        return result;
    }

    bool checkEnemyWin()
    {
        bool result = true;
        foreach (MapUnit unit in units)
        {
            if (!unit.isDead && unit.isEnemy == false)
            {
                result = false;
            }

        }

        return result;
    }
}
