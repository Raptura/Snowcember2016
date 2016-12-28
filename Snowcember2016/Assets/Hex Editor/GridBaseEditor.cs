using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;


public class GridBaseEditor : EditorWindow
{
    //Grid Object Information
    public HexGridBoard board;
    public HexGrid grid;

    Sprite cellSprite;
    int groupSize = 5; //default value of 5
    float cellSize = 1.0f; //default value of 1
    int x = 0, y = 0; //default 0, 0 values;


    Color lineColor = Color.white;

    bool gridOpen, cellOpen;


    [MenuItem("Window/GridBase/HexGrid")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(GridBaseEditor));
    }

    void OnGUI()
    {
        ManagementGUI();
        EditorGUILayout.Separator();

        if (board != null && grid != null)
        {
            GridGUI();
            EditorGUILayout.Separator();
            CellGUI();
        }
        else
        {
            //do nothing
        }
    }

    /// <summary>
    /// The UI Always Present
    /// Holds Version Info
    /// Edits Board and Grid Info
    /// </summary>
    void ManagementGUI()
    {
        EditorGUILayout.LabelField("Grid Base Version: 1.00");
        board = (HexGridBoard)EditorGUILayout.ObjectField("Board", board, typeof(HexGridBoard), allowSceneObjects: true);
        if (board != null)
        {
            grid = (HexGrid)EditorGUILayout.ObjectField("Grid", board.grid, typeof(HexGrid), allowSceneObjects: false);
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Load Hex Grid"))
        {
            if (board == null)
            {
                GameObject obj = new GameObject("Hex Grid Board");
                board = obj.AddComponent<HexGridBoard>();
            }

            string pathToHex = EditorUtility.OpenFilePanel("Hex Grid", "", "json");

            if (File.Exists(pathToHex))
            {
                string hexDataAsJson = File.ReadAllText(pathToHex);
                grid = CreateInstance<HexGrid>();
                JsonUtility.FromJsonOverwrite(hexDataAsJson, grid);
                grid.linkCells();
                board.GenerateMap(grid);

                EditorUtility.SetDirty(grid);
            }
            else
                grid = null;

            EditorUtility.SetDirty(board);
        }

        if (GUILayout.Button("Create New Grid"))
        {
            //Clear the Information stored in the editor
            board = null;
            grid = null;

            GameObject obj = new GameObject("Hex Grid Board");
            board = obj.AddComponent<HexGridBoard>();

            grid = CreateInstance<HexGrid>();
            grid.cells = new List<Cell>();
            board.grid = grid;
        }
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// UI Responsible for managing the HexGrid object
    /// </summary>
    void GridGUI()
    {
        gridOpen = EditorGUILayout.Foldout(gridOpen, "Hex Grid Information");
        if (gridOpen)
        {
            if (grid != null)
            {
                if (GUILayout.Button("Export Grid"))
                {
                    string save = JsonUtility.ToJson(grid, true);

                    //TODO: Make User set the path up themselves
                    using (FileStream fs = new FileStream("Assets/Resources/HexGrids/NewHexGrid.json", FileMode.Create))
                    {
                        using (StreamWriter writer = new StreamWriter(fs))
                        {
                            writer.Write(save);
                            writer.Close();
                            writer.Dispose();
                        }
                        fs.Close();
                        fs.Dispose();
                    }
                }

                EditorGUI.BeginChangeCheck();
                grid.hasFlatTop = EditorGUILayout.Toggle("Has Flat Top", grid.hasFlatTop);
                grid.cellSize = EditorGUILayout.FloatField("Cell Size", grid.cellSize);

                if (GUI.changed)
                {
                    EditorUtility.SetDirty(grid);
                    redrawAll();
                }
                EditorGUI.EndChangeCheck();
                lineColor = EditorGUILayout.ColorField("(Debug) Line Color", lineColor);

                foreach (MapCell mapCell in board.GetComponentsInChildren<MapCell>())
                {
                    mapCell.drawCellDebug(lineColor);
                }
            }
        }
    }

    void CellGUI()
    {
        cellOpen = EditorGUILayout.Foldout(cellOpen, "Cell Information");
        if (cellOpen)
        {
            if (grid.cells.Count == 0)
            {
                EditorGUILayout.LabelField("There are no cells associated with this Hex Grid");
            }
            else
            {

            }

            if (grid != null)
            {
                //Cell Type
                cellSprite = (Sprite)EditorGUILayout.ObjectField("Cell Sprite", cellSprite, typeof(Sprite), false);

                //Group Size
                EditorGUILayout.PrefixLabel("Group Size: " + groupSize);
                groupSize = EditorGUILayout.IntField(groupSize);

                //Positioning
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("X: ");
                x = EditorGUILayout.IntField(x);
                EditorGUILayout.PrefixLabel("Y: ");
                y = EditorGUILayout.IntField(y);
                EditorGUILayout.EndHorizontal();


                if (GUILayout.Button("Create Cell"))
                {
                    Cell newCell = Cell.createCell(x, y);
                    grid.cells.Add(newCell);
                    newCell.grid = grid;

                    MapCell mapCell = MapCell.createCell(grid, newCell);
                    mapCell.transform.SetParent(board.transform);
                    mapCell.GetComponent<SpriteRenderer>().sprite = cellSprite;
                }

                if (GUILayout.Button("Create Cell Group"))
                {
                    List<Cell> created = grid.createCellGroup(x, y, groupSize);

                    foreach (Cell cell in created)
                    {
                        grid.cells.Add(cell);
                        cell.grid = grid;

                        MapCell mapCell = MapCell.createCell(grid, cell);
                        mapCell.transform.SetParent(board.transform);
                        mapCell.GetComponent<SpriteRenderer>().sprite = cellSprite;

                        mapCell.transform.SetParent(board.transform);
                        mapCell.GetComponent<SpriteRenderer>().sprite = cellSprite;
                    }
                }

                if (GUILayout.Button("Re - Generate Map"))
                {
                    foreach (Transform child in board.transform)
                    {
                        DestroyImmediate(child.gameObject);
                    }
                    board.GenerateMap(grid);

                }
            }
            else
            {

                //Cell Size
                EditorGUILayout.PrefixLabel("Cell Size");
                cellSize = EditorGUILayout.FloatField(cellSize);
            }
        }
    }

    public void redrawAll()
    {
        foreach (MapCell m_cell in board.transform.GetComponentsInChildren<MapCell>())
        {
            m_cell.maintainPos();
        }
    }
}
