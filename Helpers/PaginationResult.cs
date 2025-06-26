using System;

namespace RestAPI.Helpers;

public class PaginationResult<T>
{
    public int Page { get; set; }
    public int Total { get; set; }
    public int TotalPages { get; set; }
    public List<T> Data { get; set; } = new();
    public string? Message { get; set; }
}