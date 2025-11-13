using MarkShop.Data.ShopSbS.Data;
using MarkShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> EditPr(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        //POST: ShoppingCarts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPr(int id, [Bind("Id,Name,Price,Description,ImageUrl")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Products.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!productExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }
        private bool productExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
