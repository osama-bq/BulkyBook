using BulkyBook.Models;

namespace BulkyBook.Business.Services.IServices;

public interface IOrderService
{
    Task<OrderHeader> CreateOrderAsync(OrderHeader orderHeader);
    Task<OrderHeader?> GerOrderByIdAsync(int id, bool includeUser = false, bool includeDetails = false);
    Task<IEnumerable<OrderHeader>> GerAllOrderAsync(string? userId, string? status = null, bool includeUser = false, bool includeDetails = false);
}