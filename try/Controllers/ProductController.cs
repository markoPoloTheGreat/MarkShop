using Microsoft.AspNetCore.Mvc;
using MarkShop.Data.ShopSbS.Data;
namespace MarkShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult IndexPr()
        {
            var products = _context.Products.ToList();
            return View(products);
        }
    }
}
