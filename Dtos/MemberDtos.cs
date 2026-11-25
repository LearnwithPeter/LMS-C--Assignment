using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Api.Dtos;

public class MemberDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime JoinedOn { get; set; }
    public bool IsActive { get; set; }
}

public class MemberRequest
{
    [Required, StringLength(100)]
    public string FullName { get; set; } = "";

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = "";

    public bool IsActive { get; set; } = true;
}