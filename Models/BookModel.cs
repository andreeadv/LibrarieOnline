using LibrarieOnline.Models;
using System.ComponentModel.DataAnnotations;

namespace LibrarieOnline.Models
{
    public class BookModel
    {
        [Key]
        public int BookID { get; set; }
        [Required(ErrorMessage = "Titlul este obligatoriu")]
        [StringLength(100, ErrorMessage = "Titlul nu poate avea mai mult de 100 de caractere")]
        [MinLength(5, ErrorMessage = "Titlul trebuie sa aiba mai mult de 5 caractere")]
        public string? Title { get; set; }
        public string? Image { get; set; }
        [Required(ErrorMessage = "Pretul este obligatoriu")]
        public decimal Price { get; set; }
        [Required(ErrorMessage = "Descrierea este obligatorie")]
        [StringLength(1000, ErrorMessage = "Descrierea nu poate avea mai mult de 1000 de caractere")]
        [MinLength(5, ErrorMessage = "Descrierea trebuie sa aiba mai mult de 5 caractere")]
        public string? Description { get; set; }
        [Required(ErrorMessage = "Numarul de pagini este obligatoriu")]
        public int? NrPages { get; set; }
        [Required(ErrorMessage = "Data publicarii  este obligatorie")]
        public DateTime? PublishedDate { get; set; }
        [Required(ErrorMessage = "Autorul este obligatoriu")]
        [StringLength(100, ErrorMessage = "Numele autorului nu poate avea mai mult de 100 de caractere")]
        [MinLength(5, ErrorMessage = "Numele autorului trebuie sa aiba mai mult de 5 caractere")]
        public string? Author { get; set; }
        [Required(ErrorMessage = "Ratingul este obligatoriu")]
        [Range(1, 5)]
        public int AvgRating { get; set; }
        public int? CategoryID { get; set; }
        public bool Approved { get; set; }
        public virtual CategoryModel? Category { get; set; }
        public ICollection<CommentModel>? Comments { get; set; }
        public ICollection<BookCartModel>? BookCarts { get; set; }

    }
}
