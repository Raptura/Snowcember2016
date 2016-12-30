using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ControlScript : ScriptableObject
{
    /// <summary>
    /// The time it takes for an Ai script to make a move
    /// </summary>
    public const float AITimer = 2f;

    protected MapUnit myUnit;
    protected CombatManager combatInstance;

    /// <summary>
    /// The pattern for your turn
    /// </summary>
    public abstract void TurnPattern();

    /// <summary>
    /// How you attack
    /// </summary>
    public abstract void AttackPattern();

    /// <summary>
    /// How you move
    /// </summary>
    public abstract void MovePattern();

    public void init(MapUnit myUnit, CombatManager instance)
    {
        this.myUnit = myUnit;
        this.combatInstance = instance;

    }

    public MapUnit getNearestEnemyUnit()
    {
        MapUnit result = null;
        int lowestDist = 999; //Arbitrarily Huge Number

        foreach (MapUnit unit in combatInstance.units)
        {
            if (unit != myUnit && unit.isEnemy != myUnit.isEnemy && !unit.isDead)
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

    /// <summary>
    /// Gets the closest cell to a desired position within a certain range
    /// </summary>
    /// <param name="to">The destination</param>
    /// <param name="from">The origin</param>
    /// <param name="dist">The max distance you can go</param>
    /// <returns>The closest cell to the destination, if max distance is less than the distance of the two cells, returns the destination</returns>
    public MapCell getClosestCell(MapCell to, MapCell from, int dist)
    {
        int cellDist = Cell.getDist(to.cellData, from.cellData);
        MapCell[] path = findPath(to, from);

        if (cellDist > dist)
            return path[dist];
        else
            return path[(cellDist - 1) > 0 ? cellDist - 1 : 0];

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


                    if (m_cell.passable && combatInstance.isEmptyCell(m_cell))
                    {
                        if (!cameFrom.ContainsKey(m_cell))
                        {
                            frontier.Enqueue(m_cell);
                            cameFrom.Add(m_cell, current);
                        }
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
