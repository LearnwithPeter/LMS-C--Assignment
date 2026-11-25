using LibraryManagement.Api.Dtos;

namespace LibraryManagement.Api.Services;

public interface IMemberService
{
    Task<List<MemberDto>> GetAllAsync();
    Task<MemberDto?> GetByIdAsync(int id);
    Task<MemberDto> CreateAsync(MemberRequest request);
    Task<bool> UpdateAsync(int id, MemberRequest request);
    Task<bool> DeleteAsync(int id);
    Task<bool> EmailExistsAsync(string email, int excludeId = 0);
}