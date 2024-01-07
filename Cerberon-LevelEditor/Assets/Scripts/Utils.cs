public static class Utils
{
    public static string ToFixedLength(this string str, int fixedLength)
    {
        fixedLength -= 1; //reserve last index to null terminator
        if (str.Length > fixedLength)
            return str.Substring(0, fixedLength) + "\0";
        else
            return str.PadRight(fixedLength, ' ') + "\0"; //TODO: Insert null terminator on last character before padding
    }
}