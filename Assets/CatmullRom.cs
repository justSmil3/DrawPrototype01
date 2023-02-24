using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CatmullRom
{
    public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t, float a = 1.0f)
    {
        t = Mathf.Clamp01(t);
        a = Mathf.Clamp01(a);

        Vector3 result = (-1.0f * a * t + 2.0f * a * t * t - a * t * t * t) * p0
                       + (1.0f + (a - 3.0f) * t * t + (2.0f - a) * t * t * t) * p1
                       + (a * t + (3.0f - 2.0f * a) * t * t + (a - 2.0f) * t * t * t) * p2
                       + (-1.0f * a * t * t + a * t * t * t) * p3;

        return result;
    }

    public static Vector3 GetPoint(Vector3 p1, Vector3 p2, Vector3 p3, float t, float a = 1.0f, bool start = true)
    {
        if (start)
        {
            return GetPoint(p1 + (p1 - p2), p1, p2, p3, t);
        }
        return GetPoint(p1, p2, p3, p3 + (p3 - p2), t);
    }

    public static Vector3 GetDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t, float a = 1.0f)
    {
        t = Mathf.Clamp01(t);
        a = Mathf.Clamp01(a);

        Vector3 result = (-1.0f * a + 4.0f * a * t - 3.0f *  a * t * t) * p0
                       + (2.0f * (a - 3.0f) * t + 3.0f * (2.0f - a) * t * t) * p1
                       + (a + 2.0f * (3.0f - 2.0f * a) * t + 3.0f * (a - 2.0f) * t * t) * p2
                       + (-2.0f * a * t + 3.0f * a * t * t) * p3;

        return result.normalized;
    }

}
