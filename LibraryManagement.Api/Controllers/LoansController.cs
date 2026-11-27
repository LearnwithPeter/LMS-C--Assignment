using LibraryManagement.Api.Dtos;
using LibraryManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;

    public LoansController(ILoanService loanService)
    {
        _loanService = loanService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LoanDto>> GetById(int id)
    {
        var loan = await _loanService.GetByIdAsync(id);
        return loan is null ? NotFound() : Ok(loan);
    }

    [HttpPost]
    public async Task<ActionResult<LoanDto>> Borrow(LoanRequest request)
    {
        var loan = await _loanService.BorrowAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = loan.Id }, loan);
    }

    [HttpPost("{id}/return")]
    public async Task<ActionResult<LoanDto>> Return(int id)
    {
        var loan = await _loanService.ReturnAsync(id);
        return Ok(loan);
    }
}