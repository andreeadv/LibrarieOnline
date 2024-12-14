using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibrarieOnline.Data;
using LibrarieOnline.Models;
using System.Linq;
using System.Threading.Tasks;

namespace LibrarieOnline.Controllers
{
    public class BookCartController : Controller
    {
        private readonly LibrarieOnlineContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookCartController(LibrarieOnlineContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Adăugarea unei cărți în cos
        [HttpPost]
        public async Task<IActionResult> AddToCart(int bookId, int quantity)
        {
            var user = await _userManager.GetUserAsync(User);  // Obține utilizatorul curent

            if (user == null)
            {
                return RedirectToAction("Login", "Account");  // Dacă nu există utilizator, redirecționează la Login
            }

            // Căutăm cosul utilizatorului
            var cart = await _context.Carts
                                     .FirstOrDefaultAsync(c => c.UserID == user.UserID);

            // Dacă nu există cos pentru utilizator, creăm unul nou
            if (cart == null)
            {
                cart = new CartModel { UserID = user.UserID };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // Căutăm dacă produsul este deja în cos
            var existingBookInCart = await _context.BookCarts
                                                    .FirstOrDefaultAsync(bc => bc.BookID == bookId && bc.CartID == cart.CartID);

            if (existingBookInCart != null)
            {
                // Dacă produsul există deja în cos, actualizăm cantitatea
                existingBookInCart.Quantity += quantity;
                _context.BookCarts.Update(existingBookInCart);
            }
            else
            {
                // Dacă produsul nu există în cos, adăugăm o nouă intrare în BookCart
                var bookCartItem = new BookCartModel
                {
                    BookID = bookId,
                    CartID = cart.CartID,
                    Quantity = quantity
                };
                _context.BookCarts.Add(bookCartItem);
            }

            // Salvăm modificările în baza de date
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Cart");  // Redirecționăm către pagina cosului
        }

        // Actualizarea cantității unui produs din cos
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int bookId, int quantity)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = await _context.Carts
                                     .FirstOrDefaultAsync(c => c.UserID == user.UserID);

            if (cart == null)
            {
                return RedirectToAction("Index", "Cart");
            }

            var bookCartItem = await _context.BookCarts
                                              .FirstOrDefaultAsync(bc => bc.BookID == bookId && bc.CartID == cart.CartID);

            if (bookCartItem != null)
            {
                bookCartItem.Quantity = quantity;
                _context.BookCarts.Update(bookCartItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Cart");
        }

        // Eliminarea unui produs din cos
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int bookId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = await _context.Carts
                                     .FirstOrDefaultAsync(c => c.UserID == user.UserID);

            if (cart == null)
            {
                return RedirectToAction("Index", "Cart");
            }

            var bookCartItem = await _context.BookCarts
                                              .FirstOrDefaultAsync(bc => bc.BookID == bookId && bc.CartID == cart.CartID);

            if (bookCartItem != null)
            {
                _context.BookCarts.Remove(bookCartItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Cart");
        }
    }
}
