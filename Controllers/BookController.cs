using LibrarieOnline.Data;
using LibrarieOnline.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibrarieOnline.Controllers
{
    public class BookController : Controller
    {
        private readonly LibrarieOnlineContext _context;
        public BookModel? CurrentBook { get; set; }
        int pointsEarned = 0; // Pentru punctele de recompensă


        // Constructorul
        public BookController(LibrarieOnlineContext context)
        {
            _context = context;
        }
   

        // Afișarea tuturor cărților (combinată)
        [HttpGet]
        public IActionResult Index()
        {
            var books = _context.Books.Include(b => b.Category).Include(b => b.Comments).AsQueryable();

            // Calcularea ratingului mediu pentru fiecare carte
            foreach (var book in books)
            {
                if (book.Comments != null && book.Comments.Any())
                {
                    book.AvgRating = (int)book.Comments.Average(c => c.Rating);
                }
                else
                {
                    book.AvgRating = 0;
                }
            }

            // MOTOR DE CĂUTARE
            var search = Convert.ToString(HttpContext.Request.Query["search"])?.Trim() ?? "";
            if (!string.IsNullOrEmpty(search))
            {
                var bookIds = _context.Books.Where(b => b.Title.Contains(search) || b.Description.Contains(search))
                    .Select(b => b.BookID).ToList();

                var bookIdsFromComments = _context.Comments.Where(c => c.Content.Contains(search))
                    .Select(c => c.BookID ?? 0).ToList();

                var mergedBookIds = bookIds.Union(bookIdsFromComments).ToList();
                books = books.Where(b => mergedBookIds.Contains(b.BookID));
            }

            // FILTRARE
            var author = Convert.ToString(HttpContext.Request.Query["author"])?.Trim();
            if (!string.IsNullOrEmpty(author)) books = books.Where(b => b.Author.Contains(author));

            var category = Convert.ToString(HttpContext.Request.Query["category"])?.Trim();
            if (!string.IsNullOrEmpty(category)) books = books.Where(b => b.Category.CategoryName.Contains(category));

            decimal? minPrice = null;
            if (decimal.TryParse(Convert.ToString(HttpContext.Request.Query["minPrice"]), out var parsedMinPrice))
            {
                minPrice = parsedMinPrice;
                books = books.Where(b => b.Price >= minPrice);
            }

            decimal? maxPrice = null;
            if (decimal.TryParse(Convert.ToString(HttpContext.Request.Query["maxPrice"]), out var parsedMaxPrice))
            {
                maxPrice = parsedMaxPrice;
                books = books.Where(b => b.Price <= maxPrice);
            }

            // SORTARE
            var sortBy = Convert.ToString(HttpContext.Request.Query["sortBy"]);
            switch (sortBy)
            {
                case "price_asc": books = books.OrderBy(b => b.Price); break;
                case "price_desc": books = books.OrderByDescending(b => b.Price); break;
                case "title": books = books.OrderBy(b => b.Title); break;
                case "author": books = books.OrderBy(b => b.Author); break;
            }

            // PAGINARE
            int _perPage = 8;
            int totalItems = books.Count();
            var currentPage = HttpContext.Request.Query.ContainsKey("page") ? Convert.ToInt32(HttpContext.Request.Query["page"]) : 1;
            var offset = (currentPage - 1) * _perPage;
            var paginatedBooks = books.Skip(offset).Take(_perPage).ToList();

            // ViewBag pentru View
            ViewBag.Books = paginatedBooks;
            ViewBag.SearchString = search;
            ViewBag.Author = author;
            ViewBag.Category = category;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SortBy = sortBy;
            ViewBag.CurrentPage = currentPage;
            ViewBag.LastPage = (int)Math.Ceiling((double)totalItems / _perPage);

            var baseUrl = "/Book/Index/?";
            if (!string.IsNullOrEmpty(search)) baseUrl += $"search={search}&";
            if (!string.IsNullOrEmpty(author)) baseUrl += $"author={author}&";
            if (!string.IsNullOrEmpty(category)) baseUrl += $"category={category}&";
            if (minPrice.HasValue) baseUrl += $"minPrice={minPrice}&";
            if (maxPrice.HasValue) baseUrl += $"maxPrice={maxPrice}&";
            if (!string.IsNullOrEmpty(sortBy)) baseUrl += $"sortBy={sortBy}&";

            ViewBag.PaginationBaseUrl = baseUrl + "page";
            return View();
        }

        // Detalii despre o anumită carte
        public IActionResult Details(int bookId)
        {
            var book = _context.Books.Include(b => b.Category).FirstOrDefault(b => b.BookID == bookId);
            if (book == null) return RedirectToAction("Error", "Home");
            return View(book);
        }

        public IActionResult Book(int bookId)
        {
            var book = _context.Books.Include(b => b.Category)
                .Include(b => b.Comments).ThenInclude(c => c.User)
                .FirstOrDefault(b => b.BookID == bookId);

            if (book == null) return RedirectToAction("Error", "Home");

            book.AvgRating = book.Comments?.Any() == true ? (int)book.Comments.Average(c => c.Rating) : 0;
            return View(book);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddReview(int bookId, string content, int rating)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized("Trebuie să fii autentificat pentru a adăuga o recenzie.");

            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var reward = await _context.Rewards.FirstOrDefaultAsync(r => r.RewardID == 2);

            if (currentUser != null)
            {
                currentUser.Points += (int)reward.Points;
                _context.Users.Update(currentUser);
                pointsEarned = (int)reward.Points;
            }

            if (currentUser != null)
            {
                currentUser.Points += (int)reward.Points;
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

            TempData["Message"] = $"Recenzia a fost adăugată cu succes! Ați primit {pointsEarned} puncte.";
            return RedirectToAction("Book", "Book", new { bookId });
        }
    }
}
