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
    public MapCell getClosestCell(MapCell to, MapCell from, int dist, bool adjacentOnly = false)
    {
        int cellDist = Cell.getDist(to.cellData, from.cellData);
        MapCell[] path = findPath(to, from);

        if (!adjacentOnly)
        {
            for (int i = 0; i < path.Length; i++)
            {
                if (Cell.getDist(path[i].cellData, from.cellData) > dist)
                    return path[i - 1 > 0 ? i - 1 : 0];
            }
            return path[path.Length - 1];
        }
        else {
            //choose a path to work backwards from
            int startIndex = path.Length - 1;
            for (int i = 0; i < path.Length; i++)
            {
                if (Cell.getDist(path[i].cellData, from.cellData) > dist)
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
                    if (combatInstance.board.grid.hasFlatTop && !(dir == Cell.Direction.East || dir == Cell.Direction.West) ||
                        (!combatInstance.board.grid.hasFlatTop && !(dir == Cell.Direction.North || dir == Cell.Direction.South))
                        == true)
                    {
                        startIndex = i;
                        break;
                    }
                }
            }
            return path[startIndex];
        }

    }

    public MapCell[] findPath(MapCell to, MapCell from)
    {
        //Queue<MapCell> frontier = new Queue<MapCell>();
        //frontier.Enqueue(from);

        //Dictionary<MapCell, MapCell> cameFrom = new Dictionary<MapCell, MapCell>();
        //MapCell current = frontier.Peek();
        //while (frontier.Count > 0)
        //{
        //    current = frontier.Dequeue();

        //    if (current == to)
        //        break;


        //    Cell cell = current.cellData;
        //    foreach (Cell neighbors in cell.getNeighbors())
        //    {
        //        if (neighbors != null)
        //        {
        //            MapCell m_cell = combatInstance.board.getCellAtPos(neighbors.x, neighbors.y);


        //            if (m_cell.passable && combatInstance.isEmptyCell(m_cell))
        //            {
        //                if (!cameFrom.ContainsKey(m_cell))
        //                {
        //                    frontier.Enqueue(m_cell);
        //                    cameFrom.Add(m_cell, current);
        //                }
        //            }
        //        }
        //    }
        Dictionary<MapCell, int> frontier = new Dictionary<MapCell, int>();
        frontier.Add(from, 0); //Priority Queue, Higher gets priority
        Dictionary<MapCell, MapCell> cameFrom = new Dictionary<MapCell, MapCell>();
        Dictionary<MapCell, int> costSoFar = new Dictionary<MapCell, int>();
        costSoFar.Add(from, 0);

        MapCell current = from;
        while (frontier.Count > 0)
        {
            int currCost = 0;
            foreach (MapCell cell in frontier.Keys)
            {
                if (frontier[cell] > currCost)
                {
                    current = cell;
                    currCost = frontier[cell];
                }
            }

            frontier.Remove(current);

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
                            if (frontier.ContainsKey(m_cell))
                                frontier.Remove(m_cell);
                            if (cameFrom.ContainsKey(m_cell))
                                cameFrom.Remove(m_cell);

                            costSoFar.Add(m_cell, newCost);
                            int priority = newCost + Cell.getDist(m_cell.cellData, to.cellData);
                            frontier.Add(m_cell, priority);
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
