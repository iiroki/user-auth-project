using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserAuthServer.Models;
using UserAuthServer.Models.Dto;

namespace UserAuthServer.Controllers;

[ApiController]
[Route("user")]
public class UserController : ControllerBase {
    private readonly ILogger<UserController> Logger;
    private readonly UserManager<User> UserManager;
    private readonly RoleManager<IdentityRole> RoleManager;

    public UserController(
            ILogger<UserController> logger,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager) {
        this.Logger = logger;
        this.UserManager = userManager;
        this.RoleManager = roleManager;
    }

    /// <summary>
    ///     Get all users
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers() {
        this.Logger.LogDebug($"{nameof(GetUsers)}");
        return await this.UserManager.Users.Select(u => UserToDto(u)).ToListAsync();
    }

    /// <summary>
    ///     Get specific user
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUser(string id) {
        var user = await this.UserManager.FindByIdAsync(id);
        this.Logger.LogDebug($"{nameof(GetUser)} | ID: {id} - Found: {user != null}");
        if (user == null) {
            return NotFound();
        }

        return UserToDto(user);
    }

    /// <summary>
    ///     Create new user
    /// </summary>
    /// <remarks>
    ///     Username must be unique.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> CreateUser(UserCreateDto registration) {
        var existingUser = await this.UserManager.FindByNameAsync(registration.Username);
        if (existingUser != null) {
            return BadRequest();
        }

        var user = new User {
            UserName = registration.Username,
            Name = registration.Name
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
    [HttpPut("{id}")]
    [Authorize(Roles = UserRole.User)]
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
    ///     Delete existing user
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = UserRole.User)]
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

    private async Task<User?> FindUserByIdIfTokenMatches(string id) {
        var user = await this.UserManager.FindByIdAsync(id);
        if (user == null) {
            return null;
        }

        var httpUser = await this.UserManager.GetUserAsync(this.HttpContext.User);
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