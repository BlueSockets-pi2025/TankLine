using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


public class UserAccount
{
    [Key]
    [Required]
    [Column("username")]
    [RegularExpression(@"^\S*$", ErrorMessage = "Invalid username.")]
    public required string Username { get; set; }

    [Required]
    [EmailAddress]
    [Column("email")]
    [RegularExpression(@"^\S*$", ErrorMessage = "Email cannot contain white spaces.")]
    public required string Email { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 8)] // Password length 
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[#$^+=!*()@%&]).{8,}$", ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
    [Column("password_hash")]
    public required string PasswordHash { get; set; }


<<<<<<< HEAD
    [MaxLength(50)] // Limite à 50 caractères
    public string? LastName { get; set; }

    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; set; }
=======
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
>>>>>>> bd0047afc7f2b0081a7f9e5d249afec9bb8a2c59
}
