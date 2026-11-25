using System.Security.Claims;
using BulkyBook.Business.Services.IServices;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Stripe;

namespace BulkyBook.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.RoleAdmin)]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        
        [BindProperty]
        public OrderHeader OrderHeader { get; set; }
        
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return View();
        }
        
        [AllowAnonymous]
        public async Task<IActionResult> Details(int orderId)
        {
            OrderHeader = await _orderService.GerOrderByIdAsync(orderId, includeDetails: true, includeUser: true);
            return View(OrderHeader);
        }

        [HttpPost]
        [Authorize(Roles = SD.RoleAdmin + "," + SD.RoleEmployee)]
        public async Task<IActionResult> UpdateOrderDetails()
        {
            var orderHeaderFromDb = await _orderService.GerOrderByIdAsync(OrderHeader.Id);
            
            orderHeaderFromDb.Name =  OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber =  OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress =  OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderHeader.City;
            orderHeaderFromDb.State = OrderHeader.State;
            orderHeaderFromDb.PostalCode = OrderHeader.PostalCode;

            if (!string.IsNullOrEmpty(OrderHeader.Carrier) && orderHeaderFromDb.OrderStatus == SD.StatusShipped)
            {
                orderHeaderFromDb.Carrier = OrderHeader.Carrier;
            }

            if (!string.IsNullOrEmpty(OrderHeader.TrackingNumber) && orderHeaderFromDb.OrderStatus == SD.StatusShipped)
            {
                orderHeaderFromDb.TrackingNumber = OrderHeader.TrackingNumber;
            }
            
            await _orderService.UpdateOrderAsync(orderHeaderFromDb);
            TempData["Success"] = "Order Details updated successfully";
            
            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
        }
        
        [HttpPost]
        [Authorize(Roles = SD.RoleAdmin + "," + SD.RoleEmployee)]
        public async Task<IActionResult> UpdateOrderStatus(string status)
        {
            var orderHeader = await _orderService.GerOrderByIdAsync(OrderHeader.Id);
            if (orderHeader == null)
            {
                TempData["error"] = "Order Not Found";
                return RedirectToAction(nameof(Index));
            }

            string successMessage;

            switch (status)
            {
                case SD.StatusProcessing:
                    await _orderService.UpdateOrderStatusAsync(OrderHeader.Id, status);
                    successMessage = "Order processing started successfully";
                    break;
                case SD.StatusCancelled:
                case SD.StatusRefunded:
                    try
                    {
                        bool refundIssued = await _orderService.CancelOrderWithRefundAsync(OrderHeader.Id);
                        if (refundIssued)
                        {
                            successMessage =
                                "Order cancelled and refund issued successfully. Funds will be returned to customer within 5-10 business days.";
                        }
                        else
                        {
                            successMessage = "Order cancelled successfully. (No payment was processed)";
                        }
                    }
                    catch (InvalidOperationException e)
                    {
                        TempData["error"] = e.Message;
                        return RedirectToAction(nameof(Details), new { orderId = orderHeader.Id });
                    }
                    catch (StripeException e)
                    {
                        TempData["error"] = $"Order cancelled but refund failed: {e.Message}. Please process refund manually in Stripe Dashboard.";
                        return RedirectToAction("Details", new { orderId = orderHeader.Id });
                    }
                    break;
                case SD.StatusShipped:

                    if (string.IsNullOrEmpty(OrderHeader.Carrier) || string.IsNullOrEmpty(OrderHeader.TrackingNumber))
                    {
                        TempData["error"] = "Please provide both carrier and tracking number.";
                        return RedirectToAction(nameof(Details),  new { orderId = orderHeader.Id });
                    }
                    await _orderService.UpdateOrderStatusAsync(OrderHeader.Id, status, OrderHeader.Carrier, OrderHeader.TrackingNumber);
                    successMessage = "Order shipped successfully";
                    break;
                
                default:
                    successMessage = "Invalid status update.";
                    return RedirectToAction(nameof(Details),  new { orderId = orderHeader.Id });
            }
            
            TempData["Success"] = successMessage;
            
            return RedirectToAction(nameof(Details), new { orderId = OrderHeader.Id });
        }

        #region API CALLS
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(string status)
        {
            string? userId = null;

            if (!User.IsInRole(SD.RoleAdmin) && !User.IsInRole(SD.RoleEmployee))
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }
            }
            var orders = await _orderService.GetAllOrderAsync(userId, status, includeUser: true);
            return Json(new { data = orders });
        }
        
        #endregion
    }
}