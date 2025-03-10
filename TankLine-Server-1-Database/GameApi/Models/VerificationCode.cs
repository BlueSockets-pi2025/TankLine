public class VerificationCode
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string Code { get; set; }
    public DateTime Expiration { get; set; }
}
