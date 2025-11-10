using MarkShop.Data.ShopSbS.Data;
using MarkShop.Models;
using Microsoft.AspNetCore.Mvc;
namespace MarkShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult IndexPr1()
        {
            var products = _context.Products.ToList();
            return View(products);
        }
        public IActionResult CreatePr()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CreatePr(Product product)
        {
            if (ModelState.IsValid)
            {
                // If the user entered an image name, prepend /images/
                if (!string.IsNullOrWhiteSpace(product.ImageUrl) && !product.ImageUrl.StartsWith("/images/"))
                {
                    product.ImageUrl = "/images/" + product.ImageUrl.TrimStart('/');
                }
                _context.Products.Add(product);
                _context.SaveChanges();
                return RedirectToAction(nameof(IndexPr1));
            }
            return View(product);
        }
        public IActionResult EditPr(int id)
        {
            return View(id);
        }
    }
}
