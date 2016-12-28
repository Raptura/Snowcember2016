using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(RoomGen))]
public class RoomGenEditor : Editor
{

    void OnEnable()
    {

    }

    public override void OnInspectorGUI()
    {
        RoomGen gen = (RoomGen)target;
        serializedObject.Update();

        SerializedProperty cam = serializedObject.FindProperty("cam");
        EditorGUILayout.PropertyField(cam, true);

        gen.hasFlatTop = EditorGUILayout.Toggle("Has Flat Top", gen.hasFlatTop);
        gen.cellSize = EditorGUILayout.FloatField("Cell Size", gen.cellSize);

        gen.columns = EditorGUILayout.IntField("Columns", gen.columns);
        gen.rows = EditorGUILayout.IntField("Rows", gen.columns);
        //Width Min max Slider

        gen.w_min = EditorGUILayout.IntField("Min Width", gen.w_min);
        gen.w_max = EditorGUILayout.IntField("Max Width", gen.w_max);

        float w_min = gen.w_min;
        float w_max = gen.w_max;

        EditorGUILayout.MinMaxSlider(ref w_min, ref w_max, 0, gen.columns);

        gen.w_min = Mathf.RoundToInt(w_min);
        gen.w_max = Mathf.RoundToInt(w_max);

        //Height Min Max Slider


        gen.h_min = EditorGUILayout.IntField("Min Height", gen.h_min);
        gen.h_max = EditorGUILayout.IntField("Max Height", gen.h_max);

        float h_min = gen.h_min;
        float h_max = gen.h_max;

        EditorGUILayout.MinMaxSlider(ref h_min, ref h_max, 0, gen.rows);

        gen.h_min = Mathf.RoundToInt(h_min);
        gen.h_max = Mathf.RoundToInt(h_max);

        //Enemy Count Min Max Slider

        gen.e_min = EditorGUILayout.IntField("Min Enemy Count", gen.e_min);
        gen.e_max = EditorGUILayout.IntField("Max Enemy Count", gen.e_max);

        float e_min = gen.e_min;
        float e_max = gen.e_max;

        //TODO: Make this based on something more defined (not the rows and columns, somnething associated with the room itself, maybe Min?)

        EditorGUILayout.MinMaxSlider(ref e_min, ref e_max, 0, gen.h_min * gen.w_min);

        gen.e_min = Mathf.RoundToInt(e_min);
        gen.e_max = Mathf.RoundToInt(e_max);


        SerializedProperty friendlyCount = serializedObject.FindProperty("friendlyCount");
        EditorGUILayout.PropertyField(friendlyCount, true);
        //Other Properties


        SerializedProperty floorTiles = serializedObject.FindProperty("floorTiles");
        EditorGUILayout.PropertyField(floorTiles, true);

        SerializedProperty wallTiles = serializedObject.FindProperty("wallTiles");
        EditorGUILayout.PropertyField(wallTiles, true);

        SerializedProperty enemyUnits = serializedObject.FindProperty("enemyUnits");
        EditorGUILayout.PropertyField(enemyUnits, true);

        SerializedProperty friendlyUnits = serializedObject.FindProperty("friendlyUnits");
        EditorGUILayout.PropertyField(friendlyUnits, true);

        serializedObject.ApplyModifiedProperties();
    }
}
