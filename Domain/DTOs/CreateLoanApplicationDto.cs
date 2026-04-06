namespace Domain.DTOs;

public class CreateLoanApplicationDto
{
    public Guid ApplicantId { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public decimal MonthlyIncome { get; set; }
    public int CreditScore { get; set; }
    public int EmploymentYears { get; set; }
}