using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapUnit : MonoBehaviour
{
    public string unitName;
    public Unit unitScript;
    public ControlScript controlScript;

    public bool isDead { get { return health == 0; } }
    private int _health;
    public int health
    {
        get { return _health; }
        set
        {
            if (value < 0)
            {
                _health = 0;
            }
            else
                _health = value;
        }
    }
    public int startPosX, startPosY;

    public bool isPlayerControlled;
    public bool isEnemy;

    public float lastAction;

    [HideInInspector]
    public MapCell pos;


    [HideInInspector]
    public bool conductedTurn;
    [HideInInspector]
    public bool canMove, canAttack;
    private bool damageAnim;

    // Use this for initialization
    void Start()
    {
        health = unitScript.maxHealth;
        lastAction = Mathf.NegativeInfinity;
        conductedTurn = true;
        damageAnim = false;
    }

    // Update is called once per frame
    void Update()
    {
        maintainPos();
        highlightIfTurnPlayer();
        highlightIfDead();
    }

    /// <summary>
    /// Handles damage for a unit
    /// </summary>
    /// <param name="issuer">The unit issuing the damage</param>
    public void handleDamage(Unit issuer)
    {
        if (!isDead)
        {
            health -= issuer.damage;
            CombatText.createCombatText("-" + issuer.damage, Color.black, 2f, pos.transform.position, Vector2.up);
        }
    }

    public void maintainPos()
    {
        transform.position = Vector3.Lerp(transform.position, pos.transform.position, Time.deltaTime);
    }

    public void EndTurn()
    {
        conductedTurn = true;
        canMove = false;
        canAttack = false;
        lastAction = Time.time;
    }

    /// <summary>
    /// Initialize The turn
    /// </summary>
    public void StartTurn(CombatManager instance)
    {
        lastAction = Time.time;
        controlScript.init(this, instance);
        conductedTurn = false;
        canMove = true;
        canAttack = true;

    }

    public void Move(MapCell target, CombatManager instance)
    {
        int dist = Cell.getDist(target.cellData, pos.cellData);

        if ((canMove && dist <= unitScript.mov && target.passable && instance.isEmptyCell(target)
            ) || target == pos)
        {
            instance.highlightMode = CombatManager.HighlightMode.Movement;
            instance.setCurrCell(target);
            pos = target;
            canMove = false;
            lastAction = Time.time;
        }
        else {
            Debug.Log("Can not move here!");
        }
    }

    public void Attack(MapCell target, CombatManager instance)
    {
        int dist = Cell.getDist(target.cellData, pos.cellData);

        if (canAttack && dist <= unitScript.range)
        {
            instance.highlightMode = CombatManager.HighlightMode.Attack;
            instance.setCurrCell(target);

            handleAttack(target, instance);
            canAttack = false;
            lastAction = Time.time;
        }
    }

    public void TurnUpdate()
    {
        controlScript.TurnPattern();
    }

    public void handleAttack(MapCell target, CombatManager instance)
    {
        if (unitScript.attackType == Unit.AttackType.Point)
        {
            foreach (MapUnit unit in instance.units)
            {
                if (unit.pos == target)
                {
                    unit.handleDamage(this.unitScript);
                    return;
                }
            }
        }

        if (unitScript.attackType == Unit.AttackType.Line)
        {
            MapCell curr = pos;
            int dist = Cell.getDist(pos.cellData, target.cellData);
            Cell.Direction dir = pos.cellData.getDirection(target.cellData);

            for (int i = 0; i < dist; i++)
            {
                if (curr.cellData.getNeighbor(dir) != null)
                {
                    curr = instance.board.getCellAtPos(curr.cellData.getNeighbor(dir).x, curr.cellData.getNeighbor(dir).y);

                    foreach (MapUnit unit in instance.units)
                    {
                        if (unit.pos == curr)
                        {
                            unit.handleDamage(this.unitScript);
                            return;
                        }
                    }
                }
                else
                    return;
            }
        }

        if (unitScript.attackType == Unit.AttackType.Spread)
        {
            List<Cell> group = target.cellData.getNeighbors();
            group.Add(target.cellData);
            foreach (Cell surroundingcell in group)
            {
                MapCell obj = instance.board.getCellAtPos(surroundingcell.x, surroundingcell.y);

                if (obj != null)
                {
                    foreach (MapUnit unit in instance.units)
                    {
                        if (unit.pos == obj)
                        {
                            unit.handleDamage(this.unitScript);
                        }
                    }
                }
            }
        }
    }

    public void highlightIfTurnPlayer()
    {
        if (!conductedTurn)
            GetComponent<SpriteRenderer>().color = new Color(150f / 255f, 100f / 255f, 200f / 255f);
        else
            GetComponent<SpriteRenderer>().color = Color.white;
    }

    public void highlightIfDead()
    {
        if (isDead)
            GetComponent<SpriteRenderer>().color = Color.gray;
    }

}
