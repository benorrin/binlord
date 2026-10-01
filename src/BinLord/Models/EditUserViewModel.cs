using System.ComponentModel.DataAnnotations;

namespace BinLord.Models;

public class EditUserViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    [StringLength(200, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
    [DataType(DataType.Password)]
    [Display(Name = "New password (optional)")]
    public string? NewPassword { get; set; }
}
