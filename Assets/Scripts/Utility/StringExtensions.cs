using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;


public static class StringExtensions
{
    public static string Timestamped(this string str)
    {
        return System.DateTime.Now.ToString() + " : " + str;
    }

    private static Regex UpperCamelCaseRegex = new Regex(@"(?<!^)((?<!\d)\d|(?(?<=[A-Z])[A-Z](?=[a-z])|[A-Z]))", RegexOptions.None);

    public static string AsUpperCamelCaseName(this Enum e)
    {
        return UpperCamelCaseRegex.Replace(e.ToString(), " $1");

    }
}

