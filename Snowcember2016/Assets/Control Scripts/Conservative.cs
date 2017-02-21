using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Conservative : ControlScript
{
    public override void AttackPattern()
    {
        if (getNearestEnemyUnit() != null)
        {
            int dist = Cell.getDist(myUnit.pos.cellData, getNearestEnemyUnit().pos.cellData);

            if (dist <= myUnit.unitScript.range)
            {

                if (myUnit.unitScript.attackType == Unit.AttackType.Line)
                {
                    Cell.Direction dir = myUnit.pos.cellData.getDirection(getNearestEnemyUnit().pos.cellData);
                    if (dir != Cell.Direction.None)
                    {
                        if (combatInstance.board.grid.hasFlatTop && !(dir == Cell.Direction.East || dir == Cell.Direction.West) ||
                            (!combatInstance.board.grid.hasFlatTop && !(dir == Cell.Direction.North || dir == Cell.Direction.South))
                            == true)
                        {
                            myUnit.Attack(getNearestEnemyUnit().pos, combatInstance);
                        }
                    }
                    else
                    {
                        myUnit.canAttack = false;
                        myUnit.lastAction = Mathf.NegativeInfinity;
                    }
                }
                else
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
        {
            //Find the cell such that you are exactly in range, and no further
            int dist = Cell.getDist(myUnit.pos.cellData, getNearestEnemyUnit().pos.cellData);

            //If you are already in attack range
            if (dist < myUnit.unitScript.range)
            {
                //find first cell that is exactly attack range
                List<MapCell> radial = combatInstance.board.getAllInRadius(myUnit.unitScript.range, getNearestEnemyUnit().pos);

                MapCell foundCell = myUnit.pos;
                foreach (MapCell m_cell in radial)
                {
                    int rad_dist = Cell.getDist(m_cell.cellData, getNearestEnemyUnit().pos.cellData);
                    if (rad_dist == myUnit.unitScript.range)
                    {
                        Cell.Direction dir = myUnit.pos.cellData.getDirection(m_cell.cellData);
                        if (m_cell != null && myUnit.pos.cellData.getDirection(m_cell.cellData) != Cell.Direction.None
                            && m_cell.passable && combatInstance.isEmptyCell(m_cell))
                        {
                            if (myUnit.unitScript.attackType == Unit.AttackType.Line)
                            {
                                if (combatInstance.board.grid.hasFlatTop && (dir == Cell.Direction.East || dir == Cell.Direction.West) ||
                                    (!combatInstance.board.grid.hasFlatTop && (dir == Cell.Direction.North || dir == Cell.Direction.South))
                                    == false)
                                {
                                    foundCell = m_cell;
                                    break;
                                }
                            }
                            else {
                                foundCell = m_cell;
                                break;
                            }
                        }
                    }
                }
                //move to that cell
                movCell = getClosestCell(foundCell, myUnit.pos, myUnit.unitScript.mov);
            }
            //if you are not in attack range
            else if (dist > myUnit.unitScript.range)
            {
                //move to the closest cell to the target
                movCell = getClosestCell(getNearestEnemyUnit().pos, myUnit.pos, myUnit.unitScript.mov);
                //Debug.Log("Moving to Unit");

            }
            //exactly in attack range then stay in place
            else
            {
                movCell = myUnit.pos;
            }

        }

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
        if (myUnit.canAttack && Time.time - myUnit.lastAction > AITimer)
        {
            AttackPattern();
        }
        else if (myUnit.canMove && Time.time - myUnit.lastAction > AITimer)
        {
            MovePattern();
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
