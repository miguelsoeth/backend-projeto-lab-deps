using Application.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query;

namespace WebApi.Controllers;
[Route("api/[controller]")]
[ApiController]
//[Authorize(Roles = "Admin" )]
public class CreditsController : ControllerBase
{
    private readonly ICreditService _creditService;

    public CreditsController(ICreditService creditService)
    {
        _creditService = creditService;
    }


    [HttpPut("Increase/{userId}")]
    public async Task<ActionResult> IncreaseCredits(Guid userId, decimal amount)
    {
        if (amount < 0)
        {
            return BadRequest("Insira um valor maior que 0!");
        }
        var result = await _creditService.IncreaseCreditAsync(userId, amount);
        return Ok(result);
    }

    [HttpPut("decrease/{userId}")]
    public async Task<ActionResult> DecreaseCredits(Guid userId, decimal amount)
    {
        if (amount <= 0)
        {
            return BadRequest("Insira um valor maior que 0!");
        }
        var result = await _creditService.DecreaseCreditAsync(userId, amount);
        return Ok(result);
    }
    
}