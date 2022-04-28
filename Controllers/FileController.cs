using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserAuthServer.Constants;
using UserAuthServer.Models;
using UserAuthServer.Models.Dto;

namespace UserAuthServer.Controllers;

[ApiController]
[Route("file")]
[Authorize(Roles = UserRole.User)] // [SECURE] Deny by default
public class FileController : ControllerBase {
    private readonly ILogger<FileController> Logger;
    private readonly DbSet<UserFileInfo> FileContext;

    public FileController(ILogger<FileController> logger, UserAuthServerDbContext dbContext) {
        this.Logger = logger;
        this.FileContext = dbContext.UserFileInfos;
    }

    /// <summary>
    ///     Get all file informations
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserFileInfoDto>>> GetFileInfos() {
        return new List<UserFileInfoDto>{};
    }

    /// <summary>
    ///     Get specific file
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetFile(string id) {
        return NotFound();
    }
}