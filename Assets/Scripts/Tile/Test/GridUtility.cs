using UnityEngine;

public static class GridUtility
{
    // 월드 좌표를 격자에 맞추는 함수
    public static Vector3 SnapToGrid(Vector3 position, float gridSize)
    {
        float x = Mathf.Round(position.x / gridSize) * gridSize;
        float z = Mathf.Round(position.z / gridSize) * gridSize;
        return new Vector3(x, position.y, z); // y 좌표는 그대로 유지
    }
}