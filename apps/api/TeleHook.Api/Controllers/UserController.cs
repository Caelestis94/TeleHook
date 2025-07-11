using Microsoft.AspNetCore.Mvc;
using TeleHook.Api.DTO;
using TeleHook.Api.Middleware.Attributes;
using TeleHook.Api.Models;
using TeleHook.Api.Services.Interfaces;

namespace TeleHook.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;

    public UserController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    [HttpGet]
    [RequireApiKey]
    [Route("setup-required")]
    public async Task<ActionResult<bool>> Get()
    {
        var setupIsRequired = await _userManagementService.SetupIsRequiredAsync();
        return Ok(setupIsRequired);
    }

    [HttpPost]
    [RequireApiKey]
    [Route("setup")]
    public async Task<ActionResult<User>> Post([FromBody] CreateUserDto createUserRequest)
    {
        var newUser = await _userManagementService.CreateAdminUserAsync(createUserRequest);
       
        return Created($"/api/user/{newUser.Id}", newUser);
    }
    
    [HttpPost]
    [RequireApiKey]
    [Route("signin")]
    public async Task<ActionResult<User>> UserSignIn([FromBody] EmailPasswordSignInDto signInRequest)
    {
        var user = await _userManagementService.SignInAsync(signInRequest);
        return Ok(user);
    }

    [HttpGet]
    [RequireApiKey]
    [Route("{id}")]
    public async Task<ActionResult<User>> Get(int id)
    {
        var user = await _userManagementService.GetUserByIdAsync(id);
        return Ok(user);
    }
    
    [HttpPut]
    [RequireApiKey]
    [Route("{id}")]
    public async Task<ActionResult<User>> Put(int id, UpdateUserDto updateUserRequest)
    {
        var updatedUser = await _userManagementService.UpdateUserAsync(updateUserRequest,id);
        return Ok(updatedUser);
    }

    [HttpPost]
    [RequireApiKey]
    [Route("oidc-signin")]
    public async Task<ActionResult<User>> OidcSignIn([FromBody] OidcSignInDto oidcSignInRequest)
    {
        var existingUser = await _userManagementService.OidcSignInAsync(oidcSignInRequest);
        return Ok(existingUser);
    }
}