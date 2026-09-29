using BuildingSoftwareWithLLMs.Application.Abstractions.LLM;
using Microsoft.AspNetCore.Mvc;

namespace BuildingSoftwareWithLLMs.Api.Controllers;

[ApiController]
[Route("api/tools")]
public sealed class ToolsController : ControllerBase
{
    private readonly ILlmToolRegistry _toolRegistry;

    public ToolsController(ILlmToolRegistry toolRegistry)
    {
        ArgumentNullException.ThrowIfNull(toolRegistry);
        _toolRegistry = toolRegistry;
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<LlmToolDefinition>> Get()
    {
        return Ok(_toolRegistry.Definitions);
    }
}
