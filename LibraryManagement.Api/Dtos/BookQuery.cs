using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Api.Dtos;

public class BookQuery
{
    public string? Author { get; set; }

    public bool? Available { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}