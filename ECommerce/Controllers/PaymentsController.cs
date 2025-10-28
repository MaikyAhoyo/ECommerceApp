using Microsoft.AspNetCore.Mvc;
using ECommerce.Models.Entities;
using ECommerce.Services.Interfaces;

namespace ECommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // POST: api/payments
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Payment payment)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            payment.PaymentDate = DateTime.UtcNow;
            payment.Status = "Pending";

            var paymentId = await _paymentService.CreateAsync(payment);
            var createdPayment = await _paymentService.GetByIdAsync(paymentId);

            return CreatedAtAction(nameof(GetById), new { id = paymentId }, createdPayment);
        }

        // POST: api/payments/process
        [HttpPost("process")]
        public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request.Amount <= 0)
                return BadRequest(new { message = "Amount must be greater than 0" });

            var success = await _paymentService.ProcessPaymentAsync(
                request.OrderId,
                request.Method,
                request.Amount
            );

            if (!success)
                return BadRequest(new { message = "Payment processing failed" });

            return Ok(new { message = "Payment processed successfully" });
        }

        // GET: api/payments/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var payment = await _paymentService.GetByIdAsync(id);

            if (payment == null)
                return NotFound(new { message = "Payment not found" });

            return Ok(payment);
        }

        // GET: api/payments/order/{orderId}
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetByOrder(int orderId)
        {
            var payment = await _paymentService.GetByOrderIdAsync(orderId);

            if (payment == null)
                return NotFound(new { message = "Payment not found for this order" });

            return Ok(payment);
        }

        // GET: api/payments
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var payments = await _paymentService.GetAllAsync();
            return Ok(payments);
        }

        // PUT: api/payments/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdatePaymentStatusRequest request)
        {
            if (string.IsNullOrEmpty(request.Status))
                return BadRequest(new { message = "Status is required" });

            var success = await _paymentService.UpdateStatusAsync(id, request.Status);

            if (!success)
                return NotFound(new { message = "Payment not found" });

            return Ok(new { message = "Payment status updated successfully" });
        }

        // PUT: api/payments/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Payment payment)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            payment.Id = id;
            var success = await _paymentService.UpdateAsync(payment);

            if (!success)
                return NotFound(new { message = "Payment not found" });

            return Ok(new { message = "Payment updated successfully" });
        }

        // DELETE: api/payments/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _paymentService.DeleteAsync(id);

            if (!success)
                return NotFound(new { message = "Payment not found" });

            return Ok(new { message = "Payment deleted successfully" });
        }
    }

    public class ProcessPaymentRequest
    {
        public int OrderId { get; set; }
        public string Method { get; set; } = null!;
        public decimal Amount { get; set; }
    }

    public class UpdatePaymentStatusRequest
    {
        public string Status { get; set; } = null!;
    }
}