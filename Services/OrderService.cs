using Microsoft.EntityFrameworkCore;
using ProjectTemplate.Data;
using ProjectTemplate.Models.Ecommerce;

namespace ProjectTemplate.Services;

public interface IOrderService
{
    Task<Order> PlaceOrderAsync(CheckoutViewModel checkout, ShoppingCart cart);
    Task<Order?> GetByOrderNumberAsync(string orderNumber);
    Task<List<Order>> GetAllOrdersAsync(int page = 1, int pageSize = 50);
    Task UpdateStatusAsync(int orderId, OrderStatus status);
}

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;
    private readonly IEmailService _email;

    public OrderService(AppDbContext db, IEmailService email)
    {
        _db = db;
        _email = email;
    }

    public async Task<Order> PlaceOrderAsync(CheckoutViewModel checkout, ShoppingCart cart)
    {
        var orderNumber = GenerateOrderNumber();

        var order = new Order
        {
            OrderNumber   = orderNumber,
            CustomerName  = checkout.CustomerName.Trim(),
            CustomerEmail = checkout.CustomerEmail?.Trim(),
            CustomerPhone = checkout.CustomerPhone.Trim(),
            DeliveryAddress = checkout.DeliveryAddress?.Trim(),
            Notes         = checkout.Notes?.Trim(),
            PaymentMethod = checkout.PaymentMethod,
            SubTotal      = cart.SubTotal,
            DeliveryFee   = 0,
            TotalAmount   = cart.SubTotal,
            Status        = OrderStatus.Pending,
            CreatedAt     = DateTime.UtcNow,
            UpdatedAt     = DateTime.UtcNow,
            Items         = cart.Items.Select(i => new OrderItem
            {
                ProductId      = i.ProductId,
                ProductName    = i.ProductName,
                ProductImageUrl = i.ImageUrl,
                Quantity       = i.Quantity,
                UnitPrice      = i.UnitPrice,
                TotalPrice     = i.TotalPrice
            }).ToList()
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        // Send confirmation email if address provided
        if (!string.IsNullOrWhiteSpace(checkout.CustomerEmail))
        {
            var subject = $"Porosia #{orderNumber} u pranua — Enisi Center";
            var body = BuildConfirmationEmailBody(order);
            try
            {
                await _email.SendNoReplyEmailAsync(checkout.CustomerEmail, subject, body, isSq: true);
            }
            catch { /* Email errors should not fail the order */ }
        }

        return order;
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber) =>
        await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

    public async Task<List<Order>> GetAllOrdersAsync(int page = 1, int pageSize = 50) =>
        await _db.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task UpdateStatusAsync(int orderId, OrderStatus status)
    {
        var order = await _db.Orders.FindAsync(orderId);
        if (order != null)
        {
            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string GenerateOrderNumber()
    {
        var now = DateTime.UtcNow;
        return $"EC{now:yyMMdd}{now:HHmmss}{Random.Shared.Next(10, 99)}";
    }

    private static string BuildConfirmationEmailBody(Order order)
    {
        var itemsHtml = string.Join("", order.Items.Select(i =>
            $"<tr><td style='padding:8px;border-bottom:1px solid #eee'>{i.ProductName}</td>" +
            $"<td style='padding:8px;border-bottom:1px solid #eee;text-align:center'>{i.Quantity}</td>" +
            $"<td style='padding:8px;border-bottom:1px solid #eee;text-align:right'>{i.UnitPrice:F2} €</td>" +
            $"<td style='padding:8px;border-bottom:1px solid #eee;text-align:right'>{i.TotalPrice:F2} €</td></tr>"));

        return $@"
<div style='font-family:Inter,Arial,sans-serif;max-width:600px;margin:auto;background:#fff;border-radius:12px;overflow:hidden;border:1px solid #e5e7eb'>
  <div style='background:linear-gradient(135deg,#0a0f1e,#1a2340);padding:32px;text-align:center'>
    <h1 style='color:#f59e0b;margin:0;font-size:24px'>Enisi Center</h1>
    <p style='color:#cbd5e1;margin:8px 0 0'>Porosia juaj u pranua!</p>
  </div>
  <div style='padding:32px'>
    <h2 style='color:#1e293b'>Faleminderit, {order.CustomerName}! 🎉</h2>
    <p style='color:#64748b'>Porosia juaj me numër <strong style='color:#f59e0b'>#{order.OrderNumber}</strong> u pranua me sukses.</p>
    
    <table style='width:100%;border-collapse:collapse;margin:24px 0'>
      <thead>
        <tr style='background:#f8fafc'>
          <th style='padding:12px 8px;text-align:left;color:#374151'>Produkti</th>
          <th style='padding:12px 8px;text-align:center;color:#374151'>Sasia</th>
          <th style='padding:12px 8px;text-align:right;color:#374151'>Çmimi</th>
          <th style='padding:12px 8px;text-align:right;color:#374151'>Total</th>
        </tr>
      </thead>
      <tbody>{itemsHtml}</tbody>
    </table>
    
    <div style='background:#f8fafc;border-radius:8px;padding:16px;text-align:right'>
      <strong style='font-size:18px;color:#0a0f1e'>Total: {order.TotalAmount:F2} €</strong>
    </div>
    
    <div style='margin-top:24px;padding:16px;background:#fffbeb;border-radius:8px;border-left:4px solid #f59e0b'>
      <p style='margin:0;color:#92400e'><strong>Mënyra e pagesës:</strong> Pagesa me dorëzim</p>
      {(string.IsNullOrWhiteSpace(order.DeliveryAddress) ? "" : $"<p style='margin:8px 0 0;color:#92400e'><strong>Adresa:</strong> {order.DeliveryAddress}</p>")}
    </div>
    
    <p style='margin-top:24px;color:#64748b'>Do t'ju kontaktojmë shpejt për konfirmim. Faleminderit për besimin!</p>
  </div>
  <div style='background:#f8fafc;padding:16px;text-align:center'>
    <p style='color:#94a3b8;font-size:12px;margin:0'>© 2025 Enisi Center — Të gjitha të drejtat e rezervuara</p>
  </div>
</div>";
    }
}
