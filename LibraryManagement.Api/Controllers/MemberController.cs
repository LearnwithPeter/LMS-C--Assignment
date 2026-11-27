using LibraryManagement.Api.Dtos;
using LibraryManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{

private readonly IMemberService _memberService;
private readonly ILoanService _loanService;

  public MembersController(IMemberService memberService, ILoanService loanService)
  {
    _memberService = memberService;
    _loanService = loanService;
  }


  [HttpGet]
  public async Task<ActionResult<List<MemberDto>>> GetAll()
  {
    return Ok(await _memberService.GetAllAsync());
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<MemberDto>> GetById(int id)
  {
    var member = await _memberService.GetByIdAsync(id);
    return member is null ? NotFound() : Ok(member);
  }

  [HttpPost]
  public async Task<ActionResult<MemberDto>> Create(MemberRequest request)
  {
    if (await _memberService.EmailExistsAsync(request.Email))
      return BadRequest("A member with this email already exists.");

    var created = await _memberService.CreateAsync(request);
    return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
  }

  [HttpPut("{id}")]
  public async Task<IActionResult> Update(int id, MemberRequest request)
  {
    if (await _memberService.EmailExistsAsync(request.Email, id))
      return BadRequest("A member with this email already exists.");

    var found = await _memberService.UpdateAsync(id, request);
    return found ? NoContent() : NotFound();
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(int id)
  {
    var found = await _memberService.DeleteAsync(id);
    return found ? NoContent() : NotFound();
  }

  [HttpGet("{id}/loans")]
  public async Task<ActionResult<List<LoanDto>>> GetLoans(int id)
  {
    return Ok(await _loanService.GetByMemberAsync(id));
  }

    

}