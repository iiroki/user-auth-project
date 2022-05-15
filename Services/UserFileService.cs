using Microsoft.EntityFrameworkCore;
using UserAuthServer.Interfaces;
using UserAuthServer.Models;

namespace UserAuthServer.Services;

public class UserFileService : IUserFileService {
    private static int MAX_FILE_SIZE = 5 * 1000 * 1000; // 5 MB
    private readonly ILogger<UserFileService> Logger;
    private readonly UserAuthServerDbContext DbContext;
    private readonly string[] AllowedExtensions;

    public UserFileService(
            ILogger<UserFileService> logger,
            UserAuthServerDbContext dbContext,
            IConfiguration config) {
        this.Logger = logger;
        this.DbContext = dbContext;
        this.AllowedExtensions = config.GetSection("UserFile:AllowedExtensions").Get<string[]>();
    }

    public async Task<IEnumerable<UserFile>> GetUserFiles() {
        return await this.DbContext.UserFiles.ToListAsync();
    }

    public async Task<UserFile?> GetUserFile(string id) {
        return await this.DbContext.UserFiles.FindAsync(id);
    }

    public async Task<string?> AddUserFile(IFormFile file, User user) {
        var extension = Path.GetExtension(file.FileName);
        // [SECURE] Check allowed extensions and file size
        if (!this.AllowedExtensions.Contains(extension) || file.Length > MAX_FILE_SIZE) {
            return null;
        }

        var stream = new MemoryStream();
        await file.CopyToAsync(stream);

        // [SECURE] Generate random file name
        var userFile = new UserFile() {
            Id = Guid.NewGuid().ToString(),
            User = user,
            Data = stream.ToArray(),
            ContentType = file.ContentType
        };

        await this.DbContext.UserFiles.AddAsync(userFile);
        await this.DbContext.SaveChangesAsync();
        return userFile.Id;
    }

    public async Task<bool> RemoveUserFile(string id) {
        var file = await this.DbContext.UserFiles.FindAsync(id);
        if (file == null) {
            return false;
        }

        this.DbContext.UserFiles.Remove(file);
        return true;
    }
}