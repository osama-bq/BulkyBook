using BulkyBook.DataAccess.Data;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;

namespace BulkyBook.Business.Services.IServices;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;

    public OrderService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<OrderHeader> CreateOrderAsync(OrderHeader orderHeader)
    {
        _context.OrderHeaders.Add(orderHeader);
        await _context.SaveChangesAsync();
        
        return orderHeader;
    }

    public async Task<OrderHeader?> GerOrderByIdAsync(int id, bool includeUser = false, bool includeDetails = false)
    {
        var query = _context.OrderHeaders.AsQueryable();
        if (includeUser)
        {
            query = query.Include(u => u.ApplicationUser);
        }

        if (includeDetails)
        {
            query = query.Include(u => u.OrderDetails).ThenInclude(u => u.Product);
        }

        return await query.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<IEnumerable<OrderHeader>> GetAllOrderAsync(string? userId = null, string? status = null, bool includeUser = false,
        bool includeDetails = false)
    {
        var query = _context.OrderHeaders.AsQueryable();
        if (includeUser)
        {
            query = query.Include(u => u.ApplicationUser);
        }

        if (includeDetails)
        {
            query = query.Include(u => u.OrderDetails).ThenInclude(u => u.Product);
        }

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(u => u.ApplicationUserId == userId);
        }
        
        if (!string.IsNullOrEmpty(status) && status.ToLower() != "all")
        {
            query = query.Where(u => u.OrderStatus.ToLower() == status.ToLower());
        }

        return await query.ToListAsync();
    }

    public async Task UpdateOrderAsync(OrderHeader orderHeader)
    {
        _context.OrderHeaders.Update(orderHeader);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateOrderStatusAsync(int id, string orderStatus, string? carrier = null, string? trackingNumber = null)
    {
        var order = await _context.OrderHeaders.FindAsync(id);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order {id} not found");
        }
        
        order.OrderStatus = orderStatus;
        if (orderStatus == SD.StatusShipped) {
            order.ShippingDate = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(carrier))
            {
                order.Carrier = carrier;
            }

            if (!string.IsNullOrEmpty(trackingNumber))
            {
                order.TrackingNumber = trackingNumber;
            }
        }
        
        await _context.SaveChangesAsync();
    }

    public async Task UpdateStripePaymentAsync(int orderId, string sessionId, string paymentIntentId)
    {
        var order = await _context.OrderHeaders.FindAsync(orderId);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order {orderId} not found");
        }

        if (!string.IsNullOrEmpty(sessionId))
        {
            order.SessionId = sessionId;
        }
        
        if (!string.IsNullOrEmpty(paymentIntentId))
        {
            order.PaymentIntentId = paymentIntentId;
        }
        
        await _context.SaveChangesAsync();
    }

    public Task<bool> CancelOrderWithRefundAsync(int orderId)
    {
        throw new NotImplementedException();
    }

    public async Task<string> CreateStripeCheckoutSessionAsync(OrderHeader orderHeader, IEnumerable<ShoppingCart> cartItems, string domain)
    {
        if (orderHeader == null)
        {
            throw new ArgumentNullException(nameof(orderHeader));
        }

        if (cartItems == null || !cartItems.Any())
        {
            throw new ArgumentException("Cart items cannot be null or empty", nameof(cartItems));
        }
        
        var options = new Stripe.Checkout.SessionCreateOptions
        {
            SuccessUrl = domain + $"/customer/cart/OrderConfirmation?id={orderHeader.Id}",
            CancelUrl = domain + "/customer/cart/index",
            LineItems = new List<SessionLineItemOptions>(),
            Mode = "payment",
            Metadata = new Dictionary<string, string>
            {
                { "OrderId", orderHeader.Id.ToString() },
            }
        };

        foreach (var item in cartItems)
        {
            var sessionLineItem = new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(item.Price * 100),
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.Product.Title
                    }
                },
                Quantity = item.Count,
            };
            options.LineItems.Add(sessionLineItem);
        }
        var service = new SessionService();
        Session session = service.Create(options);
        await UpdateStripePaymentAsync(orderHeader.Id, session.Id, session.PaymentIntentId);
        return session.Url;
    }

    public async Task<bool> VerifyStripePaymentAsync(OrderHeader orderHeader)
    {
        var service = new SessionService();
        Session session = service.Get(orderHeader.SessionId);
        if (session.PaymentStatus.ToLower() == "paid")
        {
            await UpdateStripePaymentAsync(orderHeader.Id, session.Id, session.PaymentIntentId);
            await UpdateOrderStatusAsync(orderHeader.Id, SD.StatusApproved);
            return true;
        }
        else
        {
            return false;
        }
    }
}