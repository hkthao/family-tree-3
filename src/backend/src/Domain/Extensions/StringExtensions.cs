using System.Text.RegularExpressions;

namespace backend.Domain.Extensions;

public static class StringExtensions
{
    public static string ToKebabCase(this string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return Regex.Replace(name, "([a-z0-9]|(?=[A-Z]))([A-Z])", "$1_$2").ToLower();
    }
}
