# User Auth Project
**COMP.SEC.300 Secure Programming: Project**

- **Server:** C# & ASP.NET Core
- **Client:** TypeScript & React (?)

## Project
Main goal of this project is to implement the listed features using secure programming principles.

Features:
- User creation
- User authentication
- Role-based authorization
- User information management
- User role management
- User file uploads

Secure implementations are reviewed in [Security aspects](#security-aspects) chapter.

## Server API
TODO

## Security aspects
### User authentication
References:
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
- [OWASP Password Storage Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html)

OWASP states that passwords shorter than 8 characters are considered weak, so it is the minimum password length requirement when creating/updating an user. ASP.NET Core `UserManager` uses PBKDF2 password hashing algorithm by default. OWASP suggests that [bcrypt](https://en.wikipedia.org/wiki/Bcrypt) would be a better alternative, so I ended up changing `UserManager`'s password hashing algorithm to bcrypt by implementing `BCryptPasswordHasher`. OWASP also states that the bcrypt work factor should be at least 10, so the password hasher uses 16.

This project used JWTs (JSON Web Tokens) for user authentication. Successful login grants the user a refresh token (expires in 3 h) that can then be used to get an access token (expires in 5 min) that can be used to access the API. Refresh tokens can't be used to access API resources.

TODO: Email confirmation...

### Role-based authorization
References:
- [OWASP Authorization Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authorization_Cheat_Sheet.html)

An user can have multiple roles that are required to access certain API endpoints. Even though roles are returned to a client with the access token, they are completely for display purposes (e.g. show admin options if the user is admin). Role authorization is always done on the server-side. Roles are always fetched from the database on every request to ensure that role changes apply instantly. An access token only contains user identifier that `UserRoleMiddleware` uses to fetch roles from the database and add then to the request context. API endpoint authorization is done only after roles have been added to the request context.

Users always have the least amount of roles required to complete their tasks (OWASP: Enforce Least Privileges). This means that if an user has to only complete ordinary user tasks then `UserRole.User` role will be the only role assigned to the user.

API endpoints that require authorization perform role authorization checks by default for every method. For example, `UserController` uses `[Authorize(Roles = UserRole.User)]` annotation to perform role check for every possible method. This means that you have to specifically state the methods that do not require authorization with `[AllowAnonymous]` annotation (OWASP: Deny by Default).

### User information management
TODO: Can only edit own information...

### User role management
TODO: Admins can change user roles...

### User file uploads
TODO: Upload files using secure principles (e.g. generate random file names)...

## Local development

### Server
Add the following secrets with .NET Core Secret Manager tool:
```
$ cd server
$ dotnet user-secrets set "Jwt:Secret" <JwtSecret>
$ dotnet user-secrets set "PowerUser:Username" <Username>
$ dotnet user-secrets set "PowerUser:Email" <Email>
$ dotnet user-secrets set "PowerUser:Password" <Password>
$ dotnet user-secrets set "PowerUser:Name" <Name>
$ dotnet user-secrets set "Smtp:Password" <Password>
```

Start the server:
```
$ dotnet run
```

### Client
TODO
