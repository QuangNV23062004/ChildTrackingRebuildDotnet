using System;

namespace RestAPI.Helpers;

public class QueryParams
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string? Search { get; set; }

    // Enum version would be better, but you can use string for flexibility
    public string? Order { get; set; } = "ascending"; // or "descending"
    public string? SortBy { get; set; } = "date";

    // Optional: store all extra dynamic keys if needed
    public Dictionary<string, object>? Extra { get; set; }
}
