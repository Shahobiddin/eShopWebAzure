using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eShopWebRestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VaultController : ControllerBase
    {
        public VaultController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        [HttpGet("sqldb")]
        public ActionResult GetConnectionStringSQlDb()
        {
            return Ok(Configuration["ConnectionStringSqlDb"]);
        }
    }
}
