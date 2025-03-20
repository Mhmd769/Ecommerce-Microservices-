using OrderApi.Application.DTOs;
using OrderApi.Application.DTOs.Conversions;
using OrderApi.Application.Interfaces;
using Polly;
using Polly.Registry;
using System.Net.Http.Json;
namespace OrderApi.Application.Services
{
    public class OrderService(IOrder orderInterface,HttpClient httpClient,ResiliencePipelineProvider<string> resiliencePipeline) : IOrderService
    {
        //GET PRODUCT
        public async Task<ProductDTO>GetProduct(int productId)
        {
            // call Product API using HttpClient
            //Redirect this call to the API GateWay since the product Api is not response to outsiders. 
            var getProduct = await httpClient.GetAsync($"api/products/{productId}");
            if(!getProduct.IsSuccessStatusCode)
            {
                return null!;
            }
            var product = await getProduct.Content.ReadFromJsonAsync<ProductDTO>();
            return product!;
        }

        // GET USER
        public async Task<AppUserDTO> GetUser(int userId)
        {
            // call Product API using HttpClient
            //Redirect this call to the API GateWay since the product Api is not response to outsiders. 
            var getUser = await httpClient.GetAsync($"api/authentication/{userId}");
            if (!getUser.IsSuccessStatusCode)
            {
                return null!;
            }
            var product = await getUser.Content.ReadFromJsonAsync<AppUserDTO>();
            return product!;
        }

        //GET ORDER DETAILS BY ID
        public async Task<OrderDetailsDTO> GetOrderDetails(int orderId)
        {
            //Prepare Order
            var order=await orderInterface.FindByIdAsync(orderId);
            if(order is null || order!.Id<=0)
                return null!;
            //Get Retry pipeline
            var retrypipeline = resiliencePipeline.GetPipeline("my-retry-pipeline");

            //prepare Product
            var  productDTO=await retrypipeline.ExecuteAsync(async token=>await GetProduct(order.ProductId));

            //prepare Client
            var appUserDTO= await retrypipeline.ExecuteAsync(async token=>await GetUser(order.ClientId));

            //Populate order Details
            return new OrderDetailsDTO(
                orderId,
                productDTO.Id,
                appUserDTO.Id,
                appUserDTO.Name,
                appUserDTO.Email,
                appUserDTO.Address,
                appUserDTO.TelephoneNumber,
                productDTO.Name,
                order.PurchaseQuantity,
                productDTO.Price,
                productDTO.Quantity * order.PurchaseQuantity,
                order.OrderedDate
                );
        }

        // GET ORDERS BY CLIENT ID
        public async Task<IEnumerable<OrderDTO>> GetOrdersByCLientId(int clientId)
        {
            // get all client orders
            var orders=await orderInterface.GetOrdersAsync(o=>o.ClientId==clientId);
            if (!orders.Any()) return null!;
            
            //convert from entity to dto 
            var( _,_orders)=OrderConversions.FromEntity(null, orders);
            return _orders!;
        }
    }
}
