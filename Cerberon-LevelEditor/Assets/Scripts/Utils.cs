using UnityEngine;

public static class Utils
{
    public static string ToFixedLength(this string str, int fixedLength)
    {
        if (str.Length >= fixedLength)
            str = str.Substring(0, fixedLength-1) + "\0";
        else
            str = (str + "\0").PadRight(fixedLength, ' ');

        return str;
    }

    public static void DrawArrow(Vector3 from, Vector3 to)
    {
        var diff = (to - from).normalized;
        var d = (from - to).normalized;
        var d1 = Quaternion.Euler(0, 0, -15) * d;
        var d2 = Quaternion.Euler(0, 0, 15) * d;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(from, to);
        Gizmos.DrawLine(to, to + d1);
        Gizmos.DrawLine(to, to + d2);
        Gizmos.color = Color.white;
    }
}