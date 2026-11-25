using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Api.Dtos;

public class BookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public string ISBN { get; set; } = "";
    public int PublishedYear { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
}

public class BookRequest : IValidatableObject
{
  [Required, StringLength(200)]
  public string Title { get; set; } = "";

  [Required, StringLength(100)]
  public string Author { get; set; } = "";

  [Required, StringLength(20)]
  public string ISBN { get; set; } = "";

  [Range(1000, 2100)]
  public int PublishedYear { get; set; }

  [Range(1, int.MaxValue)]
  public int TotalCopies { get; set; }

  [Range(0, int.MaxValue)]
  public int AvailableCopies { get; set; }

  public IEnumerable<ValidationResult> Validate(ValidationContext context)
  {
    if (AvailableCopies > TotalCopies)
      yield return new ValidationResult(
          "AvailableCopies cannot be more than TotalCopies.",
          new[] { nameof(AvailableCopies) });
  }
}
