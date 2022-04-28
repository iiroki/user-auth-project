using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserAuthServer.Constants;
using UserAuthServer.Initialization;
using UserAuthServer.Models;
using UserAuthServer.Models.Dto;
using UserAuthServer.Utils;

namespace UserAuthServer.Controllers;

[ApiController]
[Route("user")]
[Authorize(Roles = UserRole.User)] // [SECURE] Deny by default
public class UserController : ControllerBase {
    private readonly ILogger<UserController> Logger;
    private readonly UserManager<User> UserManager;
    private readonly RoleManager<IdentityRole> RoleManager;
    private readonly IConfiguration Config;
    private readonly bool DevelopmentEnv;

    public UserController(
            ILogger<UserController> logger,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration config,
            IHostEnvironment hostingEnv) {
        this.Logger = logger;
        this.UserManager = userManager;
        this.RoleManager = roleManager;
        this.Config = config;
        this.DevelopmentEnv = hostingEnv.IsDevelopment();
    }

    /// <summary>
    ///     Get all users
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers() {
        return await this.UserManager.Users.Select(u => UserToDto(u)).ToListAsync();
    }

    /// <summary>
    ///     Get specific user
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUser(string id) {
        var user = await this.UserManager.FindByIdAsync(id);
        if (user == null) {
            return NotFound();
        }

        return UserToDto(user);
    }

    /// <summary>
    ///     Create new user
    /// </summary>
    /// <remarks>
    ///     Username and email must be unique.
    /// </remarks>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> CreateUser(UserCreateDto registration) {
        var existingUser = await this.UserManager.FindByNameAsync(registration.Username);
        if (existingUser != null) {
            return BadRequest(ResponseUtil.CreateProblemDetails("User already exists"));
        }

        var user = new User {
            UserName = registration.Username,
            Name = registration.Name,
            Email = registration.Email
        };

        var result = await this.UserManager.CreateAsync(user, registration.Password);
        if (!result.Succeeded) {
            return BadRequest();
        }

        await this.UserManager.AddToRoleAsync(user, UserRole.User);
        this.Logger.LogDebug($"{nameof(CreateUser)} | Created user with ID: {user.Id}");
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, UserToDto(user));
    }

    /// <summary>
    ///     Update existing user
    /// </summary>
    /// <remarks>
    ///     Password is changed if a new password is provided and the current password is correct.
    /// </remarks>
    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateUser(string id, UserUpdateDto userUpdate) {
        var user = await FindUserByIdIfTokenMatches(id);
        if (user == null) {
            return Forbid();
        }

        // Request JWT matches request ID
        if (!(await this.UserManager.CheckPasswordAsync(user, userUpdate.CurrentPassword))) {
            return Unauthorized();
        }

        // Update user information
        user.Name = userUpdate.Name;
        await this.UserManager.UpdateAsync(user);

        // Update password if new password is provided
        if (userUpdate.NewPassword != null) {
            await this.UserManager.ChangePasswordAsync(user, userUpdate.CurrentPassword, userUpdate.NewPassword);
        }

        this.Logger.LogDebug($"{nameof(UpdateUser)} | Updated user with ID: {id}");
        return NoContent();
    }

    /// <summary>
    ///     Update roles of existing user
    /// </summary>
    [HttpPatch("{id}/role")]
    [Authorize(Roles = UserRole.Admin)] // [SECURE] Require admin privileges
    public async Task<IActionResult> UpdateUserRoles(string id, RoleUpdateDto roleUpdate) {
        var user = await FindUserById(id);
        if (user == null) {
            return NotFound();
        }

        // Validate roles
        var newRoles = roleUpdate.Roles;
        var existingRoles = UserRole.GetAll();
        foreach (var role in newRoles) {
            if (!existingRoles.Contains(role)) {
                return BadRequest(ResponseUtil.CreateProblemDetails($"Invalid role: {role}"));
            }
        }

        // Update roles
        foreach (var existingRole in existingRoles) {
            if (!newRoles.Contains(existingRole) && await this.UserManager.IsInRoleAsync(user, existingRole)) {
                await this.UserManager.RemoveFromRoleAsync(user, existingRole);
            }
        }

        foreach (var newRole in newRoles) {
            if (!await this.UserManager.IsInRoleAsync(user, newRole)) {
                await this.UserManager.AddToRoleAsync(user, newRole);
            }
        }

        this.Logger.LogDebug($"{nameof(UpdateUserRoles)} | User ID: {id}, New roles: {String.Join(", ", newRoles)}");
        return NoContent();
    }

    /// <summary>
    ///     Delete existing user
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteUser(string id, UserDeleteDto userDelete) {
        var user = await FindUserByIdIfTokenMatches(id);
        if (user == null) {
            return Forbid();
        }

        // Request JWT matches request ID
        if (!(await this.UserManager.CheckPasswordAsync(user, userDelete.CurrentPassword))) {
            return Unauthorized();
        }

        // Delete the user
        await this.UserManager.DeleteAsync(user);
        this.Logger.LogDebug($"{nameof(DeleteUser)} | Deleted user with ID: {id}");
        return NoContent();
    }

    /// <summary>
    ///     Reset users (DEVELOPMENT ONLY)
    /// </summary>
    /// <remarks>
    ///     Clears user database and seeds new power user.
    /// </remarks>
    [HttpPost("reset")]
    [Authorize(Roles = UserRole.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reset() {
        if (!this.DevelopmentEnv) {
            return NotFound();
        }

        this.Logger.LogDebug(nameof(Reset));
        if (await RequestUtil.GetRequestUser(this.UserManager, this.HttpContext) == null) {
            return Unauthorized();
        }

        await this.UserManager.Users.ForEachAsync(async u => {
            await this.UserManager.DeleteAsync(u);
        });

        await UserInitializer.seedUsers(this.UserManager, this.Config);
        return NoContent();
    }

    private async Task<User?> FindUserById(string id) {
        return await this.UserManager.FindByIdAsync(id);
    }

    private async Task<User?> FindUserByIdIfTokenMatches(string id) {
        var user = await FindUserById(id);
        if (user == null) {
            return null;
        }

        var httpUser = await RequestUtil.GetRequestUser(this.UserManager, this.HttpContext);
        if (httpUser == null) {
            return null;
        }

        return user.Id.Equals(httpUser.Id) ? user : null;
    }

    private static UserDto UserToDto(User user) => new UserDto {
        Id = user.Id,
        Name = user.Name
    };
}
