using Microsoft.EntityFrameworkCore;
using UserAuthServer.Interfaces;
using UserAuthServer.Models;

namespace UserAuthServer.Services;

public class UserFileService : IUserFileService {
    private static int MAX_FILE_SIZE = 500000; // Bytes
    private readonly ILogger<UserFileService> Logger;
    private readonly UserAuthServerDbContext DbContext;
    private readonly string Folder;
    private readonly string[] AllowedExtensions;
    private readonly Func<int> SaveDb;

    public UserFileService(
            ILogger<UserFileService> logger,
            UserAuthServerDbContext dbContext,
            IConfiguration config) {
        this.Logger = logger;
        this.DbContext = dbContext;
        this.SaveDb = dbContext.SaveChanges;
        this.Folder = Path.Combine(Directory.GetCurrentDirectory(), config["UserFile:Folder"]);
        this.AllowedExtensions = config.GetSection("UserFile:AllowedExtensions").Get<string[]>();

        // Init user file folder
        if (!Directory.Exists(this.Folder)) {
            this.Logger.LogInformation($"Creating user file folder: {this.Folder}");
            Directory.CreateDirectory(this.Folder);
        }
    }

    public async Task<IEnumerable<UserFile>> GetUserFiles() {
        return await this.DbContext.UserFiles.ToListAsync();
    }

    public async Task<UserFile?> GetUserFile(string id) {
        return await this.DbContext.UserFiles.FindAsync(id);
    }

    public async Task<string?> AddUserFile(IFormFile file, User user) {
        var extension = Path.GetExtension(file.FileName);
        if (!this.AllowedExtensions.Contains(extension) || file.Length > MAX_FILE_SIZE) {
            return null;
        }

        var stream = new MemoryStream();
        await file.CopyToAsync(stream);
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
        return false;
    }
}