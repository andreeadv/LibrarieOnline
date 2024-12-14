using System.ComponentModel.DataAnnotations;

namespace LibrarieOnline.Models
{
    public class BookCartModel
    {
        public int? BookID { get; set; }
        [Key]
        public int? CartID { get; set; }
        public int Quantity { get; set; }

        public virtual BookModel? Book { get; set; }
        public virtual CartModel? Cart { get; set; }
    }
}
