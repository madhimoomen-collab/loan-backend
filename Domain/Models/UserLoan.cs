namespace Domain.Models;

public class UserLoan : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public Guid LoanApplicationId { get; set; }
    public LoanApplication? LoanApplication { get; set; }
}
