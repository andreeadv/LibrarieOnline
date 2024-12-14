using LibrarieOnline.Models;
using System.ComponentModel.DataAnnotations;

namespace LibrarieOnline.Models
{
    public class CategoryModel
    {
        [Key]
        public int CategoryID { get; set; }
        [Required(ErrorMessage = "Numele categoriei este obligatoriu")]
        public string? CategoryName { get; set; }
        [Required(ErrorMessage = "Descrirea categoriei este obligatorie")]
        public string? CategoryDescription { get; set; }
        
        public virtual ICollection<BookModel>? Books { get; set; }
    }
}



