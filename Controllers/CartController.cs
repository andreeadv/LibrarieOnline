//using LibrarieOnline.Data;
//using LibrarieOnline.Models;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;

//namespace LibrarieOnline.Controllers
//{
//    public class CartController : Controller
//    {
//        private readonly LibrarieOnlineContext db;
//        private readonly UserManager<ApplicationUser> _userManager;

//        public CartController(LibrarieOnlineContext context)
//        {
//            db = context;
//        }

//        public IActionResult Index()
//        {

//            var CartUser= db.Carts.Where(cp => cp.UserID == _userManager.GetUserId(User));

//            if (TempData.ContainsKey("message"))
//            {
//                ViewBag.Message = TempData["message"];
//            }
//            return View();
//        }
//    }
//}
