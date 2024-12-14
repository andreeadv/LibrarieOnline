using System.ComponentModel.DataAnnotations;

namespace LibrarieOnline.Models
{
    public class OrderModel
    {
        [Key]
        public int OrderID { get; set; }
        public string? UserID { get; set; }
        public int CartID { get; set; }
        public string? Status { get; set; }
        public string? PaymentType { get; set; }
        public string? Total { get; set; }

        public virtual ApplicationUser? User { get; set; }
        public virtual CartModel? Cart { get; set; }
    }
}
