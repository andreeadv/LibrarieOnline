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

        // Constructorul
        public BookController(LibrarieOnlineContext context)
        {
            _context = context;
        }

        // Afișarea tuturor cărților
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
                    book.AvgRating = 0; // Fără rating
                }
            }

            // MOTOR DE CĂUTARE
            var search = Convert.ToString(HttpContext.Request.Query["search"])?.Trim() ?? "";
            if (!string.IsNullOrEmpty(search))
            {
                // Căutare în titlu, descriere și comentarii
                var bookIds = _context.Books.Where(
                    b => b.Title.Contains(search) || b.Description.Contains(search))
                    .Select(b => b.BookID)
                    .ToList();

                var bookIdsFromComments = _context.Comments.Where(c => c.Content.Contains(search))
                    .Select(c => c.BookID ?? 0)
                    .ToList();

                var mergedBookIds = bookIds.Union(bookIdsFromComments).ToList();
                books = books.Where(b => mergedBookIds.Contains(b.BookID));
            }

            // FILTRARE
            var author = Convert.ToString(HttpContext.Request.Query["author"])?.Trim();
            if (!string.IsNullOrEmpty(author))
            {
                books = books.Where(b => b.Author.Contains(author));
            }

            var category = Convert.ToString(HttpContext.Request.Query["category"])?.Trim();
            if (!string.IsNullOrEmpty(category))
            {
                books = books.Where(b => b.Category.CategoryName.Contains(category));
            }

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
                case "price_asc":
                    books = books.OrderBy(b => b.Price);
                    break;
                case "price_desc":
                    books = books.OrderByDescending(b => b.Price);
                    break;
                case "title":
                    books = books.OrderBy(b => b.Title);
                    break;
                case "author":
                    books = books.OrderBy(b => b.Author);
                    break;
            }

            // PAGINARE
            int _perPage = 6;
            int totalItems = books.Count();
            var currentPage = 1;

            if (HttpContext.Request.Query.ContainsKey("page"))
            {
                currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);
            }

            var offset = (currentPage - 1) * _perPage;
            var paginatedBooks = books.Skip(offset).Take(_perPage).ToList();

            // Date pentru View
            ViewBag.Books = paginatedBooks;
            ViewBag.SearchString = search;
            ViewBag.Author = author;
            ViewBag.Category = category;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SortBy = sortBy;
            ViewBag.CurrentPage = currentPage;
            ViewBag.LastPage = (int)Math.Ceiling((double)totalItems / _perPage);

            // URL pentru paginație
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
            var book = _context.Books
                .Include(b => b.Category)
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

            TempData["Message"] = "Recenzia a fost adăugată cu succes!";
            return RedirectToAction("Book", "Book", new { bookId = bookId });
        }
    }
}
