using System.ComponentModel.DataAnnotations;

namespace Domain.DTOs;

public class ChangePasswordRequestDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
}
