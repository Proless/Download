#nullable enable
using Download.Test.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace Download.Test.Server.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FilesController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get([FromBody] TestConfig config)
        {

        }
    }
}
