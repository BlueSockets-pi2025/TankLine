using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameApi.Models
{
    [Table("achievement")]
    public class Achievement
    {
        [Key]
        [Column("username")]
        public string Username { get; set; } = string.Empty;

        [Column("unlocked")]
        public string Unlocked { get; set; } = string.Empty; 

        [ForeignKey("Username")]
        public UserAccount? User { get; set; }
    }
}
