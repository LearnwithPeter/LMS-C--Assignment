using LibraryManagement.Api.Data;
using LibraryManagement.Api.Dtos;
using LibraryManagement.Api.Models;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Api.Exceptions;

namespace LibraryManagement.Api.Services;

public class BookService : IBookService
{
    private readonly LibraryDbContext _db;

    public BookService(LibraryDbContext db)
    {
        _db = db;
    }

   public async Task<PagedResult<BookDto>> GetPagedAsync(BookQuery query)
{
    var books = _db.Books.AsNoTracking();

    if (!string.IsNullOrWhiteSpace(query.Author))
    {
        var author = query.Author.Trim().ToLower();
        books = books.Where(b => b.Author.ToLower().Contains(author));
    }

    if (query.Available == true)
        books = books.Where(b => b.AvailableCopies > 0);
    else if (query.Available == false)
        books = books.Where(b => b.AvailableCopies == 0);

    var totalCount = await books.CountAsync();

    var items = await books
        .OrderBy(b => b.Title)
        .ThenBy(b => b.Id)
        .Skip((query.Page - 1) * query.PageSize)
        .Take(query.PageSize)
        .ToListAsync();

    return new PagedResult<BookDto>
    {
        Items = items.Select(ToDto).ToList(),
        Page = query.Page,
        PageSize = query.PageSize,
        TotalCount = totalCount
    };
}

    public async Task<BookDto?> GetByIdAsync(int id)
    {
        var book = await _db.Books.FindAsync(id);
        return book is null ? null : ToDto(book);
    }

    public async Task<BookDto> CreateAsync(BookRequest request)
    {
        var book = new Book
        {
            Title = request.Title,
            Author = request.Author,
            ISBN = request.ISBN,
            PublishedYear = request.PublishedYear,
            TotalCopies = request.TotalCopies,
            AvailableCopies = request.AvailableCopies
        };

        _db.Books.Add(book);
        await _db.SaveChangesAsync();

        return ToDto(book);
    }

    public async Task<bool> UpdateAsync(int id, BookRequest request)
    {
        var book = await _db.Books.FindAsync(id);
        if (book is null) return false;

        book.Title = request.Title;
        book.Author = request.Author;
        book.ISBN = request.ISBN;
        book.PublishedYear = request.PublishedYear;
        book.TotalCopies = request.TotalCopies;
        book.AvailableCopies = request.AvailableCopies;

        await _db.SaveChangesAsync();
        return true;
    }

   public async Task<bool> DeleteAsync(int id)
{
    var book = await _db.Books.FindAsync(id);
    if (book is null) return false;

    if (await _db.Loans.AnyAsync(l => l.BookId == id))
        throw new BusinessRuleException("Cannot delete a book that has loan history.");

    _db.Books.Remove(book);
    await _db.SaveChangesAsync();
    return true;
}

    public async Task<bool> IsbnExistsAsync(string isbn, int excludeId = 0)
    {
        return await _db.Books.AnyAsync(b => b.ISBN == isbn && b.Id != excludeId);
    }

    private static BookDto ToDto(Book b) => new()
    {
        Id = b.Id,
        Title = b.Title,
        Author = b.Author,
        ISBN = b.ISBN,
        PublishedYear = b.PublishedYear,
        TotalCopies = b.TotalCopies,
        AvailableCopies = b.AvailableCopies
    };
}