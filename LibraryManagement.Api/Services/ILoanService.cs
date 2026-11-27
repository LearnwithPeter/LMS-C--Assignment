using LibraryManagement.Api.Dtos;

namespace LibraryManagement.Api.Services;

public interface ILoanService
{
  Task<LoanDto> BorrowAsync(LoanRequest request);
  Task<LoanDto> ReturnAsync(int loanId);
  Task<LoanDto?> GetByIdAsync(int id);
  Task<List<LoanDto>> GetByMemberAsync(int memberId);
    
}