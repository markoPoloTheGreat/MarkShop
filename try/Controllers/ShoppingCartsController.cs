using MarkShop.Data.ShopSbS.Data;
using MarkShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MarkShop.Controllers
{
    public class ShoppingCartsController : Controller
    {
        private readonly AppDbContext _context;

        public ShoppingCartsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ShoppingCarts
        public async Task<IActionResult> IndexShC()
        {
            // If Admin, show everything
            if (User.IsInRole("Admin"))
            {
                var allCarts = await _context.shoppingCarts
                    .Include(s => s.Items)
                    .ToListAsync();
                return View(allCarts);
            }

            // If Customer, show only THEIR cart
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userCarts = await _context.shoppingCarts
                .Where(s => s.CustomerId == userId)
                .Include(s => s.Items)
                .ToListAsync();

            return View(userCarts);
        }

        // GET: ShoppingCarts/Details/5
        // GET: ShoppingCarts/Details/5

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shoppingCart = await _context.shoppingCarts
                .Include(c => c.Items) // <--- FIX 1: Actually load the items from the database
                .FirstOrDefaultAsync(m => m.Id == id);

            if (shoppingCart == null)
            {
                return NotFound();
            }

            // <--- FIX 2: Pass the list of products so the View can find names and prices
            ViewBag.ProductList = await _context.Products.ToListAsync();

            return View(shoppingCart);
        }
        // GET: ShoppingCarts/RemoveFromCart?itemId=5&cartId=10
        public async Task<IActionResult> RemoveFromCart(int itemId, int cartId)
        {
            // 1. Find the specific item in the database
            var cartItem = await _context.CartItems.FindAsync(itemId);

            // 2. Remove it if it exists
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            // 3. Redirect back to the Details view of the specific cart
            return RedirectToAction("Details", new { id = cartId });
        }
        // GET: ShoppingCarts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ShoppingCarts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create1([Bind("Id,CustomerId,IsCheckedOut")] ShoppingCart shoppingCart)
        {
            if (ModelState.IsValid)
            {
                _context.Add(shoppingCart);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(IndexShC));
            }
            return View(shoppingCart);
        }

        // GET: ShoppingCarts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shoppingCart = await _context.shoppingCarts.FindAsync(id);
            if (shoppingCart == null)
            {
                return NotFound();
            }
            return View(shoppingCart);
        }
        public async Task<IActionResult> AddToCart(int productId)
        {
            // 1. Security Check
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");

            var claim = User.FindFirst("CustomerId");
            if (claim == null) return RedirectToAction("Login", "Account");
            int customerId = int.Parse(claim.Value);

            // 2. Load the Cart AND its Items
            // FIX: specific filter added to find only the ACTIVE cart (not checked out)
            var cart = await _context.shoppingCarts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId && !c.IsCheckedOut);

            // 3. Create Cart if it doesn't exist (or if all previous carts are checked out)
            if (cart == null)
            {
                cart = new ShoppingCart { CustomerId = customerId };
                _context.shoppingCarts.Add(cart);

                // CRITICAL STEP: Save NOW to generate the Cart ID
                await _context.SaveChangesAsync();
            }

            // 4. Modify the List (EF handles the Foreign Key automatically)
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = 1
                });
            }

            // 5. Save the final changes (the items)
            await _context.SaveChangesAsync();

            return RedirectToAction("IndexPr1", "Product");
        }
        // POST: ShoppingCarts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CustomerId,IsCheckedOut")] ShoppingCart shoppingCart)
        {
            if (id != shoppingCart.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(shoppingCart);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShoppingCartExists(shoppingCart.Id))
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
            return View(shoppingCart);
        }

        // GET: ShoppingCarts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var sc = await _context.shoppingCarts.FindAsync(id);
            if (sc == null)
            {
                return NotFound();
            }
            _context.shoppingCarts.Remove(sc);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(IndexShC));
        }


        private bool ShoppingCartExists(int id)
        {
            return _context.shoppingCarts.Any(e => e.Id == id);
        }
    }
}
