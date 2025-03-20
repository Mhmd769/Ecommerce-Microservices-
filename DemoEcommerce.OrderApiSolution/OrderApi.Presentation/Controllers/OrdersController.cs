using eCommerce.SharedLibrary.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Application.DTOs;
using OrderApi.Application.DTOs.Conversions;
using OrderApi.Application.Interfaces;
using OrderApi.Application.Services;

namespace OrderApi.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController(IOrder orderInterface,IOrderService orderService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDTO>>> GetOrders()
        {
            var orders= await orderInterface.GetAllAsync();
            if(!orders.Any())
                return NotFound("No orders found.");
            var (_,list)=OrderConversions.FromEntity(null,orders);
            return !list!.Any() ? NotFound():Ok(list);
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderDTO>> GetOrder(int id)
        {
            var order= await orderInterface.FindByIdAsync(id);
            if (order is null)
                return NotFound(null);
            var (_order, _) = OrderConversions.FromEntity(order, null);
            return Ok(_order);   
        }

        [HttpGet("client/{clientId:int}")]
        public async Task<ActionResult<OrderDTO>> GetClientOrders(int clientId)
        {
            if (clientId <= 0) return BadRequest("Invalid data provided");

            var orders= await orderService.GetOrdersByCLientId(clientId);
            return !orders.Any()? NotFound(null) : Ok(orders);
        }

        [HttpGet("details/{orderId:int}")]
        public async Task<ActionResult<OrderDetailsDTO>>GetOrderDetails(int orderId)
        {
            if (orderId <= 0) return BadRequest("Invalid data provided");
            var orderDetails =await orderService.GetOrderDetails(orderId);
            return orderDetails.OrderId>0 ? Ok(orderDetails) : NotFound("No order Found");

        }

        [HttpPost]
        public async Task<ActionResult<Response>> CreateOrder(OrderDTO orderDTO)
        {
            //check model state if all data anotations are passed
            if (!ModelState.IsValid)
                return BadRequest("Incomplete data Submited");
            //covert to entity 
            var getEntity=OrderConversions.ToEntity(orderDTO);
            var response = await orderInterface.CreateAsync(getEntity);
            return response.Flag ? Ok(response) : BadRequest(response);
        }

        [HttpPut]
        public async Task<ActionResult<Response>> UpdateOrder(OrderDTO orderDTO)
        {
            //convert from dto to entity
            var order = OrderConversions.ToEntity(orderDTO);    
            var response= await orderInterface.UpdateAsync(order);
            return response.Flag ? Ok(response) : BadRequest(response);
        }
        [HttpDelete("delete/{orderId:int}")]
        public async Task<ActionResult<Response>> DeleteOrder(int orderId)
        {
            if (orderId <= 0)
                return BadRequest("Invalid Order ID");

            var order = await orderInterface.FindByIdAsync(orderId);
            if (order is null)
                return NotFound("Order not found");

            var response = await orderInterface.DeleteAsync(order);
            return response.Flag ? Ok(response) : BadRequest(response);
        }


    }
}
