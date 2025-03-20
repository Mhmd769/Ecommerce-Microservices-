using OrderApi.Application.DTOs;
namespace OrderApi.Application.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDTO>> GetOrdersByCLientId(int clientId);
        Task<OrderDetailsDTO> GetOrderDetails(int orderId);
    }
}
