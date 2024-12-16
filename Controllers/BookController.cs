﻿

using LibrarieOnline.Data;
using LibrarieOnline.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibrarieOnline.Controllers
{
    public class BookController : Controller
    {
        private readonly LibrarieOnlineContext _context;
        public BookModel? CurrentBook { get; set; }

        int pointsEarned = 0;

        // Constructorul
        public BookController(LibrarieOnlineContext context)
        {
            _context = context;
        }

        // Afișarea tuturor cărților
        [HttpGet]
        public IActionResult Index()
        {
            var books = _context.Books.Include(book => book.Category).Include(b => b.Comments).ToList();
            
            foreach (var book in books)
            {
                if (book.Comments != null && book.Comments.Any())
                {
                    book.AvgRating = (int)book.Comments.Average(c => c.Rating);
                }
                else
                {
                    book.AvgRating = 0; // Fără rating
                }
            }

            if (books == null || !books.Any())
            {
                ViewBag.Message = "Nu există cărți disponibile.";
                ViewBag.Books = new List<BookModel>();
            }
            else
            {
                var search = "";
                ViewBag.Books = books;
                int _perPage = 6;
                int totalItems = books.Count();
                if (TempData.ContainsKey("message"))
                {
                    ViewBag.message = TempData["message"].ToString();
                }
                // Se preia pagina curenta din View-ul asociat
                // Numarul paginii este valoarea parametrului page din ruta
                // /Articles/Index?page=valoare


                // Pentru prima pagina offsetul o sa fie zero
                // Pentru pagina 2 o sa fie 3 
                // Asadar offsetul este egal cu numarul de articole 
                // care au fost deja afisate pe paginile anterioare
                var offset = 0;

                // Se calculeaza offsetul in functie de numarul 
                // paginii la care suntem

                var currentPage = 1;
                if (HttpContext.Request.Query.ContainsKey("page"))
                {
                    currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);
                }


                if (!currentPage.Equals(0))
                {
                    offset = (currentPage - 1) * _perPage;
                }

                var paginatedBooks = books.Skip(offset).Take(_perPage);

                // Preluam numarul ultimei pagini

                ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

                // Trimitem articolele cu ajutorul unui ViewBag catre View-ul corespunzator
                ViewBag.Books = paginatedBooks;

                if (search != "")
                {
                    ViewBag.PaginationBaseUrl = "/Books/Index/?search=" + search + "&page";
                }
                else
                {
                    ViewBag.PaginationBaseUrl = "/Books/Index/?page";
                }

            }
            return View();
        }


        // Detalii despre o anumită carte
        public IActionResult Details(int bookId)
        {
            var book = _context.Books
                .Include(b => b.Category) // Include relația cu Category
                .FirstOrDefault(b => b.BookID == bookId);

            if (book == null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(book);
        }

        public IActionResult Book(int bookId)
        {
            var book = _context.Books
                               .Include(b => b.Category)
                               .Include(b => b.Comments)
                               .ThenInclude(c => c.User) 
                               .FirstOrDefault(b => b.BookID == bookId);

            if (book == null)
            {
                return RedirectToAction("Error", "Home");
            }

            if (book.Comments != null && book.Comments.Any())
            {
                book.AvgRating = (int)book.Comments.Average(c => c.Rating);
            }
            else
            {
                book.AvgRating = 0; // Dacă nu sunt comentarii, ratingul e 0
            }

            return View(book);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddReview(int bookId, string content, int rating)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized("Trebuie să fii autentificat pentru a adăuga o recenzie.");
            }
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var reward = await _context.Rewards.FirstOrDefaultAsync(r => r.RewardID == 2);
           
            if (currentUser != null)
            {
                currentUser.Points += (int)reward.Points; // Adaugăm punctele la utilizator
                _context.Users.Update(currentUser);
                pointsEarned = (int)reward.Points;
            }
            var review = new CommentModel
            {
                BookID = bookId,
                UserId = userId,
                Content = content,
                Rating = rating,
                Date = DateTime.Now
            };

            _context.Comments.Add(review);
            await _context.SaveChangesAsync();


            TempData["Message"] = $"Comanda a fost plasată cu succes! Ați primit {pointsEarned} puncte.";
            return RedirectToAction("Book", "Book", new { bookId = bookId });

        }

    }
}
