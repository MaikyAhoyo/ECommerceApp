using ECommerce.Models.Entities;
using ECommerce.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // POST: api/orders
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Order order)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            order.OrderDate = DateTime.UtcNow;
            order.Status = "Cart";
            order.Total = 0;

            var orderId = await _orderService.CreateAsync(order);

            // Recalculate totals (include 16% IVA) after persisting
            var subtotal = await _orderService.CalculateOrderTotalAsync(orderId);
            var tax = Math.Round(subtotal * 0.16m, 2);
            var total = subtotal + tax;

            var createdOrder = await _orderService.GetByIdAsync(orderId);
            if (createdOrder != null)
            {
                createdOrder.Total = total;
                await _orderService.UpdateAsync(createdOrder);
            }

            var result = await _orderService.GetByIdAsync(orderId);
            return CreatedAtAction(nameof(GetById), new { id = orderId }, result);
        }

        // GET: api/orders/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _orderService.GetOrderWithDetailsAsync(id);

            if (order == null)
                return NotFound(new { message = "Order not found" });

            return Ok(order);
        }

        // GET: api/orders
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _orderService.GetAllAsync();
            return Ok(orders);
        }

        // GET: api/orders/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var orders = await _orderService.GetByUserIdAsync(userId);
            return Ok(orders);
        }

        // GET: api/orders/user/{userId}/cart
        [HttpGet("user/{userId}/cart")]
        public async Task<IActionResult> GetActiveCart(int userId)
        {
            var cart = await _orderService.GetActiveCartAsync(userId);

            if (cart == null)
                return NotFound(new { message = "No active cart found" });

            var cartWithDetails = await _orderService.GetOrderWithDetailsAsync(cart.Id);

            // Recalculate totals including tax for the returned cart
            var subtotal = await _orderService.CalculateOrderTotalAsync(cartWithDetails.Id);
            var tax = Math.Round(subtotal * 0.16m, 2);
            cartWithDetails.Total = subtotal + tax;

            return Ok(cartWithDetails);
        }

        // PUT: api/orders/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            if (string.IsNullOrEmpty(request.Status))
                return BadRequest(new { message = "Status is required" });

            var success = await _orderService.UpdateStatusAsync(id, request.Status);

            if (!success)
                return NotFound(new { message = "Order not found" });

            return Ok(new { message = "Order status updated successfully" });
        }

        // POST: api/orders/{id}/items
        [HttpPost("{id}/items")]
        public async Task<IActionResult> AddItem(int id, [FromBody] OrderItem item)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var success = await _orderService.AddItemToOrderAsync(id, item);

            if (!success)
                return BadRequest(new { message = "Failed to add item to order" });

            // Recalculate subtotal and include IVA
            var subtotal = await _orderService.CalculateOrderTotalAsync(id);
            var tax = Math.Round(subtotal * 0.16m, 2);
            var total = subtotal + tax;

            var order = await _orderService.GetByIdAsync(id);
            if (order != null)
            {
                order.Total = total;
                await _orderService.UpdateAsync(order);
            }

            return Ok(new { message = "Item added successfully", total = total, tax = tax });
        }

        // DELETE: api/orders/items/{itemId}
        [HttpDelete("items/{itemId}")]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            var success = await _orderService.RemoveItemFromOrderAsync(itemId);

            if (!success)
                return NotFound(new { message = "Order item not found" });

            return Ok(new { message = "Item removed successfully" });
        }

        // PATCH: api/orders/items/{itemId}/quantity
        [HttpPatch("items/{itemId}/quantity")]
        public async Task<IActionResult> UpdateItemQuantity(int itemId, [FromBody] UpdateQuantityRequest request)
        {
            if (request.Quantity <= 0)
                return BadRequest(new { message = "Quantity must be greater than 0" });

            var success = await _orderService.UpdateOrderItemQuantityAsync(itemId, request.Quantity);

            if (!success)
                return NotFound(new { message = "Order item not found" });

            // Optionally recalculate the parent order total here if you can get order id from item
            return Ok(new { message = "Quantity updated successfully" });
        }

        // GET: api/orders/{id}/items
        [HttpGet("{id}/items")]
        public async Task<IActionResult> GetItems(int id)
        {
            var items = await _orderService.GetOrderItemsAsync(id);
            return Ok(items);
        }

        // POST: api/orders/{id}/checkout
        [HttpPost("{id}/checkout")]
        public async Task<IActionResult> Checkout(int id)
        {
            // Recalculate total including IVA before checkout
            var subtotal = await _orderService.CalculateOrderTotalAsync(id);
            var tax = Math.Round(subtotal * 0.16m, 2);
            var order = await _orderService.GetByIdAsync(id);
            if (order != null)
            {
                order.Total = subtotal + tax;
                await _orderService.UpdateAsync(order);
            }

            var success = await _orderService.CheckoutOrderAsync(id);

            if (!success)
                return BadRequest(new { message = "Failed to checkout order" });

            return Ok(new { message = "Order checked out successfully" });
        }

        // DELETE: api/orders/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _orderService.DeleteAsync(id);

            if (!success)
                return NotFound(new { message = "Order not found" });

            return Ok(new { message = "Order deleted successfully" });
        }
    }

    // Request Models
    public class UpdateStatusRequest
    {
        public string Status { get; set; } = null!;
    }

    public class UpdateQuantityRequest
    {
        public int Quantity { get; set; }
    }
}