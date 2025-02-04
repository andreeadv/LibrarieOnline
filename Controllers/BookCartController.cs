//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using LibrarieOnline.Data;
//using LibrarieOnline.Models;
//using System.Linq;
//using System.Threading.Tasks;

//namespace LibrarieOnline.Controllers
//{
//    public class BookCartController : Controller
//    {
//        private readonly LibrarieOnlineContext _context;
//        private readonly UserManager<ApplicationUser> _userManager;

//        public BookCartController(LibrarieOnlineContext context, UserManager<ApplicationUser> userManager)
//        {
//            _context = context;
//            _userManager = userManager;
//        }

//        // Adăugarea unei cărți în cos
//        [HttpPost]
//        public async Task<IActionResult> AddToCart(int bookId, int quantity)
//        {
//            var user = await _userManager.GetUserAsync(User);  // Obține utilizatorul curent

//            if (user == null)
//            {
//                return RedirectToAction("Login", "Account");  // Dacă nu există utilizator, redirecționează la Login
//            }

//            // Căutăm cosul utilizatorului
//            var cart = await _context.Carts
//                                     .FirstOrDefaultAsync(c => c.UserID == user.UserID);

//            // Dacă nu există cos pentru utilizator, creăm unul nou
//            if (cart == null)
//            {
//                cart = new CartModel { UserID = user.UserID };
//                _context.Carts.Add(cart);
//                await _context.SaveChangesAsync();
//            }

//            // Căutăm dacă produsul este deja în cos
//            var existingBookInCart = await _context.BookCarts
//                                                    .FirstOrDefaultAsync(bc => bc.BookID == bookId && bc.CartID == cart.CartID);

//            if (existingBookInCart != null)
//            {
//                // Dacă produsul există deja în cos, actualizăm cantitatea
//                existingBookInCart.Quantity += quantity;
//                _context.BookCarts.Update(existingBookInCart);
//            }
//            else
//            {
//                // Dacă produsul nu există în cos, adăugăm o nouă intrare în BookCart
//                var bookCartItem = new BookCartModel
//                {
//                    BookID = bookId,
//                    CartID = cart.CartID,
//                    Quantity = quantity
//                };
//                _context.BookCarts.Add(bookCartItem);
//            }

//            // Salvăm modificările în baza de date
//            await _context.SaveChangesAsync();

//            return RedirectToAction("Index", "Cart");  // Redirecționăm către pagina cosului
//        }

//        // Actualizarea cantității unui produs din cos
//        [HttpPost]
//        public async Task<IActionResult> UpdateQuantity(int bookId, int quantity)
//        {
//            var user = await _userManager.GetUserAsync(User);

//            if (user == null)
//            {
//                return RedirectToAction("Login", "Account");
//            }

//            var cart = await _context.Carts
//                                     .FirstOrDefaultAsync(c => c.UserID == user.UserID);

//            if (cart == null)
//            {
//                return RedirectToAction("Index", "Cart");
//            }

//            var bookCartItem = await _context.BookCarts
//                                              .FirstOrDefaultAsync(bc => bc.BookID == bookId && bc.CartID == cart.CartID);

//            if (bookCartItem != null)
//            {
//                bookCartItem.Quantity = quantity;
//                _context.BookCarts.Update(bookCartItem);
//                await _context.SaveChangesAsync();
//            }

//            return RedirectToAction("Index", "Cart");
//        }

//        // Eliminarea unui produs din cos
//        [HttpPost]
//        public async Task<IActionResult> RemoveFromCart(int bookId)
//        {
//            var user = await _userManager.GetUserAsync(User);

//            if (user == null)
//            {
//                return RedirectToAction("Login", "Account");
//            }

//            var cart = await _context.Carts
//                                     .FirstOrDefaultAsync(c => c.UserID == user.UserID);

//            if (cart == null)
//            {
//                return RedirectToAction("Index", "Cart");
//            }

//            var bookCartItem = await _context.BookCarts
//                                              .FirstOrDefaultAsync(bc => bc.BookID == bookId && bc.CartID == cart.CartID);

//            if (bookCartItem != null)
//            {
//                _context.BookCarts.Remove(bookCartItem);
//                await _context.SaveChangesAsync();
//            }

//            return RedirectToAction("Index", "Cart");
//        }
//    }
//}








/*
 * ---------------------------------------------------------------------------
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using LibrarieOnline.Data;
using LibrarieOnline.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LibrarieOnline.Controllers
{
    [Authorize]
    public class BookCartController : Controller
    {
        private readonly LibrarieOnlineContext _context;

        public BookCartController(LibrarieOnlineContext context)
        {
            _context = context;
        }

        // Metoda pentru adăugarea unei cărți în coș
        [HttpPost]
        public async Task<IActionResult> AddToCart(int bookId)
        {
            // Obține ID-ul utilizatorului curent
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            // Găsește coșul asociat utilizatorului
            var cart = await _context.Carts.Include(c => c.BookCarts)
                                           .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                // Creează un coș nou dacă nu există
                cart = new CartModel
                {
                    UserId = userId
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var book = await _context.Books.FirstOrDefaultAsync(b => b.BookID == bookId);
            if (book == null)
            {
                return NotFound("Cartea nu a fost găsită.");
            }

            // Verifică dacă cartea este deja în coș
            var bookCart = cart.BookCarts?.FirstOrDefault(bc => bc.BookID == bookId);
            if (bookCart != null)
            {
                // Crește cantitatea dacă există deja
                bookCart.Quantity++;
            }
            else
            {
                // Adaugă cartea în coș
                bookCart = new BookCartModel
                {
                    BookID = bookId,
                    CartID = cart.CartID,
                    Quantity = 1
                };

                _context.BookCarts.Add(bookCart);
            }

            await _context.SaveChangesAsync();

            Console.WriteLine($"Cartea '{book.Title}' a fost adăugată în coș.");

            return RedirectToAction("Index", "Book");
        }

        // Metoda pentru afișarea coșului
        public async Task<IActionResult> ViewCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var cart = await _context.Carts.Include(c => c.BookCarts)
                                           .ThenInclude(bc => bc.Book)
                                           .ThenInclude(b => b.Category)
                                           .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || cart.BookCarts == null || !cart.BookCarts.Any())
            {
                ViewBag.Message = "Coșul este gol.";
                return View(new List<BookCartModel>());
            }

            // Calculul prețului total
            ViewBag.TotalPrice = cart.BookCarts.Sum(bc => bc.Book.Price * bc.Quantity);
            return View(cart.BookCarts);
        }

        // Metoda pentru ștergerea unei cărți din coș
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int bookId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var cart = await _context.Carts.Include(c => c.BookCarts)
                                           .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return NotFound();
            }

            var bookCart = cart.BookCarts?.FirstOrDefault(bc => bc.BookID == bookId);
            if (bookCart != null)
            {
                _context.BookCarts.Remove(bookCart);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ViewCart");
        }

        [HttpGet]
        public async Task<IActionResult> IncreaseQuantity(int bookId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var cart = await _context.Carts.Include(c => c.BookCarts)
                                           .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return NotFound();
            }

            var bookCart = cart.BookCarts?.FirstOrDefault(bc => bc.BookID == bookId);
            if (bookCart != null)
            {
                bookCart.Quantity++;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ViewCart");
        }

        [HttpGet]
        public async Task<IActionResult> DecreaseQuantity(int bookId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var cart = await _context.Carts.Include(c => c.BookCarts)
                                           .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                return NotFound();
            }

            var bookCart = cart.BookCarts?.FirstOrDefault(bc => bc.BookID == bookId);
            if (bookCart != null)
            {
                if (bookCart.Quantity > 1)
                {
                    bookCart.Quantity--;
                }
                else
                {
                    _context.BookCarts.Remove(bookCart);
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ViewCart");
        }
        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> ApplyDiscount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("Utilizatorul nu a fost găsit.");
            }

            // Obține numărul de puncte acumulate de utilizator
            int nrPoints = user.Points;

            var cart = await _context.Carts.Include(c => c.BookCarts)
                                           .ThenInclude(bc => bc.Book)
                                           .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || cart.BookCarts == null || !cart.BookCarts.Any())
            {
                return Json(new { newTotalPrice = 0 });
            }

            // Calculul prețului inițial
            decimal totalPrice = cart.BookCarts.Sum(bc => bc.Book.Price * bc.Quantity);

            // Aplică reducerea: scade 0.2 * nrPuncte din totalPrice
            decimal discount = 0.2m * nrPoints;
            decimal newTotalPrice = Math.Max(totalPrice - discount, 0);

            // Salvăm temporar reducerea pentru a fi aplicată la finalizarea comenzii
            HttpContext.Session.SetInt32("PointsUsed", (int)(discount / 0.2m));

            return Json(new { newTotalPrice });
        }


    }
}
