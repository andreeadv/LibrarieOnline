using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibrarieOnline.Models
{
    public class ApplicationUser : IdentityUser
    {

        public int UserID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Role { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public int Points { get; set; }

        public virtual CartModel? Cart { get; set; }
        public virtual ICollection<CommentModel>? Comments { get; set; }
        public virtual ICollection<OrderModel>? Orders { get; set; }
        public virtual ICollection<UserRewardModel>? UserRewards { get; set; }
        [NotMapped]
        public IEnumerable<SelectListItem>? AllRoles { get; set; }
        
    }
}
