using AutoMapper;
using Domain.Commands;
using Domain.DTOs;
using Domain.Models;
using Domain.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoanApprovalsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public LoanApprovalsController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LoanApplicationDto>>> GetAll()
    {
        var query = new GetListGenericQuery<LoanApplication>(
            orderBy: q => q.OrderByDescending(x => x.CreatedDate));

        var result = await _mediator.Send(query);
        return Ok(_mapper.Map<IEnumerable<LoanApplicationDto>>(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LoanApplicationDto>> GetById(Guid id)
    {
        var query = new GetGenericQuery<LoanApplication>(id);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<LoanApplicationDto>(result));
    }

    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<LoanApplicationDto>>> GetPending()
    {
        var query = new GetListGenericQuery<LoanApplication>(
            condition: x => x.Status == LoanApprovalStatus.Pending,
            orderBy: q => q.OrderBy(x => x.CreatedDate));

        var result = await _mediator.Send(query);
        return Ok(_mapper.Map<IEnumerable<LoanApplicationDto>>(result));
    }

    [HttpPost]
    public async Task<ActionResult<LoanApplicationDto>> Create([FromBody] CreateLoanApplicationDto model)
    {
        var entity = _mapper.Map<LoanApplication>(model);
        entity.Status = LoanApprovalStatus.Pending;
        entity.DecisionDate = null;
        entity.DecisionReason = null;

        var command = new AddGenericCommand<LoanApplication>(entity);
        var result = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, _mapper.Map<LoanApplicationDto>(result));
    }

    [HttpPost("{loanId:guid}/assign/{userId:guid}")]
    public async Task<ActionResult> AssignLoanToUser(Guid loanId, Guid userId)
    {
        var user = await _mediator.Send(new GetGenericQuery<User>(userId));
        if (user == null)
        {
            return NotFound("User not found.");
        }

        var loan = await _mediator.Send(new GetGenericQuery<LoanApplication>(loanId));
        if (loan == null)
        {
            return NotFound("Loan not found.");
        }

        var existingLink = await _mediator.Send(new GetListGenericQuery<UserLoan>(
            condition: x => x.UserId == userId && x.LoanApplicationId == loanId));

        if (existingLink.Any())
        {
            return BadRequest("This loan is already linked to this user.");
        }

        await _mediator.Send(new AddGenericCommand<UserLoan>(new UserLoan
        {
            UserId = userId,
            LoanApplicationId = loanId
        }));

        return NoContent();
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<IEnumerable<LoanApplicationDto>>> GetLoansByUser(Guid userId)
    {
        var user = await _mediator.Send(new GetGenericQuery<User>(userId));
        if (user == null)
        {
            return NotFound("User not found.");
        }

        var userLoans = await _mediator.Send(new GetListGenericQuery<UserLoan>(
            condition: x => x.UserId == userId,
            includes: q => q.Include(x => x.LoanApplication),
            orderBy: q => q.OrderByDescending(x => x.CreatedDate)));

        var loans = userLoans
            .Where(x => x.LoanApplication != null)
            .Select(x => x.LoanApplication!)
            .ToList();

        return Ok(_mapper.Map<IEnumerable<LoanApplicationDto>>(loans));
    }

    [HttpPost("{id:guid}/process")]
    public async Task<ActionResult<LoanApplicationDto>> Process(Guid id)
    {
        var query = new GetGenericQuery<LoanApplication>(id);
        var loan = await _mediator.Send(query);

        if (loan == null)
        {
            return NotFound();
        }

        if (loan.Status != LoanApprovalStatus.Pending)
        {
            return BadRequest("Loan application is already processed.");
        }

        var reasons = new List<string>();

        if (loan.CreditScore < 650)
        {
            reasons.Add("Credit score below 650");
        }

        if (loan.EmploymentYears < 1)
        {
            reasons.Add("Employment period below 1 year");
        }

        if (loan.MonthlyIncome <= 0)
        {
            reasons.Add("Monthly income must be greater than zero");
        }
        else if (loan.RequestedAmount > loan.MonthlyIncome * 20)
        {
            reasons.Add("Requested amount exceeds affordability threshold");
        }

        loan.DecisionDate = DateTime.Now;

        if (reasons.Count == 0)
        {
            loan.Status = LoanApprovalStatus.Approved;
            loan.DecisionReason = "Approved by generic decision rules.";
        }
        else
        {
            loan.Status = LoanApprovalStatus.Rejected;
            loan.DecisionReason = string.Join("; ", reasons);
        }

        var command = new UpdateGenericCommand<LoanApplication>(loan);
        var result = await _mediator.Send(command);

        return Ok(_mapper.Map<LoanApplicationDto>(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var command = new DeleteGenericCommand<LoanApplication>(id);
        var result = await _mediator.Send(command);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}
