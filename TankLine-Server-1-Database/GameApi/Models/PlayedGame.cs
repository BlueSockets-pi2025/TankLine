using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameApi.Models
{
    public class PlayedGame
    {
        
        [Key]
        [Column("game_id")] 
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GameId { get; set; }

        [Column("username")] 
        [Required]
        public string Username { get; set; } = string.Empty;

        [Column("game_date")] 
        public DateTime GameDate { get; set; } =  DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

        [Column("game_won")] 
        public bool GameWon { get; set; } = false;

        [Column("tanks_destroyed")] 
        public int TanksDestroyed { get; set; } = 0;

        [Column("total_score")] 
        public int TotalScore { get; set; } = 0;

        [Column("player_rank")] 
        public int PlayerRank { get; set; } = 0;

        [Column("map_id")] 
        public string? MapId { get; set; }


        [NotMapped]
        public int TotalVictories { get; internal set; }
    }
}