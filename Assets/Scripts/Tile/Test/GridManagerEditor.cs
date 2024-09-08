using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BuildingManager))]
public class BuildingManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기본 인스펙터를 표시
        DrawDefaultInspector();

        // BuildingManager 참조
        BuildingManager buildingManager = (BuildingManager)target;

        if (GUILayout.Button("Show Occupied Grids"))
        {
            buildingManager.DisplayOccupiedGrids();
        }
    }
}