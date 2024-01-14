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
}