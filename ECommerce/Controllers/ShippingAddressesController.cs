using Microsoft.AspNetCore.Mvc;
using ECommerce.Models.Entities;
using ECommerce.Services.Interfaces;

namespace ECommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShippingAddressesController : ControllerBase
    {
        private readonly IShippingAddressService _shippingAddressService;

        public ShippingAddressesController(IShippingAddressService shippingAddressService)
        {
            _shippingAddressService = shippingAddressService;
        }

        // POST: api/shippingaddresses
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ShippingAddress address)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var addressId = await _shippingAddressService.CreateAsync(address);
            var createdAddress = await _shippingAddressService.GetByIdAsync(addressId);

            return CreatedAtAction(nameof(GetById), new { id = addressId }, createdAddress);
        }

        // GET: api/shippingaddresses/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var address = await _shippingAddressService.GetByIdAsync(id);

            if (address == null)
                return NotFound(new { message = "Shipping address not found" });

            return Ok(address);
        }

        // GET: api/shippingaddresses/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var addresses = await _shippingAddressService.GetByUserIdAsync(userId);
            return Ok(addresses);
        }

        // PUT: api/shippingaddresses/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ShippingAddress address)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            address.Id = id;
            var success = await _shippingAddressService.UpdateAsync(address);

            if (!success)
                return NotFound(new { message = "Shipping address not found" });

            return Ok(new { message = "Shipping address updated successfully" });
        }

        // DELETE: api/shippingaddresses/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _shippingAddressService.DeleteAsync(id);

            if (!success)
                return NotFound(new { message = "Shipping address not found" });

            return Ok(new { message = "Shipping address deleted successfully" });
        }
    }
}