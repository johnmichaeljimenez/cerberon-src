public static class Utils
{
    public static string ToFixedLength(this string str, int fixedLength)
    {
        if (str.Length > fixedLength)
            return str.Substring(0, fixedLength) + "\0";
        else
            return str.PadRight(fixedLength, ' ') + "\0";
    }
}