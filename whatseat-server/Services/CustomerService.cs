using Microsoft.EntityFrameworkCore;
using whatseat_server.Data;
using whatseat_server.Models;

namespace whatseat_server.Services;

public class CustomerService
{
    private readonly WhatsEatContext _context;
    public CustomerService(WhatsEatContext context)
    {
        _context = context;
    }

    public async Task<Customer> FindCustomerByIdAsync(Guid userId)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == userId);
        return customer;
    }

    public async Task<List<ShippingInfo>> GetCustomerShippingInfos(Customer customer)
    {
        return await _context.ShippingInfos.AsNoTracking().Where(s => (s.Customer == customer && s.Status == true)).ToListAsync();
    }

    public async Task<ShippingInfo> GetCustomerShippingInfosById(Customer customer, int shippingId)
    {
        return await _context.ShippingInfos.FirstOrDefaultAsync(s => s.Customer == customer && s.Status == true);
    }
}