using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ECommerce.Controllers
{
    public class DatabaseController : Controller
    {
        private readonly IConfiguration _configuration;

        public DatabaseController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> TestConnection()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    return Content("Conexión exitosa a la base de datos: " + connection.Database);
                }
            }
            catch (Exception ex)
            {
                return Content("Error al conectar a la base de datos:\n" + ex.Message);
            }
        }
    }
}
