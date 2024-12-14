//using LibrarieOnline.Data;
//using LibrarieOnline.Models;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using static System.Reflection.Metadata.BlobBuilder;

//namespace LibrarieOnline.Controllers
//{


//        public class BookController(LibrarieOnlineContext context) : Controller
//        {
//            private readonly LibrarieOnlineContext _context = context;
//            public List<BookModel>? Books { get; set; }
//            public BookModel? CurrentBook { get; set; }

//        [HttpGet]
//            public IActionResult Index()
//            {
//                Books = _context.Books.Include(book => book.Category).ToList();
//                if (Books == null)
//                {
//                    return RedirectToAction("Error", "Home");
//                }
//                return View(Books);
//            }

//            public IActionResult Book(int companieId)
//            {
//                CurrentBook = _context.Books
//                    .Where(book => book.BookID == companieId).Include(book => book.Title).FirstOrDefault();
//                if (CurrentBook == null)
//                {
//                    return RedirectToAction("Error", "Home");
//                }
//                return View(CurrentBook);
//            }
//        }
//}




using LibrarieOnline.Data;
using LibrarieOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibrarieOnline.Controllers
{
    public class BookController : Controller
    {
        private readonly LibrarieOnlineContext _context;

        // Constructorul
        public BookController(LibrarieOnlineContext context)
        {
            _context = context;
        }

        // Afișarea tuturor cărților
        [HttpGet]
        public IActionResult Index()
        {
            var books = _context.Books.Include(book => book.Category).ToList();
            if (books == null || !books.Any())
            {
                ViewBag.Message = "Nu există cărți disponibile.";
                ViewBag.Books = new List<BookModel>();
            }
            else
            {
                var search = "";
                ViewBag.Books = books;
                int _perPage = 3;
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
    }
}
