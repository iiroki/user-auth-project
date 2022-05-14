using UserAuthServer.Models;
namespace UserAuthServer.Interfaces;

public interface IUserFileService {
    Task<IEnumerable<UserFile>> GetUserFiles();
    Task<UserFile?> GetUserFile(string id);
    Task<string?> AddUserFile(IFormFile file, User user);
    Task<bool> RemoveUserFile(string id);
}
