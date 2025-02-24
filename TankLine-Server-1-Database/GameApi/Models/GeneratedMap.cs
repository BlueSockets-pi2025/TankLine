using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameApi.Models
{
    [Table("generated_maps")]
    public class GeneratedMap
    {
        [Key]
        [Column("map_id")]
        public string MapId { get; set; } = string.Empty;

        [Column("nb_player")]
        public int NbPlayer { get; set; }

        [Column("backgroundtype")]
        public int BackgroundType { get; set; }

        [Column("data")]
        public string Data { get; set; } = string.Empty; 
    }
}
