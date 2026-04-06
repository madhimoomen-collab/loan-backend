using System.ComponentModel.DataAnnotations;

namespace Domain.Models;

public enum LoanApprovalStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public class LoanApplication : BaseEntity
{
    [Required]
    [MaxLength(120)]
    public string ApplicantName { get; set; } = string.Empty;

    public decimal RequestedAmount { get; set; }
    public decimal MonthlyIncome { get; set; }
    public int CreditScore { get; set; }
    public int EmploymentYears { get; set; }

    public LoanApprovalStatus Status { get; set; } = LoanApprovalStatus.Pending;
    public DateTime? DecisionDate { get; set; }

    [MaxLength(500)]
    public string? DecisionReason { get; set; }

    public ICollection<UserLoan> UserLoans { get; set; } = new List<UserLoan>();
}
