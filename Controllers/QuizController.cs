using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibrarieOnline.Data;
using LibrarieOnline.Models;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibrarieOnline.Controllers
{
    [Authorize]
    public class QuizController : Controller
    {
        private readonly LibrarieOnlineContext _context;

        public QuizController(LibrarieOnlineContext context)
        {
            _context = context;
        }

        // 🎯 Pasul 1: Selectează un Quiz aleatoriu
        [HttpGet]
        public async Task<IActionResult> StartQuiz()
        {
            var random = new Random();
            var quizCount = await _context.Quizzes.CountAsync();

            if (quizCount == 0)
            {
                TempData["ErrorMessage"] = "Momentan nu sunt quiz-uri disponibile.";
                return RedirectToAction("Index", "Home");
            }

            // Selectează un ID de quiz random
            var quizIds = await _context.Quizzes.Select(q => q.QuizID).ToListAsync();
            int randomQuizId = quizIds[random.Next(quizIds.Count)];

            return RedirectToAction("QuizPage", new { quizId = randomQuizId });
        }

        // 🎯 Pasul 2: Afișează pagina quiz-ului
        [HttpGet]
        public async Task<IActionResult> QuizPage(int quizId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            // Verifică dacă utilizatorul a rezolvat un quiz în ultimele 10 minute
            var lastAttempt = HttpContext.Session.GetString($"LastQuizAttempt_{userId}");
            if (lastAttempt != null && DateTime.TryParse(lastAttempt, out DateTime lastAttemptTime))
            {
                if ((DateTime.Now - lastAttemptTime).TotalMinutes < 1)
                {
                    TempData["ErrorMessage"] = "Poți încerca un alt quiz peste 10 minute.";
                    return RedirectToAction("Index", "Home");
                }
            }

            var questions = await _context.QuestionQuizzes
                                          .Where(q => q.QuizID == quizId)
                                          .ToListAsync();

            if (questions == null || !questions.Any())
            {
                TempData["ErrorMessage"] = "Acest quiz nu conține întrebări.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.QuizId = quizId;
            return View(questions);
        }

        // 🎯 Pasul 3: Procesează răspunsurile
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> SubmitQuiz(int quizId, Dictionary<int, string> userAnswers)
        {
            var userId = User.Identity.IsAuthenticated ? User.Identity.Name : null;
            if (userId != null)
            {
                HttpContext.Session.SetString($"LastQuizAttempt_{userId}", DateTime.Now.ToString());
            }
            if (userId == null) return Unauthorized();

            var questions = await _context.QuestionQuizzes.Where(q => q.QuizID == quizId).ToListAsync();
            if (questions == null || !questions.Any()) return NotFound("Quiz-ul nu există.");

            var correctAnswers = questions.ToDictionary(q => q.QuestionID, q => q.CorrectAnswer);
            bool isFullyCorrect = correctAnswers.All(q => userAnswers.ContainsKey(q.Key) && userAnswers[q.Key] == q.Value);

            // Stocăm rezultatul pentru evidențiere vizuală
            ViewBag.CorrectAnswers = correctAnswers;
            ViewBag.UserAnswers = userAnswers;
            ViewBag.QuizId = quizId;
            ViewBag.IsFullyCorrect = isFullyCorrect;

            // 🔹 Salvăm timpul ultimei încercări în sesiune
            HttpContext.Session.SetString("LastQuizAttempt", DateTime.Now.ToString());

            // Dacă utilizatorul a răspuns corect la toate întrebările, îi dăm puncte
            if (isFullyCorrect)
            {
                var reward = await _context.Rewards.FirstOrDefaultAsync(r => r.RewardID == 3);
                if (reward != null)
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user != null)
                    {
                        user.Points += (int)reward.Points;
                        _context.Users.Update(user);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            return View("QuizResults", questions);
        }

    }
}
