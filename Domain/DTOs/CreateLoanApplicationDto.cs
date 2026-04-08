namespace Domain.DTOs;

public class CreateLoanApplicationDto
{
    public decimal RequestedAmount { get; set; }
    public decimal MonthlyIncome { get; set; }
    public int CreditScore { get; set; }
    public int EmploymentYears { get; set; }
}