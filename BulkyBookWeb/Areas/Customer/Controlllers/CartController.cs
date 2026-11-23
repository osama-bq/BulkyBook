using System.Security.Claims;
using BulkyBook.Business.Services.IServices;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBook.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IEmailService _emailService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IApplicationUserService _applicationUserService;

        public CartController(IOrderService orderService, IEmailService emailService, IShoppingCartService shoppingCartService, IApplicationUserService applicationUserService)
        {
            _orderService = orderService;
            _emailService = emailService;
            _shoppingCartService = shoppingCartService;
            _applicationUserService = applicationUserService;
        }

        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity) User.Identity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var cartItems = await _shoppingCartService.GetUserCartItemsAsync(userId);
            var user =  await _applicationUserService.GetUserByIdAsync(userId);
            
            ShoppingCartVM shoppingCartVM = new()
            {
                ShoppingCartList = cartItems,
                OrderHeader = new()
            };
            
            shoppingCartVM.OrderHeader.ApplicationUser = user;
            shoppingCartVM.OrderHeader.ApplicationUserId = userId;
            shoppingCartVM.OrderHeader.Name = user.Name;
            shoppingCartVM.OrderHeader.PhoneNumber = user.PhoneNumber;
            shoppingCartVM.OrderHeader.StreetAddress = user.StreetAddress;
            shoppingCartVM.OrderHeader.City = user.City;
            shoppingCartVM.OrderHeader.State = user.State;
            shoppingCartVM.OrderHeader.PostalCode = user.PostalCode;
            
            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            return View(shoppingCartVM);
        }
        
            
        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPOST(ShoppingCartVM shoppingCartVM)
        {
            var claimsIdentity = (ClaimsIdentity) User.Identity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var cartItems = await _shoppingCartService.GetUserCartItemsAsync(userId);
            
            shoppingCartVM.ShoppingCartList = cartItems;
            shoppingCartVM.OrderHeader.OrderDate = DateTime.UtcNow;
            shoppingCartVM.OrderHeader.ApplicationUserId = userId;
            
            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            
            shoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            shoppingCartVM.OrderHeader.OrderDetails = shoppingCartVM.ShoppingCartList.Select(cart => new OrderDetails
            {
                ProductId = cart.ProductId,
                Price = cart.Price,
                Count = cart.Count,
            }).ToList();
            
            await _orderService.CreateOrderAsync(shoppingCartVM.OrderHeader);
            var user = await _applicationUserService.GetUserByIdAsync(userId);
            await _emailService.SendOrderConfirmationEmailAsync(user.Email,
                shoppingCartVM.OrderHeader.Id, (decimal)shoppingCartVM.OrderHeader.OrderTotal);
            return RedirectToAction("OrderConfirmation", new { id = shoppingCartVM.OrderHeader.Id });
        }
        
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            
            return View(id);
        }

        public async Task<IActionResult> Plus(int cartId)
        {
            var cart = await _shoppingCartService.GetCartByIdAsync(cartId);
            if (cart != null)
            {
                cart.Count++;
                await _shoppingCartService.UpdateCartAsync(cart);
                await UpdateCartSessionAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        
        public async Task<IActionResult> Minus(int cartId)
        {
            var cart = await _shoppingCartService.GetCartByIdAsync(cartId);
            if (cart != null)
            {
                cart.Count--;
                await _shoppingCartService.UpdateCartAsync(cart);
                await UpdateCartSessionAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        
        public async Task<IActionResult> Remove(int cartId)
        {
            var cart = await _shoppingCartService.GetCartByIdAsync(cartId);
            if (cart != null)
            {
                cart.Count = 0;
                await _shoppingCartService.UpdateCartAsync(cart);
                await UpdateCartSessionAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        
        public async Task<IActionResult> UpdateCart(int cartId, int count)
        {
            var cart = await _shoppingCartService.GetCartByIdAsync(cartId);
            if (cart == null)
            {
                return NotFound();
            }

            if (count <= 1)
            {
                count = 0;
            }

            if (count >= 1000)
            {
                count = 1000;
            }
            
            cart.Count = count;
            
            await _shoppingCartService.UpdateCartAsync(cart);
            await UpdateCartSessionAsync();
            return Ok(new { success = true });
        }

        private async Task UpdateCartSessionAsync()
        {
            var claimsIdentity = (ClaimsIdentity) User.Identity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                var count = await _shoppingCartService.GetCartCountAsync(userId);
                HttpContext.Session.SetInt32(SD.SessionCart, count);
            }
        }
    }
}

// 95a87b1b5a7645d484c1383a2d33ad0d
// 2cb1c8ee813f2cc6715116f6a73d0b98
