using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Rafiq.API.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class BaseV1Controller : ControllerBase
{
}
