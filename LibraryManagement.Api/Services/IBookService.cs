using LibraryManagement.Api.Dtos;

namespace LibraryManagement.Api.Services;

public interface IBookService
{

    Task<PagedResult<BookDto>> GetPagedAsync(BookQuery query);
    Task<BookDto?> GetByIdAsync(int id);
    Task<BookDto> CreateAsync(BookRequest request);
    Task<bool> UpdateAsync(int id, BookRequest request);
    Task<bool> DeleteAsync(int id);
    Task<bool> IsbnExistsAsync(string isbn, int excludeId = 0);
    
}