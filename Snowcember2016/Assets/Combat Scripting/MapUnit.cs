using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapUnit : MonoBehaviour
{
    public string unitName;
    public Unit unitScript;
    public ControlScript controlScript;

    public bool isDead { get { return health == 0; } }
    public int health { get; set; }
    public int startPosX, startPosY;

    public bool isPlayerControlled;

    public float lastAction;

    [HideInInspector]
    public MapCell pos;


    [HideInInspector]
    public bool conductedTurn = false;
    [HideInInspector]
    public bool canMove, canAttack;


    // Use this for initialization
    void Start()
    {
        health = unitScript.maxHealth;
        lastAction = Mathf.NegativeInfinity;
    }

    // Update is called once per frame
    void Update()
    {
        maintainPos();
    }

    /// <summary>
    /// Handles damage for a unit
    /// </summary>
    /// <param name="issuer">The unit issuing the damage</param>
    public void handleDamage(Unit issuer)
    {
        health -= issuer.damage;
        //Display Damage here
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

    public void Move(MapCell cell)
    {
        int dist = Cell.getDist(cell.cellData, pos.cellData);

        if (canMove && dist <= unitScript.mov)
        {
            pos = cell;
            canMove = false;
            lastAction = Time.time;
        }
    }

    public void Attack(MapCell target, CombatManager instance)
    {
        int dist = Cell.getDist(target.cellData, pos.cellData);

        if (canAttack && dist <= unitScript.range)
        {
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
                    break;
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
                            break;
                        }
                    }
                }
                else
                    break;
            }
        }

        if (unitScript.attackType == Unit.AttackType.Spread)
        {
            int dist = Cell.getDist(pos.cellData, target.cellData);

            foreach (Cell surroundingcell in target.cellData.getNeighbors())
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
}
