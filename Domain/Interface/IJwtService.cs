namespace Domain.Interface;

public interface IJwtService
{
    string GenerateToken(Guid userId, string email, string fullName);
}
