using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AsyncWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RemoteController : ControllerBase
    {
        public async Task<ActionResult> Get(int id)
        {
            await Task.Delay(100);

            return Ok(99);

            //requestCount++;
            //if (requestCount % 4 == 0)
            //{
            //    return Ok(requestCount);
            //}
            //return StatusCode((int)HttpStatusCode.InternalServerError, "Something Went Wrong");
        }

    }
}
