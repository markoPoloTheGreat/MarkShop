using MarkShop.Data;
using MarkShop.Data.ShopSbS.Data;
using MarkShop.Models;
using MarkShop.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace MarkShop.Controllers
{
    public class QuizController : Controller
    {
        private readonly MahalanobisService _service;
        private readonly AppDbContext _context;

        public QuizController(MahalanobisService service, AppDbContext context)
        {
            _service = service;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CalculateResult(QuizViewModel model)
        {
            // 1. Convert Quiz Answers to Math Vector
            double[] userVector = _service.GenerateUserVector(model);

            // 2. Get all pens from DB
            var allPens = await _context.Products
                                        .Where(p => p.Type == ProductType.Pen && p.Vector != null)
                                        .ToListAsync();

            // 3. Find Match
            var bestPen = _service.FindBestMatch(userVector, allPens);

            if (bestPen == null)
            {
                return RedirectToAction("Index");
            }

            // Return the result view (Reuse the Product Details view or a specific Result view)
            return View("Result", bestPen);
        }
    }
}