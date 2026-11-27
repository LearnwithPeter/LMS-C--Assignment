using LibraryManagement.Api.Data;
using LibraryManagement.Api.Dtos;
using LibraryManagement.Api.Exceptions;
using LibraryManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.Services;

public class LoanService : ILoanService
{
    private const int MaxActiveLoans = 3;
    private const int LoanDays = 14;

    private readonly LibraryDbContext _db;
    private readonly ILogger<LoanService> _logger;

    public LoanService(LibraryDbContext db, ILogger<LoanService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<LoanDto> BorrowAsync(LoanRequest request)
    {
        var member = await _db.Members.FindAsync(request.MemberId);
        if (member is null)
            throw new NotFoundException($"Member {request.MemberId} was not found.");

        var book = await _db.Books.FindAsync(request.BookId);
        if (book is null)
            throw new NotFoundException($"Book {request.BookId} was not found.");

        if (!member.IsActive)
            throw new BusinessRuleException("Inactive members cannot borrow books.");

        var activeLoans = await _db.Loans
            .CountAsync(l => l.MemberId == member.Id && l.ReturnedOn == null);
        if (activeLoans >= MaxActiveLoans)
            throw new BusinessRuleException(
                $"A member can hold at most {MaxActiveLoans} active loans.");

        if (book.AvailableCopies <= 0)
            throw new BusinessRuleException("No copies of this book are available.");

        var borrowedOn = DateTime.UtcNow;
        var loan = new Loan
        {
            Book = book,
            Member = member,
            BorrowedOn = borrowedOn,
            DueOn = borrowedOn.AddDays(LoanDays)
        };

        book.AvailableCopies--;
        _db.Loans.Add(loan);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Loan {LoanId}: member {MemberId} borrowed book {BookId}",
            loan.Id, member.Id, book.Id);

        return ToDto(loan);
    }

    public async Task<LoanDto> ReturnAsync(int loanId)
    {
        var loan = await _db.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .FirstOrDefaultAsync(l => l.Id == loanId);
        if (loan is null)
            throw new NotFoundException($"Loan {loanId} was not found.");

        if (loan.ReturnedOn is not null)
            throw new BusinessRuleException("This loan has already been returned.");

        loan.ReturnedOn = DateTime.UtcNow;
        loan.Book.AvailableCopies++;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Loan {LoanId} returned: book {BookId}",
            loan.Id, loan.BookId);

        return ToDto(loan);
    }

    public async Task<LoanDto?> GetByIdAsync(int id)
    {
        var loan = await _db.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id);
        return loan is null ? null : ToDto(loan);
    }
  public async Task<List<LoanDto>> GetByMemberAsync(int memberId)
  {
    var memberExists = await _db.Members.AnyAsync(m => m.Id == memberId);
    if (!memberExists)
      throw new NotFoundException($"Member {memberId} was not found.");

    var loans = await _db.Loans
        .Include(l => l.Book)
        .Include(l => l.Member)
        .AsNoTracking()
        .Where(l => l.MemberId == memberId)
        .OrderByDescending(l => l.BorrowedOn)
        .ToListAsync();

    return loans.Select(ToDto).ToList();
  }


    private static LoanDto ToDto(Loan l) => new()
    {
        Id = l.Id,
        BookId = l.BookId,
        BookTitle = l.Book.Title,
        MemberId = l.MemberId,
        MemberFullName = l.Member.FullName,
        BorrowedOn = l.BorrowedOn,
        DueOn = l.DueOn,
        ReturnedOn = l.ReturnedOn
    };
}