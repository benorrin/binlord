using System.ComponentModel.DataAnnotations;

namespace BinLord.Models;

public class CreateUserViewModel
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Viewer;
}
