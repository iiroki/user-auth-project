namespace UserAuthServer.Interfaces;

public interface ISessionService {
    void invalidateSessions(string userId);
}
