using LibraryManagement.Api.Data;
using LibraryManagement.Api.Dtos;
using LibraryManagement.Api.Models;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Api.Exceptions;

namespace LibraryManagement.Api.Services;

public class MemberService : IMemberService
{
  private readonly LibraryDbContext _db;

  public MemberService(LibraryDbContext db)
  {
    _db = db;
  }

  public async Task<List<MemberDto>> GetAllAsync()
  {
    var members = await _db.Members.AsNoTracking().ToListAsync();
    return members.Select(ToDto).ToList();
  }

  public async Task<MemberDto?> GetByIdAsync(int id)
  {
    var member = await _db.Members.FindAsync(id);
    return member is null ? null : ToDto(member);
  }

  public async Task<MemberDto> CreateAsync(MemberRequest request)
  {
    var member = new Member
    {
      FullName = request.FullName,
      Email = request.Email,
      IsActive = request.IsActive,
      JoinedOn = DateTime.UtcNow,

    };

    _db.Members.Add(member);
    await _db.SaveChangesAsync();

    return ToDto(member);
  }
  public async Task<bool> UpdateAsync(int id, MemberRequest request)
  {
    var member = await _db.Members.FindAsync(id);
    if (member is null) return false;

    member.FullName = request.FullName;
    member.Email = request.Email;
    member.IsActive = request.IsActive;

    await _db.SaveChangesAsync();
    return true;
  }

  public async Task<bool> DeleteAsync(int id)
{
    var member = await _db.Members.FindAsync(id);
    if (member is null) return false;

    if (await _db.Loans.AnyAsync(l => l.MemberId == id))
        throw new BusinessRuleException("Cannot delete a member who has loan history.");

    _db.Members.Remove(member);
    await _db.SaveChangesAsync();
    return true;
}

  public async Task<bool> EmailExistsAsync(string email, int excludeId = 0)
  {
    return await _db.Members.AnyAsync(m => m.Email == email && m.Id != excludeId);
  }


  private static MemberDto ToDto(Member m) => new()
  {
    Id = m.Id,
    FullName = m.FullName,
    Email = m.Email,
    JoinedOn = m.JoinedOn,
    IsActive = m.IsActive
  };
}