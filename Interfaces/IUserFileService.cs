using UserAuthServer.Models;

namespace UserAuthServer.Interfaces;

public interface IUserFileService {
    string? GetUserFile(string id);

    string AddUserFile(int file);

    bool RemoveUserFile(string id);
}
