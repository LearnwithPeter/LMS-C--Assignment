namespace LibraryManagement.Api.Models;

public class Loan
{
    public int Id { get; set; }

    public int BookId { get; set; }
    public Book Book { get; set; } = null!;

    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;

    public DateTime BorrowedOn { get; set; }
    public DateTime DueOn { get; set; }
    public DateTime? ReturnedOn { get; set; }
}