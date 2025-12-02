using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public enum ValueOperation
{
    Set,
    Offset,
    Multiply
}

public class RollingAverageFloat
{
    public int sampleCount = 10;
    
    [HideInInspector]
    public int indexLast = 0;
    
    [HideInInspector]
    private float[] values = null;

    [ShowInInspector]
    public float average => GetAverage ().Truncate (1);

    private void CheckInit ()
    {
        if (sampleCount < 2)
            sampleCount = 2;

        if (values == null || sampleCount != values.Length)
            values = new float[sampleCount];
    }

    public void AddSample (float sample)
    {
        CheckInit ();

        if (indexLast < 0 || indexLast >= sampleCount)
            indexLast = 0;

        values[indexLast] = sample;
        indexLast += 1;
    }

    public float GetAverage ()
    {
        CheckInit ();

        float sum = 0f;
        for (int i = 0; i < sampleCount; ++i)
            sum += values[i];

        sum /= sampleCount;
        return sum;
    }
}

public static class UtilityMath 
{
    public static int GetIntFromDigitViaString (int packedInteger, int digitIndex)
    {
        if (packedInteger <= 0)
            return 0;
        
        var str = packedInteger.ToString ();
        if (digitIndex >= str.Length)
            return 0;

        double charValue = Char.GetNumericValue (str[digitIndex]);
        return (int)charValue;
    }
    
    public static float WrapAngle (float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }
    
    public static float ApplyOperation (this float valueModified, ValueOperation op, float valueInput)
    {
        if (op == ValueOperation.Set)
            return valueInput;
        
        if (op == ValueOperation.Offset)
            return valueModified + valueInput;
        
        if (op == ValueOperation.Multiply)
            return valueModified * valueInput;

        return valueModified;
    }
    
    public static float GetDampedTravelDistance (float velocity, float damping, float timeLocal)
    {
        if (velocity.RoughlyEqual (0f))
            return 0f;
        
        if (damping <= 0f)
            return velocity * timeLocal;
        
        // Get forward offset based on velocity and damping. To prevent backtracking from damping, clamp time based on calculated peak time.
        var dampingScaled = (damping * velocity * 2f);
        var timeLocalStop = velocity / dampingScaled;
        var timeLocalClamped = Mathf.Min (timeLocal, timeLocalStop);
        return velocity * timeLocalClamped - 0.5f * dampingScaled * timeLocalClamped * timeLocalClamped;
    }
    
    public static float GetDampedStopTime (float velocity, float damping)
    {
        if (velocity.RoughlyEqual (0f))
            return 0f;
        
        if (damping <= 0f)
            return 1f;

        var dampingScaled = (damping * velocity * 2f);
        var timeLocalStop = velocity / dampingScaled;
        return timeLocalStop;
    }

    public static float RemapToRange (this float f, float a1, float a2, float b1, float b2)
    {
        var divisor = a2 - a1;
        if (divisor.RoughlyEqual (0f))
            return f;

		return b1 + (f - a1) * (b2 - b1) / divisor;
    }

    public static float RemapTo01 (this float f, float a1, float a2)
    {
        var divisor = a2 - a1;
        if (divisor.RoughlyEqual (0f))
            return f;
        
        return Mathf.Clamp01 ((f - a1) / divisor);
    }
    
    public static float RemapTo01 (this float f, Vector2 range)
    {
        var divisor = range.y - range.x;
        if (divisor.RoughlyEqual (0f))
            return f;
        
        f = Mathf.Clamp01 ((f - range.x) / divisor);
        return f;
    }

    public static float Truncate (this float value, int digits)
    {
        double mult = Math.Pow (10.0, digits);
        if (mult == 0)
            return value;
        
        double result = Math.Truncate (mult * value) / mult;
        return (float)result;
    }
    
    public static float PingPong (float value)
    {
        bool ascending = (int)value % 2 == 0;
        float modulus = value % 1f;
        return ascending ? modulus : 1f - modulus;
    }

    public static bool NearlyEqual (float a, float b, float epsilon)
	{
		float absA = Mathf.Abs (a);
		float absB = Mathf.Abs (b);
		float diff = Mathf.Abs (a - b);

		if (a == b)
		{
			return true;
		}
		else if (a == 0 || b == 0 || diff < float.MinValue)
		{
			// a or b is zero or both are extremely close to it
			return diff < (epsilon * float.MinValue);
		}
		else
		{ 
			// use relative error
			return diff / (absA + absB) < epsilon;
		}
	}

    public static bool RoughlyEqual (this float a, float b)
    {
        return (Mathf.Abs (a - b) <= 0.01f);
    }

    public static bool RoughlyEqual (this float a, float b, float threshold)
    {
        return (Mathf.Abs (a - b) <= threshold);
    }

    /// <summary>
    /// Minimum is 0. Max argument is inclusive, use array size - 1 as arguments to array index wrapping
    /// </summary>
    /// <param name="a"></param>
    /// <param name="forward"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static int OffsetAndWrap (this int a, bool forward, int max)
    {
        return OffsetAndWrap (a, forward, 0, max);
    }

    
    public static int OffsetAndWrap (this int a, bool forward, ICollection bounds)
    {
        return OffsetAndWrap (a, forward ? 1 : -1, 0, bounds.Count - 1);
    }

    /// <summary>
    /// Min and max arguments are inclusive, use 0 and array size - 1 as arguments to array index wrapping
    /// </summary>
    /// <param name="a"></param>
    /// <param name="forward"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static int OffsetAndWrap (this int a, bool forward, int min, int max)
    {
        return OffsetAndWrap (a, forward ? 1 : -1, min, max);
    }

    public static int OffsetAndWrap (this int a, int delta, int max)
    {
        return (OffsetAndWrap (a, delta, 0, max));
    }

    public static int OffsetAndWrap (this int a, int delta, ICollection bounds)
    {
        return OffsetAndWrap (a, delta, 0, bounds.Count - 1);
    }

    public static int OffsetAndWrap (this int a, int delta, int min, int max)
    {
        a += delta;
        if (a < min)
            a = max;
        else if (a > max)
            a = min;
        return a;
    }







    public static byte OffsetAndWrap (this byte a, bool forward, byte max)
    {
        if (forward)
        {
            if (a == max)
                a = 0;
            else
                a += 1;
        }
        else
        {
            if (a == 0)
                a = max;
            else
                a -= 1;
        }

        return a;
    }





    /// <summary>
    /// Checks if given integer is bigger or equal to "min" and lesser or equal to "max"
    /// This means that for array range checks on index vars, you should use array length - 1 as max argument
    /// Or use specialized array range checking method
    /// </summary>
    /// <param name="a"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static bool InRange (this int a, int min, int max)
    {
        return a >= min && a <= max;
    }

    public static bool IsValidIndex<T> (this int a, ICollection<T> collection)
    {
        return a >= 0 && a < collection.Count;
    }

    public static Vector3 SnapTo (this Vector3 v3, float snapAngle)
    {
        float angle = Vector3.Angle (v3, Vector3.up);
        if (angle < snapAngle / 2.0f)          // Cannot do cross product 
            return Vector3.up * v3.magnitude;  //   with angles 0 & 180
        if (angle > 180.0f - snapAngle / 2.0f)
            return Vector3.down * v3.magnitude;

        float t = Mathf.Round (angle / snapAngle);
        float deltaAngle = (t * snapAngle) - angle;

        Vector3 axis = Vector3.Cross (Vector3.up, v3);
        Quaternion q = Quaternion.AngleAxis (deltaAngle, axis);
        return q * v3;
    }

    public static float SnapTo (this float value, int step)
    {
        if (step == 0)
            return value;
        else
            return (int)Mathf.Round (value / step) * step;
    }
    
    public static float OctavePerlin (float x, float y, int octaves, float persistence, float octaveFrequencyStep)
    {
        octaves = Mathf.Max (octaves, 1);
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0; // Used for normalizing result to 0.0 - 1.0
        
        for (int i = 0; i < octaves; ++i) 
        {
            total += Mathf.PerlinNoise (x * frequency, y * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= octaveFrequencyStep;
        }

        // Avoiding bad case where max value leads to NaN output
        if (maxValue.RoughlyEqual (0f))
            return Mathf.Clamp01 (total); 
        
        return total / maxValue;
    }
    
    public static float EaseInSine (this float val, float start = 0f, float end = 1f)
    {
        end -= start;
        return -end * Mathf.Cos (val / 1 * (Mathf.PI / 2)) + end + start;
    }

    public static float EaseOutSine (this float val, float start = 0f, float end = 1f)
    {
        end -= start;
        return end * Mathf.Sin (val / 1 * (Mathf.PI / 2)) + start;
    }

    public static float EaseInOutSine (this float val, float start = 0f, float end = 1f)
    {
        end -= start;
        return -end / 2 * (Mathf.Cos (Mathf.PI * val / 1) - 1) + start;
    }
    
    public static int GreatestCommonDivisor(int a, int b)
    {
        int remainder;
        
        while (b != 0)
        {
            remainder = a % b;
            a = b;
            b = remainder;
        }
            
        return a;
    }
}
