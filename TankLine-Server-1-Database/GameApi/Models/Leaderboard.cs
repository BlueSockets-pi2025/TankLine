using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameApi.Models
{
    [Table("leaderboard")]
    public class Leaderboard
    {
        [Key]
        [Column("leaderboard_id")]
        public string LeaderboardId { get; set; } = string.Empty;

        [Column("username")]
        public string Username { get; set; } = string.Empty;

        [Column("score")]
        public int Score { get; set; }

        [Column("score_time")]
        public DateTime ScoreTime { get; set; }

        [ForeignKey("Username")]
        public UserAccount? User { get; set; }
    }
}
