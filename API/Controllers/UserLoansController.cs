using Domain.Commands;
using Domain.Models;
using Domain.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserLoansController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserLoansController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<UserLoan>> Create([FromBody] UserLoan model)
    {
        var user = await _mediator.Send(new GetGenericQuery<User>(model.UserId));
        if (user == null)
        {
            return BadRequest("User not found.");
        }

        var loan = await _mediator.Send(new GetGenericQuery<LoanApplication>(model.LoanApplicationId));
        if (loan == null)
        {
            return BadRequest("Loan application not found.");
        }

        var existing = await _mediator.Send(new GetListGenericQuery<UserLoan>(
            condition: x => x.UserId == model.UserId && x.LoanApplicationId == model.LoanApplicationId));

        if (existing.Any())
        {
            return BadRequest("This user-loan link already exists.");
        }

        var result = await _mediator.Send(new AddGenericCommand<UserLoan>(model));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteGenericCommand<UserLoan>(id));
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}
