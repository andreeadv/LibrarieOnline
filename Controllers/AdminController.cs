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
        public async Task<IActionResult> CreateBook(BookModel book)
        {
            if (ModelState.IsValid)
            {
                _context.Books.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction("ManageBooks");
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
        public async Task<IActionResult> EditBook(BookModel book)
        {
            if (ModelState.IsValid)
            {
                _context.Books.Update(book);
                await _context.SaveChangesAsync();
                return RedirectToAction("ManageBooks");
            }
            return View(book);
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
