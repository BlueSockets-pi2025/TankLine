using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameApi.Models
{
    [Table("friend_lists")]
    public class FriendList
    {
        [Column("user1")]
        public string User1 { get; set; } = string.Empty;

        [Column("user2")]
        public string User2 { get; set; } = string.Empty;

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [ForeignKey("User1")]
        public UserAccount? UserAccount1 { get; set; }

        [ForeignKey("User2")]
        public UserAccount? UserAccount2 { get; set; }
    }
}
