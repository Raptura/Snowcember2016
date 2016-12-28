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
        if (Time.time - myUnit.lastAction > AITimer)
        {
            if (myUnit.canMove == false)
            {
                myUnit.EndTurn();
            }
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
            MapCell[] path = findPath(to, from);
            return path[dist - 1];
        }
        else
        {
            return to;
        }
    }

    public MapCell[] findPath(MapCell to, MapCell from)
    {
        Queue<MapCell> frontier = new Queue<MapCell>();
        frontier.Enqueue(from);

        Dictionary<MapCell, MapCell> cameFrom = new Dictionary<MapCell, MapCell>();
        MapCell current = frontier.Peek();
        while (frontier.Count > 0)
        {
            current = frontier.Dequeue();

            if (current == to)
                break;

            Cell cell = current.cellData;
            foreach (Cell neighbors in cell.getNeighbors())
            {
                if (neighbors != null)
                {
                    MapCell m_cell = combatInstance.board.getCellAtPos(neighbors.x, neighbors.y);
                    if (!cameFrom.ContainsKey(m_cell))
                    {
                        frontier.Enqueue(m_cell);
                        cameFrom.Add(m_cell, current);
                    }
                }
            }
        }

        List<MapCell> path = new List<MapCell>();
        path.Add(current);
        while (current != from)
        {
            current = cameFrom[current];
            path.Add(current);
        }
        path.Reverse();

        return path.ToArray();
    }
}
