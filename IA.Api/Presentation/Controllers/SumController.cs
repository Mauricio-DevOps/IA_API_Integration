using IA.Api.Application.Contracts;
using IA.Api.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IA.Api.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SumController : ControllerBase
{
    private readonly ISumService _sumService;

    public SumController(ISumService sumService)
    {
        _sumService = sumService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(SumResult), StatusCodes.Status200OK)]
    public ActionResult<SumResult> Get([FromQuery] int firstValue, [FromQuery] int secondValue)
    {
        var result = _sumService.Sum(firstValue, secondValue);
        return Ok(result);
    }
}
