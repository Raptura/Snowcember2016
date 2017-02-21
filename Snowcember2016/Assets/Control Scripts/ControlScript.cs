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
        int startIndex = path.Length - 1;
        for (int i = 0; i < path.Length; i++)
        {
            if (Cell.getDist(path[i].cellData, from.cellData) >= dist)
            {
                startIndex = i - 1 > 0 ? i - 1 : 0;
                break;
            }
        }
        //if you cant move anywhere thats adjacent, just dont move
        for (int i = startIndex; i > 0; i--)
        {
            Cell.Direction dir = path[i].cellData.getDirection(path[startIndex - 1].cellData);
            if (dir != Cell.Direction.None)
            {
                if (combatInstance.board.grid.hasFlatTop && (dir == Cell.Direction.East || dir == Cell.Direction.West) ||
                    (!combatInstance.board.grid.hasFlatTop && (dir == Cell.Direction.North || dir == Cell.Direction.South))
                    == true)
                {
                    startIndex = (i + 1 < path.Length - 1 ? i + 1 : path.Length - 1);
                    break;
                }
            }
        }
        return path[startIndex];


    }


    /// <summary>
    /// Finds the shortest path from one cell to another cell
    /// </summary>
    /// <param name="to"></param>
    /// <param name="from"></param>
    /// <returns></returns>
    public MapCell[] findPath(MapCell to, MapCell from)
    {
        List<MapCell> frontier = new List<MapCell>();
        frontier.Add(from);
        List<int> frontier_q = new List<int>();
        frontier_q.Add(0);

        Dictionary<MapCell, MapCell> cameFrom = new Dictionary<MapCell, MapCell>();
        Dictionary<MapCell, int> costSoFar = new Dictionary<MapCell, int>();
        costSoFar.Add(from, 0);

        MapCell current = from;
        while (frontier.Count > 0)
        {
            int currCost = 0;
            int pos = 0;
            for (int i = 0; i < frontier.Count; i++)
            {
                if (frontier_q[i] >= currCost)
                {
                    current = frontier[i];
                    currCost = frontier_q[i];
                    pos = i;
                }
            }

            frontier.RemoveAt(pos);
            frontier_q.RemoveAt(pos);

            if (current == to)
                break;

            foreach (Cell cell in current.cellData.getNeighbors())
            {
                if (cell != null)
                {
                    MapCell m_cell = combatInstance.board.getCellAtPos(cell.x, cell.y);

                    if (m_cell.passable && combatInstance.isEmptyCell(m_cell))
                    {
                        int newCost = costSoFar[current] + 1;
                        if (!costSoFar.ContainsKey(m_cell) || newCost < costSoFar[m_cell])
                        {
                            if (costSoFar.ContainsKey(m_cell))
                                costSoFar.Remove(m_cell);
                            if (cameFrom.ContainsKey(m_cell))
                                cameFrom.Remove(m_cell);

                            costSoFar.Add(m_cell, newCost);
                            int priority = newCost + Cell.getDist(m_cell.cellData, to.cellData);
                            frontier.Add(m_cell);
                            frontier_q.Add(priority);

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
