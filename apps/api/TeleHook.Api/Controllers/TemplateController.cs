using Microsoft.AspNetCore.Mvc;
using TeleHook.Api.DTO;
using TeleHook.Api.Middleware.Attributes;
using TeleHook.Api.Services.Interfaces;

namespace TeleHook.Api.Controllers;

[ApiController]
[Route("api/templates")]
public class TemplateController : ControllerBase
{
    private readonly ITemplateService _templateService;

    public TemplateController(ITemplateService templateService)
    {
        _templateService = templateService;
    }

    [HttpPost]
    [RequireApiKey]
    [Route("render")]
    public ActionResult<RenderTemplateResponse> RenderTemplate([FromBody] RenderTemplateDto renderTemplateRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = _templateService.RenderTemplate(renderTemplateRequest.Template, renderTemplateRequest.SampleData);

        return Ok(result);
    }
}
