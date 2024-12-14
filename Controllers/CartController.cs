//using LibrarieOnline.Data;
//using LibrarieOnline.Models;
//using LibrarieOnline.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Data;

//namespace LibrarieOnline.Controllers

//{
//    [Authorize]
//    public class CartsController : Controller
//    {
//        private readonly LibrarieOnlineContext db;


//        public CartsController(LibrarieOnlineContext context)
//        {
//            db = context;
//        }



//        public IActionResult Index()
//        {

//            var CartBooks = db.Carts.Include("Book").Include("User").Include("Book.Category")
//                                       .Where(cp => cp.UserId == _userManager.GetUserId(User));

//            var sum = CartBooks.Sum(x => x.Book.Price * x.Quantity);

//            if (TempData.ContainsKey("message"))
//            {
//                ViewBag.Message = TempData["message"];
//            }
//            ViewBag.CartBooks = CartBooks;
//            ViewBag.TotalPrice = sum;
//            return View();
//        }


//        [Authorize(Roles = "User,Editor,Admin")]
//        //Nu trebuie sa fac verificare pt utilizator
//        public IActionResult New(int id)
//        {

//            var book = db.Books.Find(id);
//            //verific daca exista produsul, daca da il adaug in cosul de cumparaturi
//            if (book != null)
//            {
//                var cart = db.Carts.SingleOrDefault(cart => cart.BookID == id && cart.UserId == _userManager.GetUserId(User));
//                //daca nu mai exista acest produs in cos
//                if (cart == null)
//                {
//                    Cart c = new Cart()
//                    {
//                        UserId = _userManager.GetUserId(User),
//                        BookID = id,
//                        Quantity = 1
//                    };

//                    db.Carts.Add(c);
//                    db.SaveChanges();
//                }
//                else
//                {
//                    var c = db.Carts.Find(cart.CartID, cart.BookID, cart.UserId);
//                    c.Quantity += 1;
//                    db.SaveChanges();
//                }
//                return Redirect("/Carts/Index/");
//            }
//            else
//                return Redirect("/Books/Index");


//        }


//        [Authorize(Roles = "User,Editor,Admin")]

//        public IActionResult Increase(int id, int BookID)
//        {

//            var cart = db.Carts.Find(id, BookID, _userManager.GetUserId(User));
//            if (cart != null)
//            {
//                cart.Quantity += 1;
//                db.SaveChanges();
//            }

//            return RedirectToAction("Index", "Carts");
//        }
//        [Authorize(Roles = "User,Editor,Admin")]
//        public IActionResult Decrease(int id, int BookID)
//        {

//            var cart = db.Carts.Find(id, BookID, _userManager.GetUserId(User));
//            if (cart != null)
//            {
//                if (cart.Quantity > 1)
//                    cart.Quantity -= 1;
//                else
//                    db.Carts.Remove(cart);
//                db.SaveChanges();
//            }

//            return RedirectToAction("Index", "Carts");
//        }


//        [Authorize(Roles = "User,Editor,Admin")]
//        public IActionResult Delete(int id, int BookID)
//        {

//            var cart = db.Carts.Find(id, BookID, _userManager.GetUserId(User));
//            if (cart != null)
//            {
//                db.Carts.Remove(cart);
//                db.SaveChanges();
//            }

//            return RedirectToAction("Index", "Carts");
//        }


//    }
//}
