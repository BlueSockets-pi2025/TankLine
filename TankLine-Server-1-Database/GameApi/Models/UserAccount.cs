using System.ComponentModel.DataAnnotations;

public class UserAccount
{
    [Key]
    [Required]
    [RegularExpression(@"^\S*$", ErrorMessage = "Invalid username.")]
    public required string Username { get; set; }

    [Required]
    [EmailAddress]
    [RegularExpression(@"^\S*$", ErrorMessage = "Email cannot contain white spaces.")]
    public required string Email { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 8)] // Password length 
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[#$^+=!*()@%&]).{8,}$", ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
    public required string PasswordHash { get; set; }

    [MaxLength(50)] 
    public string? FirstName { get; set; }

    [MaxLength(50)] // Limite à 50 caractères
    public string? LastName { get; set; }

    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; set; }
}
