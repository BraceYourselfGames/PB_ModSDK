using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public static class UtilityString
{
    private static string[] byteCountSuffixes = { "B", "KB", "MB", "GB", "TB" };
        
    public static string FormatByteCount (long byteCount)
    {
        int i;
        int iLimit = byteCountSuffixes.Length;
        double byteCountAsDouble = byteCount;
            
        for (i = 0; i < iLimit && byteCount >= 1024; i++, byteCount /= 1024) 
            byteCountAsDouble = byteCount / 1024.0;

        return String.Format ("{0:0.##} {1}", byteCountAsDouble, byteCountSuffixes[i]);
    }
    
    private const string fallbackNull = "null";
    private const string fallbackPresent = "present";
    
    public static void AddQuote (this StringBuilder sb, string value)
    {
        if (sb == null || string.IsNullOrEmpty (value))
            return;

        sb.Append ("\"");
        sb.Append (value);
        sb.Append ("\"");
    }
    
    public static void AddCsvCell (this StringBuilder sb, string value, bool separator = true)
    {
        if (sb == null || string.IsNullOrEmpty (value))
            return;

        sb.AddQuote (value);
        if (separator)
            sb.Append (";");
    }
    
    public static bool IsNullOrEmpty (this string source)
    {
        return source == null || source.Length == 0;
    }
    
    public static bool Contains (this string source, string toCheck, System.StringComparison comparison)
    {
        return source.IndexOf (toCheck, comparison) >= 0;
    }
    
    public static string ReplaceFirst (this string text, string search, string replace)
    {
        int pos = text.IndexOf (search);
        if (pos < 0)
        {
            return text;
        }
        return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
    }

    public static string TrimStart (this string target, string trimString)
    {
        string result = target;
        while (result.StartsWith (trimString))
        {
            result = result.Substring (trimString.Length);
        }

        return result;
    }

    public static string TrimEnd (this string target, string trimString)
    {
        string result = target;
        while (result.EndsWith (trimString))
        {
            result = result.Substring (0, result.Length - trimString.Length);
        }

        return result;
    }

    public static string ToStringFormatted (this int[] source)
    {
        StringBuilder sb = new StringBuilder ();
        sb.Append ("[");
        for (int i = 0; i < source.Length; ++i)
        {
            sb.Append (source[i]);
            if (i < source.Length - 1)
                sb.Append (", ");
        }
        sb.Append ("]");
        return sb.ToString ();
    }

    public static string ToStringFormatted (this List<int> source)
    {
        StringBuilder sb = new StringBuilder ();
        sb.Append ("[");
        for (int i = 0; i < source.Count; ++i)
        {
            sb.Append (source[i]);
            if (i < source.Count - 1)
                sb.Append (", ");
        }
        sb.Append ("]");
        return sb.ToString ();
    }

    public static string ToStringFormatted (this float[] source)
    {
        StringBuilder sb = new StringBuilder ();
        sb.Append ("[");
        for (int i = 0; i < source.Length; ++i)
        {
            sb.Append (source[i]);
            if (i < source.Length - 1)
                sb.Append (", ");
        }
        sb.Append ("]");
        return sb.ToString ();
    }

    public static string ToStringFormatted (this List<float> source)
    {
        StringBuilder sb = new StringBuilder ();
        sb.Append ("[");
        for (int i = 0; i < source.Count; ++i)
        {
            sb.Append (source[i]);
            if (i < source.Count - 1)
                sb.Append (", ");
        }
        sb.Append ("]");
        return sb.ToString ();
    }
    
    public static string TSFK<T1, T2> (this IDictionary<T1, T2> dict, bool multiline = false) => 
        ToStringFormattedKeys (dict, multiline);

    public static string ToStringFormattedKeys<T1, T2> (this IDictionary<T1, T2> dict, bool multiline = false, string multilinePrefix = null)
    {
        bool multilinePrefixUsed = multiline && !string.IsNullOrEmpty (multilinePrefix);
        
        StringBuilder sb = new StringBuilder ();
        if (!multiline)
            sb.Append ("[");
        
        if (dict == null)
            sb.Append ("null");
        else
        {
            int i = 0;
            foreach (KeyValuePair<T1, T2> kvp in dict)
            {
                if (multiline)
                {
                    if (i > 0)
                        sb.Append ("\n");
                    if (multilinePrefixUsed)
                        sb.Append (multilinePrefix);
                }
                
                sb.Append (kvp.Key.ToString ());
                
                if (!multiline && i < dict.Count - 1)
                    sb.Append (", ");
                
                ++i;
            }
            
            if (i == 0)
                sb.Append ("—");
        }
        
        if (!multiline)
            sb.Append ("]");
        return sb.ToString ();
    }
    
    public static string ToStringFormattedKeysLegacy (this IDictionary dict, bool multiline = false, string multilinePrefix = null)
    {
        bool multilinePrefixUsed = multiline && !string.IsNullOrEmpty (multilinePrefix);
        
        StringBuilder sb = new StringBuilder ();
        if (!multiline)
            sb.Append ("[");
        
        if (dict == null)
            sb.Append (fallbackNull);
        else
        {
            int i = 0;
            var keys = dict.Keys;
            foreach (var key in dict.Keys)
            {
                if (multiline)
                {
                    if (i > 0)
                        sb.Append ("\n");
                    if (multilinePrefixUsed)
                        sb.Append (multilinePrefix);
                }
                
                sb.Append (key.ToString ());
                
                if (!multiline && i < dict.Count - 1)
                    sb.Append (", ");
                
                ++i;
            }
            
            if (i == 0)
                sb.Append ("—");
        }
        
        if (!multiline)
            sb.Append ("]");
        return sb.ToString ();
    }
    
    public static string TSFV<T1, T2> (this IDictionary<T1, T2> dict, bool multiline = false) => 
        ToStringFormattedValues (dict, multiline);

    public static string ToStringFormattedValues<T1, T2> (this IDictionary<T1, T2> dict, bool multiline = false, string multilinePrefix = null)
    {
        bool multilinePrefixUsed = multiline && !string.IsNullOrEmpty (multilinePrefix);
        
        StringBuilder sb = new StringBuilder ();
        if (!multiline)
            sb.Append ("[");
        
        if (dict == null)
            sb.Append (fallbackNull);
        else
        {
            int i = 0;
            foreach (KeyValuePair<T1, T2> kvp in dict)
            {
                if (multiline)
                {
                    if (i > 0)
                        sb.Append ("\n");
                    if (multilinePrefixUsed)
                        sb.Append (multilinePrefix);
                }

                sb.Append (kvp.Value.ToString ());
                
                if (!multiline && i < dict.Count - 1)
                    sb.Append (", ");
                
                ++i;
            }
           
            if (i == 0)
                sb.Append ("—");
        }
        
        if (!multiline)
            sb.Append ("]");
        return sb.ToString ();
    }
    
    public static string TSFKVP<T1, T2> (this IDictionary<T1, T2> dict, bool multiline = false) => 
        ToStringFormattedKeyValuePairs (dict, multiline);
    
    public static string ToStringFormattedKeyValuePairs<T1, T2> (this IDictionary<T1, T2> dict, bool multiline = false, Func<T2,string> toStringOverride = null, string multilinePrefix = null)
    {
        bool multilinePrefixUsed = multiline && !string.IsNullOrEmpty (multilinePrefix);

        StringBuilder sb = new StringBuilder ();
        if (!multiline)
            sb.Append ("[");
        
        if (dict == null)
            sb.Append (fallbackNull);
        else
        {
            int i = 0;
            foreach (KeyValuePair<T1, T2> kvp in dict)
            {
                if (!multiline)
                    sb.Append ("[");
                else
                {
                    if (i > 0)
                        sb.Append ("\n");
                    if (multilinePrefixUsed)
                        sb.Append (multilinePrefix);
                }

                sb.Append (kvp.Key);
                sb.Append (": ");
                
                var entry = kvp.Value;
                var text = toStringOverride != null ? toStringOverride (entry) : entry != null ? entry.ToString () : fallbackNull;
                sb.Append (text);
                
                if (!multiline)
                    sb.Append ("]");
                
                if (!multiline && i < dict.Count - 1)
                    sb.Append (", ");
                
                ++i;
            }
           
            if (i == 0)
                sb.Append ("—");
        }
        
        if (!multiline)
            sb.Append ("]");
        return sb.ToString ();
    }
    
    public static string TSF<T> (this T[] array, bool multiline = false) => 
        ToStringFormatted (array, multiline);

    public static string ToStringFormatted<T> (this T[] array, bool multiline = false, Func<T,string> toStringOverride = null, string multilinePrefix = null)
    {
        bool multilinePrefixUsed = multiline && !string.IsNullOrEmpty (multilinePrefix);
        
        StringBuilder sb = new StringBuilder ();
        if (!multiline)
            sb.Append ("[");
        
        if (array == null)
            sb.Append (fallbackNull);
        else
        {
            for (int i = 0; i < array.Length; ++i)
            {
                if (multiline)
                {
                    if (i > 0)
                        sb.Append ("\n");
                    if (multilinePrefixUsed)
                        sb.Append (multilinePrefix);
                }

                var entry = array[i];
                var text = toStringOverride != null ? toStringOverride (entry) : entry != null ? entry.ToString () : fallbackNull;
                sb.Append (text);

                if (!multiline && i < array.Length - 1)
                    sb.Append (", ");
            }
            
            if (array.Length == 0)
                sb.Append ("—");
        }

        if (!multiline)
            sb.Append ("]");
        return sb.ToString ();
    }
    
    public static string TSF<T> (this IEnumerable<T> collection, bool multiline = false) => 
        ToStringFormatted (collection, multiline);

    public static string ToStringFormatted<T> (this IEnumerable<T> collection, bool multiline = false, Func<T,string> toStringOverride = null, bool appendBrackets = true, string multilinePrefix = null)
    {
        bool multilinePrefixUsed = multiline && !string.IsNullOrEmpty (multilinePrefix);
        
        StringBuilder sb = new StringBuilder ();
        if (!multiline && appendBrackets)
            sb.Append ("[");

        if (collection == null)
            sb.Append (fallbackNull);
        else
        {
            int i = 0;
            bool started = false;
            
            foreach (var entry in collection)
            {
                if (multiline)
                {
                    if (i > 0)   
                        sb.Append ("\n");
                    if (multilinePrefixUsed)
                        sb.Append (multilinePrefix);
                }
                
                i += 1;
                if (!multiline)
                {
                    if (started)
                        sb.Append (", ");
                    else
                        started = true;
                }
                
                var text = toStringOverride != null ? toStringOverride (entry) : entry != null ? entry.ToString () : fallbackNull;
                sb.Append (text);
            }

            if (i == 0)
                sb.Append ("—");
        }

        if (!multiline && appendBrackets)
            sb.Append ("]");
        return sb.ToString ();
    }
    
    public static string ToStringFormattedFilter (this IDictionary<string,bool> collection, bool multiline = false, bool appendBrackets = true, string multilinePrefix = null)
    {
        bool multilinePrefixUsed = multiline && !string.IsNullOrEmpty (multilinePrefix);
        
        StringBuilder sb = new StringBuilder ();
        if (!multiline && appendBrackets)
            sb.Append ("[");

        if (collection == null)
            sb.Append (fallbackNull);
        else
        {
            int i = 0;
            bool started = false;
            
            foreach (var kvp in collection)
            {
                if (multiline)
                {
                    if (i > 0)   
                        sb.Append ("\n");
                    if (multilinePrefixUsed)
                        sb.Append (multilinePrefix);
                }
                
                i += 1;
                if (!multiline)
                {
                    if (started)
                        sb.Append (", ");
                    else
                        started = true;
                }

                sb.Append (kvp.Value ? "▣ " : "☐ ");
                sb.Append (kvp.Key);
            }

            if (i == 0)
                sb.Append ("—");
        }

        if (!multiline && appendBrackets)
            sb.Append ("]");
        return sb.ToString ();
    }

    public static string ToStringNullCheck<T> (this T target) where T : class
    {
        return target == null ? fallbackNull : fallbackPresent;
    }
    
    public static string ToStringNullCheckCollection<T> (this ICollection<T> target)
    {
        return target == null ? fallbackNull : $"present ({target.Count} entries)";
    }

    public static string FirstLetterToUpperCase (this string s)
    {
        if (string.IsNullOrEmpty (s))
            return s;

        char[] a = s.ToCharArray ();
        a[0] = char.ToUpper (a[0]);
        return new string (a);
    }
    
    public static int LineCount (this string text)
    {
        int count = 0;
        if (!string.IsNullOrEmpty(text))
        {
            count = text.Length - text.Replace("\n", string.Empty).Length;

            // if the last char of the string is not a newline, make sure to count that line too
            if (text[text.Length - 1] != '\n')
            {
                ++count;
            }
        }

        return count;
    }
    
    public static string CheckAndTrimPrefixForInversion (this string s, string inversionPattern, out bool stringNotEmpty, out bool prefixWanted)
    {
        stringNotEmpty = !string.IsNullOrEmpty (s);
        prefixWanted = true;

        if (!stringNotEmpty || string.IsNullOrEmpty (inversionPattern))
            return s;

        if (!s.StartsWith (inversionPattern))
            return s;

        prefixWanted = false;
        int prefixLength = inversionPattern.Length;
        return s.Substring (prefixLength, s.Length - prefixLength);
    }

    public static bool PrefixBlocksExecution (this string s, string prefix, bool prefixWanted)
    {
        if (s == null)
            return false;

        bool startsWithPrefix = s.StartsWith (prefix);
        if (startsWithPrefix)
        {
            if (prefixWanted)
                return false;
            else
                return true;
        }
        else
        {
            if (prefixWanted)
                return true;
            else
                return false;
        }
    }
}

public static class UtilityBool
{
    public static void Invert (ref bool value)
    {
        value = !value;
    }
}