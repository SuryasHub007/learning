using Microsoft.AspNetCore.Mvc;

namespace Backend.API.Controllers;

/// <summary>Base class for REST controllers, which are served under /api/{controller}.</summary>
[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
}