using LibraryManagement.Api.Dtos;
using LibraryManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    public async Task<ActionResult<List<BookDto>>> GetAll()
    {
        return Ok(await _bookService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BookDto>> GetById(int id)
    {
        var book = await _bookService.GetByIdAsync(id);
        return book is null ? NotFound() : Ok(book);
    }

    [HttpPost]
    public async Task<ActionResult<BookDto>> Create(BookRequest request)
    {
        if (await _bookService.IsbnExistsAsync(request.ISBN))
            return BadRequest("A book with this ISBN already exists.");

        var created = await _bookService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, BookRequest request)
    {
        if (await _bookService.IsbnExistsAsync(request.ISBN, id))
            return BadRequest("A book with this ISBN already exists.");

        var found = await _bookService.UpdateAsync(id, request);
        return found ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var found = await _bookService.DeleteAsync(id);
        return found ? NoContent() : NotFound();
    }
}