using System.ComponentModel.DataAnnotations;

namespace LibrarieOnline.Models
{
    public class CommentModel
    {
        [Key]
        public int CommentID { get; set; }
        [Required(ErrorMessage = "Continutul este obligatoriu")]
        public string? Content { get; set; }
        public DateTime Date { get; set; }
        public int Rating { get; set; }
        public int? BookID { get; set; }
        public string? UserId { get; set; }

        public virtual BookModel? Book { get; set; }
        public virtual ApplicationUser? User { get; set; }
    }
}
