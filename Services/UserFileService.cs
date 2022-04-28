using UserAuthServer.Interfaces;
using UserAuthServer.Models;

namespace UserAuthServer.Services;

public class UserFileService : IUserFileService {
    private readonly ILogger<UserFileService> Logger;

    public UserFileService(ILogger<UserFileService> logger) {
      this.Logger = logger;
    }

    public string? GetUserFile(string id) {
        return null;
    }

    public string AddUserFile(int file) {
        return "TODO";
    }

    public bool RemoveUserFile(string id) {
        return false;
    }
}