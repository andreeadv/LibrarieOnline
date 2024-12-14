using System.ComponentModel.DataAnnotations;

namespace LibrarieOnline.Models
{
    public class CartModel
    {
        [Key]
        public int CartID { get; set; }
        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<BookModel>? Book { get; set; }
        public virtual ICollection<BookCartModel>? BookCarts { get; set; }
        public virtual OrderModel? Order { get; set; }

    }
}
