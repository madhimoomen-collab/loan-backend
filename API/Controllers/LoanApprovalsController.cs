using AutoMapper;
using Domain.Commands;
using Domain.DTOs;
using Domain.Interface;
using Domain.Models;
using Domain.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LoanApprovalsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly IGenericRepository<LoanApplication> _loanRepository;

    public LoanApprovalsController(
        IMediator mediator,
        IMapper mapper,
        IGenericRepository<LoanApplication> loanRepository)
    {
        _mediator = mediator;
        _mapper = mapper;
        _loanRepository = loanRepository;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<LoanApplicationDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = new GetListGenericQuery<LoanApplication>(
            orderBy: q => q.OrderByDescending(x => x.CreatedDate),
            page: page,
            pageSize: pageSize);

        var result = await _mediator.Send(query);
        var totalCount = await _loanRepository.CountAsync();
        return Ok(new PagedResult<LoanApplicationDto>
        {
            Items = _mapper.Map<IEnumerable<LoanApplicationDto>>(result),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
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
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var applicantId))
        {
            return Unauthorized();
        }

        var applicant = await _mediator.Send(new GetGenericQuery<User>(applicantId));
        if (applicant == null)
        {
            return BadRequest("Applicant user was not found.");
        }

        var entity = _mapper.Map<LoanApplication>(model);
        entity.ApplicantId = applicant.Id;
        entity.ApplicantName = $"{applicant.FirstName} {applicant.LastName}";
        entity.Status = LoanApprovalStatus.Pending;
        entity.DecisionDate = null;
        entity.DecisionReason = null;

        var command = new AddGenericCommand<LoanApplication>(entity);
        var result = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, _mapper.Map<LoanApplicationDto>(result));
    }

    [HttpPost("{loanId:guid}/assign/{userId:guid}")]
    [Authorize(Roles = "Admin")]
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

    [HttpGet("my-loans")]
    public async Task<ActionResult<IEnumerable<LoanApplicationDto>>> GetMyLoans()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
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

    [HttpGet("user/{userId:guid}")]
    [Authorize(Roles = "Admin")]
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

    [HttpPost("{id:guid}/under-review")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LoanApplicationDto>> MarkUnderReview(Guid id)
    {
        var loan = await _mediator.Send(new GetGenericQuery<LoanApplication>(id));
        if (loan == null)
        {
            return NotFound();
        }

        if (loan.Status != LoanApprovalStatus.Pending)
        {
            return BadRequest("Only pending loans can be marked as under review.");
        }

        loan.Status = LoanApprovalStatus.UnderReview;
        var result = await _mediator.Send(new UpdateGenericCommand<LoanApplication>(loan));
        return Ok(_mapper.Map<LoanApplicationDto>(result));
    }

    [HttpPost("{id:guid}/process")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LoanApplicationDto>> Process(Guid id)
    {
        var query = new GetGenericQuery<LoanApplication>(id);
        var loan = await _mediator.Send(query);

        if (loan == null)
        {
            return NotFound();
        }

        if (loan.Status == LoanApprovalStatus.Pending)
        {
            loan.Status = LoanApprovalStatus.UnderReview;
            loan = await _mediator.Send(new UpdateGenericCommand<LoanApplication>(loan));
        }

        if (loan.Status != LoanApprovalStatus.UnderReview)
        {
            return BadRequest("Loan application must be under review to process.");
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
