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
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) return RedirectToAction("Login", "Account");

            // --- 1. Check if the product has supply BEFORE doing anything else ---
            var product = await _context.Products
                .Include(p => p.Supply)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null || product.Supply == null || product.Supply.Quantity <= 0)
            {
                TempData["ErrorMessage"] = "Sorry, this item is currently out of stock.";
                return RedirectToAction("IndexPr1", "Product");
            }

            // 2. Find the user's cart including its current items
            var cart = await _context.shoppingCarts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == userId && c.IsCheckedOut == false); // Ensure we don't add to an old cart

            if (cart == null)
            {
                cart = new ShoppingCart { CustomerId = userId };
                _context.shoppingCarts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // 3. Check if the product is already in the cart
            var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (cartItem != null)
            {
                // --- 4. Ensure adding 1 more doesn't exceed total stock ---
                if (cartItem.Quantity + 1 > product.Supply.Quantity)
                {
                    TempData["ErrorMessage"] = $"Sorry, we only have {product.Supply.Quantity} of these available.";
                    return RedirectToAction("IndexPr1", "Product");
                }

                cartItem.Quantity++;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = 1
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("IndexPr1", "Product");
        }


        // GET: ShoppingCarts/IndexShC
        public async Task<IActionResult> IndexShC()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAdmin = User.IsInRole("Admin");

            var cartsQuery = _context.shoppingCarts.Include(s => s.Items);

            List<ShoppingCart> carts;

            if(isAdmin)
            {
                carts = await cartsQuery.Where(s => s.IsCheckedOut == false).ToListAsync();
            }
            else if (int.TryParse(userIdClaim, out int userId))
            {
                carts = await cartsQuery.Where(s => s.CustomerId == userId && s.IsCheckedOut == false).ToListAsync();
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(int cartId)
        {
            var cart = await _context.shoppingCarts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId);

            if (cart == null || !cart.Items.Any() || cart.IsCheckedOut)
            {
                return RedirectToAction(nameof(IndexShC));
            }

            // Verify authorization
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && cart.CustomerId.ToString() != userIdClaim)
            {
                return Forbid();
            }

            // Loop through the items and deduct the stock
            foreach (var item in cart.Items)
            {
                var product = await _context.Products
                    .Include(p => p.Supply)
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                if (product?.Supply != null)
                {
                    // Concurrency check: If someone bought the last one while this user was browsing
                    if (product.Supply.Quantity < item.Quantity)
                    {
                        TempData["ErrorMessage"] = $"Sorry, {product.Name} just ran out of stock! Please adjust your cart.";
                        return RedirectToAction(nameof(IndexShC));
                    }

                    product.Supply.Quantity -= item.Quantity;
                }
            }

            // Finalize the cart
            cart.IsCheckedOut = true;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your writing instruments have been ordered successfully!";
            return RedirectToAction(nameof(IndexShC));
        }
    }
}