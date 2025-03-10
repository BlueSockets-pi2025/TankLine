using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameApi.Models
{
    [Table("verification_codes")]
    public class VerificationCode
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("email")]
        public required string Email { get; set; }


        [Column("code")]
        public required string Code { get; set; }

        [Column("expiration")]
        public DateTime Expiration { get; set; }
    }
}
