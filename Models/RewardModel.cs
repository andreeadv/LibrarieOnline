using System.ComponentModel.DataAnnotations;

namespace LibrarieOnline.Models
{
    public class RewardModel
    {
        [Key]
        public int RewardID { get; set; }
        [Required]
        public string? RewardType { get; set; }
        public decimal Points { get; set; }
        public string? Description { get; set; }

        public virtual ICollection<UserRewardModel>? UserRewards { get; set; }
        public virtual ICollection<QuizModel>? Quizzes { get; set; }
    }
}
