namespace UserAuthServer.Models;

public class UserFile {
    public string Id { get; set; } = null!;
    public User User { get; set; } = null!;
    public byte[] Data { get; set; } = null!;
    public string ContentType { get; set; } = null!;
}
