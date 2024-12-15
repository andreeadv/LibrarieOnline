using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using LibrarieOnline.Data;
using LibrarieOnline.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Net.Mail;
using System.Net; // Pentru trimiterea email-urilor

namespace LibrarieOnline.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly LibrarieOnlineContext _context;

        public OrderController(LibrarieOnlineContext context)
        {
            _context = context;
        }

        // Metoda pentru afișarea formularului de finalizare comandă
        [HttpGet]
        public async Task<IActionResult> FinalizeOrder()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var cart = await _context.Carts.Include(c => c.BookCarts)
                                           .ThenInclude(bc => bc.Book)
                                           .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || cart.BookCarts == null || !cart.BookCarts.Any())
            {
                ViewBag.Message = "Coșul este gol. Nu puteți finaliza comanda.";
                return RedirectToAction("ViewCart", "BookCart");
            }

            var totalPrice = cart.BookCarts.Sum(bc => bc.Book.Price * bc.Quantity);

            var order = new OrderModel
            {
                UserId = userId,
                CartID = cart.CartID,
                Total = totalPrice.ToString("F2"),
                Status = "În așteptare"
            };

            return View(order);
        }
        // Metoda pentru procesarea formularului de finalizare comandă
        // Metoda pentru procesarea formularului de finalizare comandă
        // Metoda pentru procesarea formularului de finalizare comandă
        [HttpPost]
        public async Task<IActionResult> FinalizeOrder(OrderModel order)
        {
            if (!ModelState.IsValid)
            {
                return View(order);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var cart = await _context.Carts.Include(c => c.BookCarts)
                                           .ThenInclude(bc => bc.Book)
                                           .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || cart.BookCarts == null || !cart.BookCarts.Any())
            {
                ViewBag.Message = "Coșul este gol. Nu puteți finaliza comanda.";
                return RedirectToAction("ViewCart", "BookCart");
            }

            var totalPrice = cart.BookCarts.Sum(bc => bc.Book.Price * bc.Quantity);

            order.UserId = userId;
            order.CartID = cart.CartID;
            order.Status = "Comandă plasată";
            order.PaymentType = order.PaymentType; // Preluăm PaymentType din formular
            order.Total = totalPrice.ToString("F2"); // Setăm prețul total

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Atribuie punctele utilizatorului conectat din Reward cu ID-ul 1
            var reward = await _context.Rewards.FirstOrDefaultAsync(r => r.RewardID == 1);
            if (reward != null)
            {
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (currentUser != null)
                {
                    currentUser.Points += (int)reward.Points; // Adaugăm punctele la utilizator
                    _context.Users.Update(currentUser);
                }

                var userReward = new UserRewardModel
                {
                    UserId = userId,
                    RewardID = reward.RewardID,
                    ClaimedDate = DateTime.Now
                };
                _context.UserRewards.Add(userReward);
                await _context.SaveChangesAsync();
            }

            // Goliți coșul după finalizarea comenzii
            _context.BookCarts.RemoveRange(cart.BookCarts);
            await _context.SaveChangesAsync();

            /*
            // Trimite email de confirmare
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null)
            {
                await SendConfirmationEmail(user.Email, order, cart);
            }
            */

            ViewBag.Message = "Comanda a fost plasată cu succes!";
            return RedirectToAction("Index", "Books");
        }

        /*
        private async Task SendConfirmationEmail(string userEmail, OrderModel order, CartModel cart)
        {
            var fromAddress = new MailAddress("librarie@gmail.com", "Librarie Online");
            var toAddress = new MailAddress(userEmail);
            const string fromPassword = "Librarie1!";
            const string subject = "Confirmare comandă - Librarie Online";

            string body = $"<h1>Confirmarea comenzii</h1>" +
                          $"<p>Stimate client,</p>" +
                          $"<p>Comanda dumneavoastră a fost plasată cu succes!</p>" +
                          $"<p><strong>Detalii comandă:</strong></p>" +
                          $"<ul>" +
                          $"<li>Total: {order.Total} lei</li>" +
                          $"<li>Status: {order.Status}</li>" +
                          $"<li>Metoda de plată: {order.PaymentType}</li>" +
                          $"</ul>" +
                          $"<p>Vă mulțumim că ați ales Librarie Online!</p>";

            using (var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            })
            {
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    await smtp.SendMailAsync(message);
                }
            }
        }
        */
    }
}
