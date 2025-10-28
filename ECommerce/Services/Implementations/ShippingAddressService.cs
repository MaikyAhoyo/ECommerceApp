using Dapper;
using ECommerce.Data;
using ECommerce.Models.Entities;
using ECommerce.Services.Interfaces;

namespace ECommerce.Services.Implementations
{
    public class ShippingAddressService : IShippingAddressService
    {
        private readonly DapperContext _context;

        public ShippingAddressService(DapperContext context)
        {
            _context = context;
        }

        public async Task<int> CreateAsync(ShippingAddress address)
        {
            using var connection = _context.CreateConnection();
            var sql = @"INSERT INTO ShippingAddresses (UserId, AddressLine1, AddressLine2, City, State, Country, PostalCode)
                        VALUES (@UserId, @AddressLine1, @AddressLine2, @City, @State, @Country, @PostalCode);
                        SELECT CAST(SCOPE_IDENTITY() as int)";
            return await connection.ExecuteScalarAsync<int>(sql, address);
        }

        public async Task<ShippingAddress?> GetByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT * FROM ShippingAddresses WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<ShippingAddress>(sql, new { Id = id });
        }

        public async Task<IEnumerable<ShippingAddress>> GetByUserIdAsync(int userId)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT * FROM ShippingAddresses WHERE UserId = @UserId";
            return await connection.QueryAsync<ShippingAddress>(sql, new { UserId = userId });
        }

        public async Task<bool> UpdateAsync(ShippingAddress address)
        {
            using var connection = _context.CreateConnection();
            var sql = @"UPDATE ShippingAddresses 
                        SET AddressLine1 = @AddressLine1, AddressLine2 = @AddressLine2, 
                            City = @City, State = @State, Country = @Country, PostalCode = @PostalCode
                        WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, address);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var sql = "DELETE FROM ShippingAddresses WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }
    }
}