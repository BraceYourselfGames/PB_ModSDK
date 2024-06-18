using System.Runtime.CompilerServices;
using UnityEngine;
using System.Text;

public static class UtilityVector3
{
    [MethodImpl (MethodImplOptions.AggressiveInlining)]
    public static Vector3 TransformLocalToWorld (this Vector3 point, Vector3 rootPosition, Quaternion rootRotation)
    {
        return rootPosition + rootRotation * point;
    }
    
    private static string start = "(";
    private static string end = ")";
    private static string separator = ", ";
    private static StringBuilder sb = new StringBuilder ();

    public static string ToStringDetailed (this Vector3 vector, string format = "0.###")
    {
        sb.Clear ();
        sb.Append (start);
        sb.Append (vector.x.ToString (format));
        sb.Append (separator);
        sb.Append (vector.y.ToString (format));
        sb.Append (separator);
        sb.Append (vector.z.ToString (format));
        sb.Append (end);
        return sb.ToString ();
    }
    
    public static string ToStringMultilineYAML (this Vector3 vector, string offset = null, string format = "0.###")
    {
        sb.Clear ();
        bool offsetUsed = !string.IsNullOrEmpty (offset);
        
        if (offsetUsed)
            sb.Append (offset);
        sb.Append ("x: ");
        sb.Append (vector.x.ToString (format));
        
        sb.Append ("\n");
        if (offsetUsed)
            sb.Append (offset);
        sb.Append ("y: ");
        sb.Append (vector.y.ToString (format));
        
        sb.Append ("\n");
        if (offsetUsed)
            sb.Append (offset);
        sb.Append ("z: ");
        sb.Append (vector.z.ToString (format));
        
        return sb.ToString ();
    }

    public static Vector3 Flatten (this Vector3 vector)
    {
        if (vector.y == 0f)
            return vector;
        else
            return new Vector3 (vector.x, 0f, vector.z);
    }
    
    public static Vector2 Flatten2D (this Vector3 vector)
    {
        return new Vector2 (vector.x, vector.z);
    }

    public static Vector3 Flatten (this Vector3 vector, float y)
    {
        if (vector.y == y)
            return vector;
        else
            return new Vector3 (vector.x, y, vector.z);
    }
    
    public static Vector3 FlattenAndNormalize (this Vector3 vector)
    {
        if (vector.y == 0f)
            return vector.normalized;
        else
            return new Vector3 (vector.x, 0f, vector.z).normalized;
    }

    public static Vector3 GetDirection (this Vector3 from, Vector3 to)
    {
        return (to - from).normalized;
    }
    
    public static Vector3 GetDirectionFlat (this Vector3 from, Vector3 to)
    {
        var shift = (to - from);
        shift = new Vector3 (shift.x, 0f, shift.z);
        return shift.normalized;
    }
}

public static class UtilityQuaternion
{
    private static float quaternionDampingMultiplier;
    private static float quaternionDampingDtInv;
    private static Vector4 quaternionDampingVector;
    private static Quaternion quaternionDamped;

    public static Quaternion SmoothDamp (Quaternion current, Quaternion target, ref Quaternion velocity, float time, float maxSpeed, float deltaTime)
    {
        // account for double-cover
        quaternionDampingMultiplier = Quaternion.Dot (current, target) > 0f ? 1f : -1f;

        target = new Quaternion
        (
            target.x * quaternionDampingMultiplier,
            target.y * quaternionDampingMultiplier,
            target.z * quaternionDampingMultiplier,
            target.w * quaternionDampingMultiplier
        );

        // smooth damp (nlerp approx)
        quaternionDampingVector = new Vector4
        (
            Mathf.SmoothDamp (current.x, target.x, ref velocity.x, time, maxSpeed, deltaTime),
            Mathf.SmoothDamp (current.y, target.y, ref velocity.y, time, maxSpeed, deltaTime),
            Mathf.SmoothDamp (current.z, target.z, ref velocity.z, time, maxSpeed, deltaTime),
            Mathf.SmoothDamp (current.w, target.w, ref velocity.w, time, maxSpeed, deltaTime)
        ).normalized;

        // compute deriv
        quaternionDampingDtInv = 1f / deltaTime;
        velocity = new Quaternion
        (
            (quaternionDampingVector.x - current.x) * quaternionDampingDtInv,
            (quaternionDampingVector.y - current.y) * quaternionDampingDtInv,
            (quaternionDampingVector.z - current.z) * quaternionDampingDtInv,
            (quaternionDampingVector.w - current.w) * quaternionDampingDtInv
        );

        quaternionDamped = new Quaternion
        (
            quaternionDampingVector.x,
            quaternionDampingVector.y,
            quaternionDampingVector.z,
            quaternionDampingVector.w
        );
        return quaternionDamped;
    }
    
    public static Vector3 NearestPointOnLine (Vector3 lineOrigin, Vector3 lineDirection, Vector3 pointChecked)
    {
        var v = pointChecked - lineOrigin;
        var dot = Vector3.Dot(v, lineDirection);
        return lineOrigin + lineDirection * dot;
    }
        
    public static float DistanceToLine (Vector3 lineOrigin, Vector3 lineDirection, Vector3 pointChecked)
    {
        var v = pointChecked - lineOrigin;
        var dot = Vector3.Dot(v, lineDirection);
        var pointOnLine = lineOrigin + lineDirection * dot;
        var distanceToLine = Vector3.Distance (pointChecked, pointOnLine);
        return distanceToLine;
    }
}

public static class UtilityGizmo
{
    public static void DrawDisc (Vector3 position, float radius)
    {
        float theta = 0;
        float x = radius * Mathf.Cos (theta);
        float y = radius * Mathf.Sin (theta);
        Vector3 pos = position + new Vector3 (x, 0, y);
        Vector3 newPos = pos;
        Vector3 lastPos = pos;

        for (theta = 0.1f; theta < Mathf.PI * 2; theta += 0.1f)
        {
            x = radius * Mathf.Cos (theta);
            y = radius * Mathf.Sin (theta);
            newPos = position + new Vector3 (x, 0, y);
            Gizmos.DrawLine (pos, newPos);
            pos = newPos;
        }

        Gizmos.DrawLine (pos, lastPos);
    }
}