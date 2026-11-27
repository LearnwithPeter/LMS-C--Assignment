using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Api.Dtos;

public class LoanRequest
{
    [Range(1, int.MaxValue)]
    public int BookId { get; set; }

    [Range(1, int.MaxValue)]
    public int MemberId { get; set; }
}

public class LoanDto
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = "";
    public int MemberId { get; set; }
    public string MemberFullName { get; set; } = "";
    public DateTime BorrowedOn { get; set; }
    public DateTime DueOn { get; set; }
    public DateTime? ReturnedOn { get; set; }
}