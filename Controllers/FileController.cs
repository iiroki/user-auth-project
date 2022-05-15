using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserAuthServer.Constants;
using UserAuthServer.Interfaces;
using UserAuthServer.Models;
using UserAuthServer.Models.Dto;
using UserAuthServer.Utils;

namespace UserAuthServer.Controllers;

[ApiController]
[Route("file")]
[Authorize(Roles = UserRole.User)] // [SECURE] Deny by default
public class FileController : ControllerBase {
    private readonly ILogger<FileController> Logger;
    private readonly IUserFileService UserFileService;
    private readonly UserManager<User> UserManager;

    public FileController(
            ILogger<FileController> logger,
            IUserFileService userFileService,
            UserManager<User> userManager) {
        this.Logger = logger;
        this.UserFileService = userFileService;
        this.UserManager = userManager;
    }

    /// <summary>
    ///     Get all file informations
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserFileInfoDto>>> GetFileInfos() {
        var files = await this.UserFileService.GetUserFiles();
        return Ok(files.Select(f => UserFileToDto(f)));
    }

    /// <summary>
    ///     Get specific file
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetFile(string id) {
        var file = await this.UserFileService.GetUserFile(id);
        if (file == null) {
            return NotFound();
        }
        return File(file.Data, file.ContentType);
    }

    /// <summary>
    ///     Add new file
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AddFile(IFormFile file) {
        var user = await RequestUtil.GetRequestUser(this.UserManager, this.HttpContext);
        var id = await this.UserFileService.AddUserFile(file, user!);
        return id != null ? Created(id, new UserFileInfoDto { Id = id, UserId = user!.Id }) : BadRequest();
    }

    /// <summary>
    ///     Delete file
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DeleteFile(string id) {
        var file = await this.UserFileService.GetUserFile(id);
        if (file == null) {
            return NotFound();
        }

        var user = await RequestUtil.GetRequestUser(this.UserManager, this.HttpContext);
        if (user == null || user.Id != file.User.Id) {
            return Forbid();
        }

        await this.UserFileService.RemoveUserFile(file.Id);
        return NoContent();
    }

    private static UserFileInfoDto UserFileToDto(UserFile file) => new UserFileInfoDto {
        Id = file.Id,
        UserId = file.User.Id
    };
}
