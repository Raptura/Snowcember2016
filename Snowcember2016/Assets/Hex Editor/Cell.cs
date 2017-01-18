using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


[System.Serializable]
public class Cell
{
    [NonSerialized]
    public HexGrid grid;

    public enum Direction
    {
        None,
        North,
        NorthEast,
        NorthWest,
        South,
        SouthEast,
        SouthWest,
        East,
        West
    }

    /// <summary>
    /// Gets the x value of the cell
    /// This value is used for the Grid, NOT for absolute position
    /// </summary>
    /// <value>
    /// The x.
    /// </value>
    [SerializeField]
    public int x;

    /// <summary>
    /// Gets the y value of the cell
    /// This value is used for the Grid, NOT for absolute position
    /// </summary>
    /// <value>
    /// The y.
    /// </value>
    [SerializeField]
    public int y;

    /// <summary>
    /// Gets the height of the specified cell.
    /// </summary>
    /// <value>
    /// The height.
    /// </value>
    public float height
    {
        get
        {
            if (grid.hasFlatTop)
            {
                return ((float)Mathf.Sqrt(3)) / 2 * width;
            }
            else
            {
                return grid.cellSize * 2;
            }
        }
    }

    /// <summary>
    /// Gets the width of the specified cell.
    /// </summary>
    /// <value>
    /// The width.
    /// </value>
    public float width
    {
        get
        {
            if (grid.hasFlatTop)
            {
                return grid.cellSize * 2;
            }
            else
            {
                return ((float)Mathf.Sqrt(3)) / 2 * height;
            }
        }
    }

    /// <summary>
    /// Gets the horizontal distance of the specified cell.
    /// </summary>
    /// <value>
    /// The horiz dist.
    /// </value>
    public float horizDist
    {
        get
        {
            if (grid.hasFlatTop)
            {
                return width * 3 / 4;
            }
            else
            {
                return width;
            }
        }

    }

    /// <summary>
    /// Gets the vertical distance of the spefied cell.
    /// </summary>
    /// <value>
    /// The vert dist.
    /// </value>
    public float vertDist
    {
        get
        {
            if (grid.hasFlatTop)
            {
                return height;
            }
            else
            {
                return height * 3 / 4;
            }
        }
    }

    public static Cell createCell(int nx, int ny)
    {
        Cell cell = new Cell();
        cell.x = nx;
        cell.y = ny;
        return cell;
    }

    /// <summary>
    /// Gets all Cells in a specified radius.
    /// </summary>
    /// <param name="radius">The radius.</param>
    /// <returns></returns>
    public List<Cell> getAllInRadius(int radius)
    {
        List<Cell> neighbors = new List<Cell>();

        int xBound = radius + Mathf.Abs(this.x);
        int yBound = radius + Mathf.Abs(this.y);


        for (int x = -xBound; x <= xBound; x++)
        {
            for (int y = -yBound; y <= yBound; y++)
            {

                Cell currentCell = grid.getCellAtPos(x, y);
                if (currentCell != null && currentCell != this)
                {
                    if (getDist(this, currentCell) <= radius)
                    {
                        neighbors.Add(currentCell);
                    }
                }


            }
        }


        return neighbors;
    }

    /// <summary>
    /// Gets all neighbors
    /// </summary>
    /// <returns></returns>
    public List<Cell> getNeighbors()
    {
        return getAllInRadius(1);
    }

    /// <summary>
    /// Gets a neighbor cell in a specified direction
    /// </summary>
    /// <param name="dirX">The dir x.</param>
    /// <param name="dirY">The dir y.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">The directions must be within -1 and 1</exception>
    public Cell getNeighbor(int dirX, int dirY)
    {
        if (dirX < -1 || dirX > 1 || dirY < -1 || dirY > 1)
            throw new ArgumentException("The directions must be within -1 and 1");

        foreach (Cell cell in getNeighbors())
        {
            if (cell.x == this.x + dirX && cell.y == this.y + dirY)
                return cell;
        }

        return null; //There is no neighbor
    }

    /// <summary>
    /// Gets the neighbor cell in a specified direction.
    /// </summary>
    /// <param name="direction">The direction.</param>
    /// <returns></returns>
    public Cell getNeighbor(Direction direction)
    {
        switch (direction)
        {
            case Direction.North:
                if (grid.hasFlatTop)
                    return getNeighbor(0, -1);
                else
                    return null;
            //return getNeighbor(1, 1);

            case Direction.NorthEast:
                if (grid.hasFlatTop)
                    return getNeighbor(1, -1);
                else
                    return getNeighbor(1, 0);
            case Direction.NorthWest:
                if (grid.hasFlatTop)
                    return getNeighbor(-1, 0);
                else
                    return getNeighbor(0, 1);

            case Direction.South:
                if (grid.hasFlatTop)
                    return getNeighbor(0, 1);
                else
                    return null;
            //return getNeighbor(-1, -1);

            case Direction.SouthEast:
                if (grid.hasFlatTop)
                    return getNeighbor(1, 0);
                else
                    return getNeighbor(0, -1);

            case Direction.SouthWest:
                if (grid.hasFlatTop)
                    return getNeighbor(-1, 1);
                else
                    return getNeighbor(-1, 0);

            case Direction.West:
                if (grid.hasFlatTop)
                    return null;
                //return getNeighbor(-2, 1);
                else
                    return getNeighbor(-1, 1);
            case Direction.East:
                if (grid.hasFlatTop)
                    return null;
                //return getNeighbor(2, -1);
                else
                    return getNeighbor(1, -1);

            default:
                return null;

        }
    }

    /// <summary>
    /// Gets the distance between two Cells.
    /// Note: If any cell is Null, returns -1 
    /// </summary>
    /// <param name="a">Cell a.</param>
    /// <param name="b">Cell b.</param>
    /// <returns>The distance between two cells</returns>
    public static int getDist(Cell a, Cell b)
    {
        if (a == null || b == null)
            return -1;

        //return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));

        return (int)(Mathf.Abs(a.x - b.x)
        + Mathf.Abs(a.x - (a.y + a.x) - b.x + (b.y + b.x))
        + Mathf.Abs(-(a.y + a.x) + (b.y + b.x))) / 2;
    }

    /// <summary>
    /// Gets the Direction of target cell relative to this cell
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public Direction getDirection(Cell target)
    {
        if (target == null || (target.x == x && target.y == y))
            return Direction.North;

        if (grid.hasFlatTop)
        {
            //good
            if (target.x == x && target.y < y)
                return Direction.North;
            //good
            if (target.x == x && target.y > y)
                return Direction.South;
            //good
            if (target.x < x && target.y == y)
                return Direction.NorthWest;
            //good
            if (target.x > x && target.y == y)
                return Direction.SouthEast;

            if (target.x > x && target.y < y)
                return Direction.NorthEast;
            if (target.x < x && target.y > y)
                return Direction.SouthWest;
        }
        else
        {
            //good
            if (target.x > x && target.y == y)
                return Direction.NorthEast;
            //good
            if (target.x < x && target.y == y)
                return Direction.SouthWest;
            //good
            if (target.x == x && target.y >= y)
                return Direction.NorthWest;
            //good
            if (target.x == x && target.y <= y)
                return Direction.SouthEast;

            if (target.x > x && target.y < y)
                return Direction.East;
            if (target.x < x && target.y > y)
                return Direction.West;

        }

        return Direction.None;

    }
}
