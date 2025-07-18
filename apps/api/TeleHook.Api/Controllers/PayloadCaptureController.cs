using Microsoft.AspNetCore.Mvc;
using TeleHook.Api.DTO;
using TeleHook.Api.Middleware.Attributes;
using TeleHook.Api.Services.Interfaces;

namespace TeleHook.Api.Controllers;

[ApiController]
[Route("api/payload")]
public class PayloadCaptureController : ControllerBase
{
    private readonly IPayloadCaptureManagementService _payloadCaptureManagementService;

    public PayloadCaptureController(IPayloadCaptureManagementService payloadCaptureManagementService)
    {
        _payloadCaptureManagementService = payloadCaptureManagementService;
    }

    [HttpPost]
    [RequireApiKey]
    [Route("capture/start")]
    public async Task<ActionResult<CaptureSessionDto>> StartCapture([FromBody] StartCaptureSessionDto startCaptureRequest)
    {
        var session = await _payloadCaptureManagementService.CreateSessionAsync(startCaptureRequest.UserId);
        return Ok(session);
    }

    [HttpGet]
    [RequireApiKey]
    [Route("capture/status/{sessionId}")]
    public async Task<ActionResult<CaptureSessionDto>> GetStatus(string sessionId)
    {
        var status = await _payloadCaptureManagementService.GetStatusAsync(sessionId);
        return Ok(status);
    }

    [HttpGet]
    [RequireApiKey]
    [Route("capture/cancel/{sessionId}")]
    public async Task<ActionResult<CaptureSessionDto>> CancelCapture(string sessionId)
    {
        var result = await _payloadCaptureManagementService.CancelSessionAsync(sessionId);
        return Ok(result);
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("capture/{sessionId}")]
    public async Task<ActionResult> ReceivePayload(string sessionId, [FromBody] object payload)
    {
        var result = await _payloadCaptureManagementService.CompleteSessionAsync(sessionId, payload);

        if (result)
        {
            return Ok(new { message = "Payload captured successfully" });
        }

        return BadRequest(new { message = "Failed to capture payload" });
    }

}
