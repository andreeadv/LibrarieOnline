using System.ComponentModel.DataAnnotations;

namespace LibrarieOnline.Models
{
    public class UserRewardModel
    {
        [Key]
        public int UserRewardID { get; set; }
        public string? UserId { get; set; }
        public int RewardID { get; set; }
        public DateTime ClaimedDate { get; set; }

        public virtual ApplicationUser? User { get; set; }
        public virtual RewardModel? Reward { get; set; }
    }
}
