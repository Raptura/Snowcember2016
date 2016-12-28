using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapUnit : MonoBehaviour
{
    public string name;
    public Unit unitScript;

    public bool isDead { get { return health == 0; } }
    public int health { get; set; }
    public int startPosX, startPosY;

    public bool isPlayerControlled;

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
        transform.position = pos.transform.position;
    }

    public void EndTurn()
    {
        conductedTurn = true;
        canMove = false;
        canAttack = false;
    }

    /// <summary>
    /// Initialize The turn
    /// </summary>
    public void StartTurn()
    {
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
        }
    }

    public void Attack(MapCell target, CombatManager instance)
    {
        int dist = Cell.getDist(target.cellData, pos.cellData);

        if (canAttack && dist <= unitScript.range)
        {
            handleAttack(target, instance);
            canAttack = false;
        }
    }

    public void TurnUpdate()
    {

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

            foreach (Cell surroundingcell in target.cellData.getNeightbors())
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
