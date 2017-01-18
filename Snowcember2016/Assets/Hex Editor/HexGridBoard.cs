using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGridBoard : MonoBehaviour
{
    public HexGrid grid;
    public List<MapCell> cells;

    /// <summary>
    /// Generates the Map using the Map Cell class
    /// </summary>
    /// <param name="grid">The hex grid that is being generated</param>
    /// <returns>The list of newly created Map Cells</returns>
    public List<MapCell> GenerateMap(HexGrid grid)
    {
        this.grid = grid;
        List<MapCell> cellList = new List<MapCell>();
        foreach (Cell cell in grid.cells)
        {
            MapCell newCell = MapCell.createCell(grid, cell);
            newCell.transform.SetParent(this.transform);
            cellList.Add(newCell);
            cells.Add(newCell);
        }

        return cellList;
    }

    public MapCell getCellAtPos(int x, int y)
    {
        Cell cell = grid.getCellAtPos(x, y);
        if (cell != null)
        {
            foreach (MapCell m_cell in cells)
            {
                if (m_cell.cellData.x == cell.x && m_cell.cellData.y == cell.y)
                    return m_cell;
            }
        }
        return null;
    }

    void Start()
    {
        cells = new List<MapCell>();
        foreach (MapCell m_cell in GetComponentsInChildren<MapCell>())
        {
            cells.Add(m_cell);
        }
        grid.linkCells();
        foreach (MapCell m_cell in cells)
        {
            m_cell.cellData.grid = grid;
        }
    }

    /// <summary>
    /// Gets all Cells in a specified radius.
    /// </summary>
    /// <param name="radius">The radius.</param>
    /// <returns></returns>
    public List<MapCell> getAllInRadius(int radius, MapCell cell)
    {
        List<MapCell> neighbors = new List<MapCell>();

        int xBound = radius + Mathf.Abs(cell.cellData.x);
        int yBound = radius + Mathf.Abs(cell.cellData.y);


        for (int x = -xBound; x <= xBound; x++)
        {
            for (int y = -yBound; y <= yBound; y++)
            {

                MapCell currentCell = getCellAtPos(x, y);
                if (currentCell != null && currentCell != cell)
                {
                    if (Cell.getDist(cell.cellData, currentCell.cellData) <= radius)
                    {
                        neighbors.Add(currentCell);
                    }
                }
            }
        }


        return neighbors;
    }

}
