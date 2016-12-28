using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aggressive : ControlScript
{

    public override void AttackPattern()
    {

    }

    public override void MovePattern()
    {
        MapUnit nearest = getNearestUnit();
        MapCell nearestCell = myUnit.pos;
        if (nearest != null)
            nearestCell = getClosestCell(nearest.pos, myUnit.pos, myUnit.unitScript.mov);

        myUnit.Move(nearestCell);

    }

    public override void TurnPattern()
    {
        if (Time.time - myUnit.lastAction > AITimer)
        {
            MovePattern();
        }
        if (Time.time - myUnit.lastAction > AITimer)
        {
            AttackPattern();
        }
        if (myUnit.canMove == false)
        {
            myUnit.EndTurn();
        }

    }

    public MapUnit getNearestUnit()
    {
        MapUnit result = null;
        int lowestDist = 999; //Arbitrarily Huge Number

        foreach (MapUnit unit in combatInstance.units)
        {
            if (unit != myUnit)
            {
                int dist = Cell.getDist(unit.pos.cellData, myUnit.pos.cellData);
                if (dist < lowestDist)
                {
                    lowestDist = dist;
                    result = unit;
                }
            }
        }
        return result;
    }

    public MapCell getClosestCell(MapCell to, MapCell from, int dist)
    {
        int cellDist = Cell.getDist(to.cellData, from.cellData);
        if (cellDist > dist)
        {
            MapCell[] path = findPath(to, from).ToArray();
            return path[dist];
        }
        else
        {
            return to;
        }
    }

    public List<MapCell> findPath(MapCell to, MapCell from)
    {
        List<MapCell> path = new List<MapCell>();
        path.Add(from);

        List<MapCell> traversed = new List<MapCell>();
        traversed.Add(from);

        while (traversed.Count != 0)
        {
            MapCell current = traversed.ToArray()[traversed.Count - 1];
            traversed.RemoveAt(traversed.Count - 1);

            if (current == to)
                break;

            Cell cell = from.cellData;
            foreach (Cell neighbors in cell.getNeighbors())
            {
                if (neighbors != null)
                {
                    MapCell m_cell = combatInstance.board.getCellAtPos(neighbors.x, neighbors.y);
                    if (traversed.Contains(m_cell) == false)
                    {
                        path.Add(m_cell);
                        traversed.Add(current);
                    }
                }
            }
        }

        return path;
    }
}
