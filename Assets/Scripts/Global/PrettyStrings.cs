using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class PrettyStrings
{
    public static string GetPrettyDateString(string date)
    {
        if (DateTime.TryParse(date, out DateTime dt))
        {
            int day = dt.Day;
            string suffix = "th";
            if (day % 10 == 1 && day != 11) suffix = "st";
            else if (day % 10 == 2 && day != 12) suffix = "nd";
            else if (day % 10 == 3 && day != 13) suffix = "rd";
            return $"{day}{suffix} {dt:MMMM yyyy}";
        }
        return date;
    }
    public static string GetPrettyEnumString(string enumName)
    {
        return Regex.Replace(enumName, "([a-z])([A-Z])", "$1 $2");
    }
    public static string AddSpacesToEnum(string enumName)
    {
        return Regex.Replace(enumName, "([a-z])([A-Z])", "$1 $2");
    }
}
