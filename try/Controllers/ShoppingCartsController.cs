using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarkShop.Data.ShopSbS.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MarkShop.Models;

namespace MarkShop.Controllers
{
    [Authorize]
    public class ShoppingCartsController : Controller
    {
        private readonly AppDbContext _context;

        public ShoppingCartsController(AppDbContext context)
        {
            _context = context;
        }

        // POST: ShoppingCarts/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) return RedirectToAction("Login", "Account");

            // 1. Find the user's cart or create a new one if it doesn't exist
            var cart = await _context.shoppingCarts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == userId);

            if (cart == null)
            {
                cart = new ShoppingCart { CustomerId = userId };
                _context.shoppingCarts.Add(cart);
                await _context.SaveChangesAsync(); // Save to get the Cart Id
            }

            // 2. Check if the product is already in the cart
            var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (cartItem != null)
            {
                // If it exists, just increase quantity
                cartItem.Quantity++;
                _context.Update(cartItem);
            }
            else
            {
                // If it's new, add a new CartItem
                var newItem = new CartItem
                {
                    ProductId = productId,
                    Quantity = 1,
                   
                };
                _context.CartItems.Add(newItem);
            }

            await _context.SaveChangesAsync();

            // Redirect to the cart view to show the result
            return RedirectToAction(nameof(IndexShC));
        }

        // GET: ShoppingCarts/IndexShC
        public async Task<IActionResult> IndexShC()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAdmin = User.IsInRole("Admin");

            var cartsQuery = _context.shoppingCarts.Include(s => s.Items);

            List<ShoppingCart> carts;

            if (isAdmin)
            {
                carts = await cartsQuery.ToListAsync();
            }
            else if (int.TryParse(userIdClaim, out int userId))
            {
                carts = await cartsQuery.Where(s => s.CustomerId == userId).ToListAsync();
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

            var productIds = carts.SelectMany(c => c.Items).Select(i => i.ProductId).Distinct().ToList();
            var productsDict = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            ViewBag.Products = productsDict;
            return View(carts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearCart(int id)
        {
            var cart = await _context.shoppingCarts
                .Include(s => s.Items)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cart == null) return NotFound();

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && cart.CustomerId.ToString() != userIdClaim)
            {
                return Forbid();
            }

            if (cart.Items != null && cart.Items.Any())
            {
                _context.CartItems.RemoveRange(cart.Items);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(IndexShC));
        }

        // POST: ShoppingCarts/DeleteItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(int itemId)
        {
            var item = await _context.CartItems.FindAsync(itemId);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(IndexShC));
        }
    }
}