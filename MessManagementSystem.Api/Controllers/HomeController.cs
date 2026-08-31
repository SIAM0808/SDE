using Microsoft.AspNetCore.Mvc;
namespace MessManagementSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HomeController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Mess World!");
    }
}