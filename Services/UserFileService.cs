using UserAuthServer.Interfaces;

namespace UserAuthServer.Services;

public class UserFileService : IUserFileService {
    private readonly ILogger<UserFileService> Logger;
    private readonly string Folder;
    private readonly string[] AllowedExtensions;

    public UserFileService(ILogger<UserFileService> logger, IConfiguration config) {
        this.Logger = logger;
        this.Folder = Path.Combine(Directory.GetCurrentDirectory(), config["UserFile:Folder"]);
        this.AllowedExtensions = config.GetSection("UserFile:AllowedExtensions").Get<string[]>();

        // Init user file folder
        if (!Directory.Exists(this.Folder)) {
            this.Logger.LogInformation($"Creating user file folder: {this.Folder}");
            Directory.CreateDirectory(this.Folder);
        }
    }

    public string? GetUserFile(string id) {
        return null;
    }

    public string? AddUserFile(IFormFile file) {
        return "TODO";
    }

    public bool RemoveUserFile(string id) {
        return false;
    }
}