using System.ComponentModel.DataAnnotations;

namespace LibrarieOnline.Models
{
    public class QuizModel
    {
        [Key]
        public int QuizID { get; set; }
        public int RewardID { get; set; }
        public string? Question { get; set; }
        public int Score { get; set; }
        public string? AnswerOptions { get; set; }
        public string? CorrectAnswer { get; set; }

        public virtual RewardModel? Reward { get; set; }
        public virtual ICollection<QuestionQuizModel>? QuestionQuizzes { get; set; }
    }
}
