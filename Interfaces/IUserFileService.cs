namespace UserAuthServer.Interfaces;

public interface IUserFileService {
    string? GetUserFile(string id);

    string? AddUserFile(IFormFile file);

    bool RemoveUserFile(string id);
}
