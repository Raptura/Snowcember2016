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
        MapCell movCell = myUnit.pos;
        int e_index = 0;
        MapUnit[] unitQueue = getClosestEnemyQueue().ToArray();

        //Find the cell such that you are exactly in range, and no further
        int dist = Cell.getDist(myUnit.pos.cellData, unitQueue[e_index].pos.cellData);

        //If you are already in attack range
        if (dist < myUnit.unitScript.range)
        {
            //find first cell that is exactly attack range
            List<MapCell> radial = combatInstance.board.getAllInRadius(myUnit.unitScript.range, unitQueue[e_index].pos);

            foreach (MapCell m_cell in radial)
            {
                int rad_dist = Cell.getDist(m_cell.cellData, unitQueue[e_index].pos.cellData);
                if (rad_dist == myUnit.unitScript.range)
                {
                    Cell.Direction dir = myUnit.pos.cellData.getDirection(m_cell.cellData);
                    if (m_cell != null /* && myUnit.pos.cellData.getDirection(m_cell.cellData) != Cell.Direction.None
                        && m_cell.passable && combatInstance.isEmptyCell(m_cell) */ )
                    {
                        if (myUnit.unitScript.attackType == Unit.AttackType.Line)
                        {
                            if (combatInstance.board.grid.hasFlatTop && (dir == Cell.Direction.East || dir == Cell.Direction.West) ||
                                (!combatInstance.board.grid.hasFlatTop && (dir == Cell.Direction.North || dir == Cell.Direction.South))
                                == false)
                            {
                                MapCell foundCell = getClosestCell(m_cell, myUnit.pos, myUnit.unitScript.mov);
                                if (foundCell != myUnit.pos)
                                {
                                    movCell = foundCell;
                                    break;
                                }
                            }
                        }
                        else {
                            MapCell foundCell = getClosestCell(m_cell, myUnit.pos, myUnit.unitScript.mov);
                            if (foundCell != myUnit.pos)
                            {
                                movCell = foundCell;
                                break;
                            }
                        }
                    }
                }
            }
            //move to that cell
            //movCell = getClosestCell(foundCell, myUnit.pos, myUnit.unitScript.mov);
        }
        //if you are not in attack range
        else if (dist > myUnit.unitScript.range)
        {
            while (e_index < unitQueue.Length && movCell == myUnit.pos)
            {
                //move to the closest cell to the target
                movCell = getClosestCell(unitQueue[e_index].pos, myUnit.pos, myUnit.unitScript.mov);
                //Debug.Log("Moving to Unit");
                e_index++;
            }
        }
        //exactly in attack range then stay in place
        else
        {
            movCell = myUnit.pos;
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
