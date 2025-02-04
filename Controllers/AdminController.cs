using LibrarieOnline.Data;
using LibrarieOnline.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace LibrarieOnline.Controllers
{
    [Authorize(Roles = "Admin")] // Restricționează accesul doar pentru admini
    public class AdminController : Controller
    {
        private readonly LibrarieOnlineContext _context;

        public AdminController(LibrarieOnlineContext context)
        {
            _context = context;
        }

        // 🎯 Panoul de administrare
        public IActionResult Index()
        {
            return View();
        }

        // 🎯 CRUD pentru Quiz-uri
        public async Task<IActionResult> ManageQuizzes()
        {
            var quizzes = await _context.Quizzes.ToListAsync();
            return View(quizzes);
        }

        public IActionResult CreateQuiz()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuiz(QuizModel quiz)
        {
            if (ModelState.IsValid)
            {
                _context.Quizzes.Add(quiz);
                await _context.SaveChangesAsync();
                return RedirectToAction("ManageQuizzes");
            }
            return View(quiz);
        }

        public async Task<IActionResult> EditQuiz(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null) return NotFound();
            return View(quiz);
        }

        [HttpPost]
        public async Task<IActionResult> EditQuiz(QuizModel quiz)
        {
            if (ModelState.IsValid)
            {
                _context.Quizzes.Update(quiz);
                await _context.SaveChangesAsync();
                return RedirectToAction("ManageQuizzes");
            }
            return View(quiz);
        }

        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null) return NotFound();

            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();
            return RedirectToAction("ManageQuizzes");
        }

        // 🎯 CRUD pentru Cărți
        public async Task<IActionResult> ManageBooks()
        {
            var books = await _context.Books.ToListAsync();
            return View(books);
        }

        public IActionResult CreateBook()
        {
            return View();
        }

        [HttpPost]
        [HttpPost]
        [HttpPost]
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> CreateBook(BookModel book)
        {
            // Debugging: Afișăm datele primite în consolă
            Console.WriteLine("===== DEBUG: Adăugare Carte =====");
            Console.WriteLine($"Titlu: {book.Title}");
            Console.WriteLine($"Autor: {book.Author}");
            Console.WriteLine($"Preț: {book.Price}");
            Console.WriteLine($"Descriere: {book.Description}");
            Console.WriteLine($"Imagine: {book.Image}");
            Console.WriteLine($"Categorie ID: {book.CategoryID}");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBook = await _context.Books
                        .FirstOrDefaultAsync(b => b.Title == book.Title && b.Author == book.Author);

                    if (existingBook != null)
                    {
                        Console.WriteLine("Această carte există deja în baza de date!");
                        ModelState.AddModelError("", "O carte cu acest titlu și autor există deja.");
                        return View(book);
                    }

                    await _context.Books.AddAsync(book);
                    Console.WriteLine("Cartea a fost adăugată în context. Salvăm modificările...");

                    await _context.SaveChangesAsync();
                    Console.WriteLine("Cartea a fost adăugată cu succes!");

                    TempData["SuccessMessage"] = "Cartea a fost adăugată cu succes!";
                    return RedirectToAction("ManageBooks");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Eroare la adăugare: {ex.Message}");
                    ModelState.AddModelError("", "A apărut o eroare în timpul adăugării cărții.");
                }
            }
            else
            {
                Console.WriteLine("ModelState NU este valid!");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"EROARE: {error.ErrorMessage}");
                }
            }

            return View(book);
        }



        public async Task<IActionResult> EditBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();
            return View(book);
        }

        [HttpPost]
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> EditBook(int id, BookModel book)
        {
            Console.WriteLine($"BookID: {book.BookID}");
            Console.WriteLine($"Title: {book.Title}");
            Console.WriteLine($"Author: {book.Author}");
            Console.WriteLine($"Price: {book.Price}");

            if (id != book.BookID)
            {
                Console.WriteLine("ID-urile nu se potrivesc!");
                return BadRequest();
            }

            var existingBook = await _context.Books.FindAsync(id);
            if (existingBook == null)
            {
                Console.WriteLine("Cartea nu există în baza de date!");
                return NotFound();
            }

            existingBook.Title = book.Title;
            existingBook.Author = book.Author;
            existingBook.Price = book.Price;
            existingBook.Description = book.Description;
            existingBook.Image = book.Image;
            existingBook.CategoryID = book.CategoryID;

            await _context.SaveChangesAsync();
            Console.WriteLine("Modificările au fost salvate cu succes!");

            return RedirectToAction("ManageBooks");
        }



        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return RedirectToAction("ManageBooks");
        }

        // 🎯 Ștergere recenzii
        public async Task<IActionResult> ManageReviews()
        {
            var reviews = await _context.Comments.Include(c => c.User).Include(c => c.Book).ToListAsync();
            return View(reviews);
        }

        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Comments.FindAsync(id);
            if (review == null) return NotFound();

            _context.Comments.Remove(review);
            await _context.SaveChangesAsync();
            return RedirectToAction("ManageReviews");
        }
    }
}
