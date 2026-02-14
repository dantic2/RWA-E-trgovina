using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    [Authorize(Roles = "User")]
    public class CartController : Controller
    {
        private readonly EcommerceDbContext _context;

        public CartController(EcommerceDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            if (quantity < 1)
            {
                return BadRequest("Quantity must be at least 1");
            }

            var product = await _context.Products.FindAsync(productId);

            if (product == null)
            {
                return NotFound();
            }

            if (product.Stock < quantity)
            {
                TempData["ErrorMessage"] = $"Only {product.Stock} items available in stock";
                return RedirectToAction("Details", "Product", new {id = productId});
            }

            var cart = GetCart();

            // check if item already exists in cart
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem != null)
            {
                int newQuantity = existingItem.Quantity + quantity;
                if (newQuantity > product.Stock)
                {
                    TempData["ErrorMessage"] = $"Cannot add {quantity} more items. Only {product.Stock - existingItem.Quantity} additional items available in stock";
                    return RedirectToAction("Details", "Product", new {id = productId});
                }
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Items.Add(new CartItemViewModel
                {
                    ProductId = product.Id,
                    Title = product.Title,
                    Price = product.Price,
                    ImageUrl = product.ImageUrl,
                    Quantity = quantity
                });
            }

            SaveCart(cart);

            TempData["SuccessMessage"] = $"{product.Title} added to cart! ";
            return RedirectToAction("Index");
        }

        //  POST: /Cart/RemoveFromCart 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                cart.Items.Remove(item);
                SaveCart(cart);
                TempData["SuccessMessage"] = $"{item.Title} removed from cart";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            if (quantity < 1)
            {
                return RedirectToAction("Index");
            }

            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                var product = _context.Products.Find(productId);
                if (product == null && quantity > product.Stock)
                {
                    TempData["ErrorMessage"] = $"Only {product.Stock} items available in stock";
                    return RedirectToAction("Index");
                }
                item.Quantity = quantity;
                SaveCart(cart);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout()
        {
            var cart = GetCart();

            if (cart.Items.Count == 0)
            {
                TempData["ErrorMessage"] = "Your cart is empty";
                return RedirectToAction("Index");
            }

            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Unauthorized();
                }

                int userId = int.Parse(userIdClaim);

                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = cart.Total,
                    Status = "Pending"
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var cartItem in cart.Items)
                {
                    var product = await _context.Products.FindAsync(cartItem.ProductId);
                    if(product == null)
                    {
                        throw new Exception($"Product with ID {cartItem.ProductId} not found");
                        return RedirectToAction("Index");
                    }
                    if (product.Stock < cartItem.Quantity)
                    {
                        throw new Exception($"Insufficient stock for product {product.Title}");
                    }

                    product.Stock -= cartItem.Quantity;

                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        PriceAtOrder = cartItem.Price
                    };

                    _context.OrderItems.Add(orderItem);
                }

                await _context.SaveChangesAsync();

                ClearCart();

                TempData["SuccessMessage"] = $"Order #{order.Id} placed successfully!";
                return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Checkout failed: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || order.UserId != int.Parse(userIdClaim))
            {
                return Forbid();
            }

            return View(order);
        }

        //  HELPER METHODS 

        private string GetCartSessionKey()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return "ShoppingCart_Guest";
            }

            return $"ShoppingCart_{userId}";
        }

        private CartViewModel GetCart()
        {
            var cartJson = HttpContext.Session.GetString(GetCartSessionKey());

            if (string.IsNullOrEmpty(cartJson))
            {
                return new CartViewModel();
            }

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var cart = JsonSerializer.Deserialize<CartViewModel>(cartJson, options);

                if (cart == null)
                {
                    return new CartViewModel();
                }

                cart.Items ??= new List<CartItemViewModel>();

                return cart;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Cart deserialization error: {ex.Message}");
                return new CartViewModel();
            }
        }

        private void SaveCart(CartViewModel cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(GetCartSessionKey(), cartJson);
        }

        private void ClearCart()
        {
            HttpContext.Session.Remove(GetCartSessionKey());
        }
    }
}