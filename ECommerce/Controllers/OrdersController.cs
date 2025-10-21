using ECommerce.Models.Entities;
using ECommerce.Models.ViewModels;
using ECommerce.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> Cart()
        {
            // Para demo, CartViewModel podría almacenar productos temporalmente
            return View(new CartViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Crear pedido simulado
            var order = new Order
            {
                UserId = model.UserId,
                OrderDate = DateTime.Now,
                Status = "Pending",
                Total = model.Total
            };

            await _orderService.CreateOrderAsync(order, model.Items, model.Payment, model.ShippingAddress);

            return RedirectToAction("Confirmation");
        }

        public IActionResult Confirmation()
        {
            return View();
        }
    }
}
