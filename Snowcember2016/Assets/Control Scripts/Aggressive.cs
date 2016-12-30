using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aggressive : ControlScript
{

    public override void AttackPattern()
    {
        if (getNearestEnemyUnit() != null)
        {
            int dist = Cell.getDist(myUnit.pos.cellData, getNearestEnemyUnit().pos.cellData);
            if (dist <= myUnit.unitScript.range)
            {
                myUnit.Attack(getNearestEnemyUnit().pos, combatInstance);
            }
            else
            {
                myUnit.canAttack = false;
                myUnit.lastAction = Mathf.NegativeInfinity;
            }
        }
        else
        {
            myUnit.canAttack = false;
            myUnit.lastAction = Mathf.NegativeInfinity;
        }
    }

    public override void MovePattern()
    {
        MapUnit nearest = getNearestEnemyUnit();
        MapCell movCell = myUnit.pos;

        if (nearest != null)
            movCell = getClosestCell(nearest.pos, myUnit.pos, myUnit.unitScript.mov);

        if (movCell != myUnit.pos)
        {
            myUnit.Move(movCell, combatInstance);
            //Debug.Log("Moving!");
        }
        else
        {
            myUnit.canMove = false;
            myUnit.lastAction = Mathf.NegativeInfinity;
            //Debug.Log("Staying in Place");
        }

    }

    public override void TurnPattern()
    {
        if (myUnit.canMove && Time.time - myUnit.lastAction > AITimer)
        {
            MovePattern();
        }
        else if (myUnit.canAttack && Time.time - myUnit.lastAction > AITimer)
        {
            AttackPattern();
        }
        else if (Time.time - myUnit.lastAction > AITimer)
        {
            if (!myUnit.canMove && !myUnit.canAttack)
            {
                myUnit.EndTurn();
            }
        }

    }
}
