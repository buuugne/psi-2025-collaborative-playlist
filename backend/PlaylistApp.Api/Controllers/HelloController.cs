using Microsoft.AspNetCore.Mvc; //need this so we could inherit from ControllerBase

namespace MyApi.Controllers
{
    [ApiController] //need this in controllers, it validates requests, throws error responses and other stuff
    [Route("api/[controller]")] //defines url path [controller] means the file name minus "controller"
    public class HelloController : ControllerBase //: means that it inherits from ControllerBase
    {
        [HttpGet] //responds only to get requests (like GetHello())
        public IActionResult GetHello()
        {
            return Ok("Hello from your first custom API endpoint!"); //function from ControllerBase. Returns 200 http response + the message
        }
    }
}
