using Domain.Models;

namespace Domain.DTOs;

public class LoanApplicationDto
{
    public Guid Id { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public decimal MonthlyIncome { get; set; }
    public int CreditScore { get; set; }
    public int EmploymentYears { get; set; }
    public LoanApprovalStatus Status { get; set; }
    public DateTime? DecisionDate { get; set; }
    public string? DecisionReason { get; set; }
    public DateTime CreatedDate { get; set; }
}