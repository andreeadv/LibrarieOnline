using System.ComponentModel.DataAnnotations;

namespace LibrarieOnline.Models
{
    public class QuestionQuizModel
    {
        [Key]
        public int QuestionID { get; set; }
        public int QuizID { get; set; }
        public string? Question { get; set; }
        public string? Answer1 { get; set; }
        public string? Answer2 { get; set; }
        public string? Answer3 { get; set; }
        public string? Answer4 { get; set; }
        public string? CorrectAnswer { get; set; }

        public virtual QuizModel? Quiz { get; set; }
    }
}
