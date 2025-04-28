using System;
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
    [StringLength(100, MinimumLength = 8)]
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[#$^+=!*()@%&]).{8,}$", ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
    [Column("password_hash")]
    public required string PasswordHash { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

    [Required]
    [Column("is_verified")]
    public bool IsVerified { get; set; } = false;

    [Required]
    [Column("first_name")]
    [StringLength(50)]
    public required string FirstName { get; set; }

    [Required]
    [Column("last_name")]
    [StringLength(50)]
    public required string LastName { get; set; }

    [Required]
    [Column("birth_date")]
    public DateTime BirthDate { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

    // New fields for verification code
    [Column("verification_code")]
    public string? VerificationCode { get; set; }

    [Column("verification_expiration")]
    public DateTime? VerificationExpiration { get; set; }

    // New fields for password reset
    [Column("password_reset_token")]
    public string? PasswordResetToken { get; set; }

    [Column("password_reset_expiration")]
    public DateTime? PasswordResetExpiration { get; set; }

    [Column("refresh_token_hash")]
    public string? RefreshTokenHash { get; set; }

    [Column("refresh_token_expiration")]
    public DateTime? RefreshTokenExpiration { get; set; }


    [Required]
    [Column("is_logged_in")]
    public bool IsLoggedIn { get; set; } = false;


    [Column("last_activity")]
    public DateTime? LastActivity { get; set; }


}