using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Api.Models;


public class Loan
{
  public int Id { get; set; }

  [Required]
  public int BookId { get; set; }

  [Required]
  public int MemberId { get; set; }

  [Required]
  public DateOnly BorrowedOn { get; set; } = new DateOnly();

  [Required]
  public DateOnly DueOn { get; set; } = new DateOnly();

  [Required]
  public DateOnly? ReturnedOn { get; set; } = new DateOnly();

  
}
