using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameApi.Models
{
    [Table("user_statistics")]
    public class UserStatistic
    {
        [Key]
        [Column("username")]
        public string Username { get; set; } = string.Empty;

        [Column("games_played")]
        public int GamesPlayed { get; set; } = 0;

        [Column("highest_score")]
        public int HighestScore { get; set; } = 0;

        [Column("ranking")]
        public int Ranking { get; set; } = 0;
    }
}
