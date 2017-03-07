using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ControlScript : ScriptableObject
{
    /// <summary>
    /// The time it takes for an Ai script to make a move
    /// </summary>
    public const float AITimer = 1f;

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

    public List<MapUnit> getClosestEnemyQueue()
    {
        List<MapUnit> result = new List<MapUnit>();

        foreach (MapUnit unit in combatInstance.units)
        {
            if (unit != myUnit && unit.isEnemy != myUnit.isEnemy && !unit.isDead)
            {
                result.Add(unit);
            }
        }
        result.Sort((unit1, unit2) => Cell.getDist(unit1.pos.cellData, myUnit.pos.cellData).CompareTo(Cell.getDist(unit2.pos.cellData, myUnit.pos.cellData)));

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
        List<MapCell> goodCells = new List<MapCell>();
        foreach (MapCell m_cell in path)
        {
            if (Cell.getDist(m_cell.cellData, from.cellData) <= dist && combatInstance.isEmptyCell(m_cell) && m_cell != from)
            {
                goodCells.Add(m_cell);
            }
        }

        int closest = 999999; //Arbitrary large number
        MapCell chosenCell = from;
        foreach (MapCell m_cell in goodCells)
        {
            if (Cell.getDist(m_cell.cellData, to.cellData) <= closest)
            { //move to the closest cell to target you can move to
                chosenCell = m_cell;
                closest = Cell.getDist(m_cell.cellData, to.cellData);
            }
        }

        return chosenCell;
    }

    /// <summary>
    /// Finds the shortest path from one cell to another cell
    /// </summary>
    /// <param name="to"></param>
    /// <param name="from"></param>
    /// <returns></returns>
    public MapCell[] findPath(MapCell to, MapCell from)
    {
        Dictionary<MapCell, int> frontier = new Dictionary<MapCell, int>();
        frontier.Add(from, 0); //Priority Queue, Higher gets priority
        Dictionary<MapCell, MapCell> cameFrom = new Dictionary<MapCell, MapCell>();
        Dictionary<MapCell, int> costSoFar = new Dictionary<MapCell, int>();
        costSoFar.Add(from, 0);

        MapCell current = from;
        while (frontier.Count > 0)
        {
            int currCost = 99999;
            foreach (MapCell cell in frontier.Keys)
            {
                if (frontier[cell] <= currCost)
                {
                    current = cell;
                    currCost = frontier[cell];
                }
            }

            frontier.Remove(current);

            //if (current == to)
            //    break;

            foreach (Cell cell in current.cellData.getNeighbors())
            {
                if (cell != null)
                {
                    MapCell m_cell = combatInstance.board.getCellAtPos(cell.x, cell.y);

                    if (m_cell.passable)
                    {
                        int newCost;
                        if (combatInstance.isEmptyCell(m_cell))
                            newCost = costSoFar[current] + 1;
                        else
                            newCost = costSoFar[current] + 3;

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


            if (current == to)
                break;

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
