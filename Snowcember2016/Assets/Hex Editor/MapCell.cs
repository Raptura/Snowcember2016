using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// How the cell is being represented as a MonoBehaviour
/// </summary>
public class MapCell : MonoBehaviour
{
    public Cell cellData;

    /// <summary>
    /// Creates the cell.
    /// If cell already exists at position specified, returns the existing cell
    /// </summary>
    /// <param name="grid">The grid.</param>
    /// <param name="cell">The cell being added to the object</param>
    public static MapCell createCell(HexGrid grid, Cell cell)
    {
        GameObject cellObj = new GameObject("Cell");
        MapCell newCell = cellObj.AddComponent<MapCell>();
        newCell.cellData = cell;

        newCell.cellData.grid = grid;

        cellObj.AddComponent<SpriteRenderer>();
        newCell.maintainPos();

        return newCell;
    }

    /// <summary>
    /// Maintains position for the cell
    /// </summary>
    public void maintainPos()
    {
        float posX, posY;
        if (cellData.grid.hasFlatTop)
        {
            posX = (float)cellData.x * (3f / 2f) * cellData.grid.cellSize;
            posY = (((float)cellData.x / 2f) - (cellData.y + cellData.x)) * Mathf.Sqrt(3) * cellData.grid.cellSize;
        }
        else
        {

            posX = ((cellData.x) - ((float)(cellData.x + cellData.y) / 2f)) * Mathf.Sqrt(3) * cellData.grid.cellSize;
            posY = (float)(cellData.y + cellData.x) * (3f / 2f) * cellData.grid.cellSize;
        }
        transform.localPosition = new Vector2(posX, posY);
    }

    /// <summary>
    /// Draws the Cell using Line Renderers
    /// </summary>
    public void drawCellLine()
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.SetVertexCount(7);
        lineRenderer.SetColors(Color.white, Color.white);
        lineRenderer.SetWidth(0.05f, 0.05f);

        for (int i = 0; i < 7; i++)
        {
            float angle = Mathf.Deg2Rad * (60 * i + (cellData.grid.hasFlatTop ? 0 : 30));

            float vectX = transform.position.x + cellData.grid.cellSize * Mathf.Cos(angle);
            float vectY = transform.position.y + cellData.grid.cellSize * Mathf.Sin(angle);
            lineRenderer.SetPosition(i, new Vector2(vectX, vectY));
        }
    }

    public void drawCellDebug(Color lineColor)
    {
        Vector3[] verticies = new Vector3[6];

        for (int i = 0; i < 6; i++)
        {
            float angle = Mathf.Deg2Rad * (60 * i + (cellData.grid.hasFlatTop ? 0 : 30));
            float vectX = transform.position.x + cellData.grid.cellSize * Mathf.Cos(angle);
            float vectY = transform.position.y + cellData.grid.cellSize * Mathf.Sin(angle);

            verticies[i] = new Vector2(vectX, vectY);
        }

        for (int i = 0; i < 6; i++)
        {
            int nextIndex = i + 1;
            if (nextIndex >= 6)
                nextIndex = 0;

            Debug.DrawLine(verticies[i], verticies[nextIndex], lineColor, 0); //For Internal Use only
        }
    }

    /// <summary>
    /// Draws the Cell on Screen using Sprites
    /// </summary>
    public void drawCellSprite()
    {
        GetComponent<SpriteRenderer>().color = Color.white;
        transform.localScale = new Vector2(cellData.grid.cellSize, cellData.grid.cellSize);
    }

    void Update()
    {
        CellUpdate();
    }

    /// <summary>
    /// Cell Update 
    /// </summary>
    void CellUpdate()
    {
        maintainPos();
        drawCellSprite();
        //drawCellMesh();
        //drawCellLine();
    }

}
