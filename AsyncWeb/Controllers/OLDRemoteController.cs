//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Linq;
//using System.Net;
//using System.Threading.Tasks;
//using System.Web.Http;

//namespace AsyncWeb.Controllers
//{
//    [Produces("application/json")]
//    [Route("api/remote")]
//    public class RemoteController : ApiController
//    {
//        static int requestCount = 0;
//        private readonly ILogger<RemoteController> _logger;
//        protected RemoteController(ILogger<RemoteController> logger)
//        {
//            _logger = logger;
//        }

//        [System.Web.Http.HttpGet("{id}")]
//        public async Task<ActionResult> Get(int id)
//        {
//            await Task.Delay(100);
//            requestCount++;

//            if (requestCount % 4 == 0)
//            {
//                return Ok(requestCount);
//            }
//            return StatusCode((int)HttpStatusCode.InternalServerError, "Something Went Wrong");
//        }
//    }
//}
