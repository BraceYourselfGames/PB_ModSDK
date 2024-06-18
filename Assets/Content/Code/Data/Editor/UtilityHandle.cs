using UnityEngine;
using UnityEditor;

public static class UtilityHandle
{
    public static void DrawShape (Vector3 position, float radius, int sides, float spin, Color color)
    {
        sides = Mathf.Max (sides, 3);
        var colorOld = Handles.color;
        Handles.color = color;
        
        var step = 360f / (float)sides;
        var positionEdgeLast = (Quaternion.Euler (0f, spin, 0f) * Vector3.forward) * radius + position;
        for (int i = 0; i <= sides; ++i)
        {
            var positionEdge = (Quaternion.Euler (0f, i * step + spin, 0f) * Vector3.forward) * radius + position;
            Handles.DrawLine (positionEdgeLast, positionEdge);
            positionEdgeLast = positionEdge;
        }

        Handles.color = colorOld;
    }
}
