# User Auth Server

**COMP.SEC.300 Secure Programming: Project**

## Content
- [Project description](#project-description)
- [API](#api)
- [Security aspects](#security-aspects)
- [Testing](#testing)
- [Future improvements](#future-improvements)
- [Local development](#local-development)

## Project description

**Tech stack:** C#, ASP.NET Core & Entity Framework Core

Main goal of this project is to implement a web server with the following features using secure programming principles:
- User management
- User authentication
- Role-based authorization
- File uploads

**These features provide a secure foundation for any application that needs these features.**

Lines of code that are meant to increase security are marked with code comments starting with `[SECURE]`. Secure implementations are reviewed in [Security aspects](#security-aspects) section.

## API

The server has a REST API and provides the following features:

| METHOD | URL | Description | Auth |
| :-----: | :----- | :----- | :-----: |
| `GET` | `/api-docs` | Generated ReDoc documentation | - |
| `GET` | `/user` | Get all users | - |
| `GET` | `/user/<id>` | Get specific user | - |
| `POST` | `/user` | Create new user | - |
| `PATCH` | `/user/<id>` | Update specific user | User |
| `DELETE` | `/user/<id>` | Delete specific user | User |
| `PATCH` | `/user/<id>/role` | Update specific user's roles | Admin |
| `POST` | `/auth/login` | Log in with user credentials (refresh token) | - |
| `POST` | `/auth/refresh` | Gain an access token from a refresh token | - |
| `POST` | `/auth/email-send-confirmation` | Send confirmation email | - |
| `GET` | `/auth/email-confirm` | Confirm user email | - |
| `GET` | `/file` | Get all file informations | - |
| `GET` | `/file/<id>` | Get specific file | - |
| `POST` | `/file/` | Add new file | User |
| `DELETE` | `/file/<id>` | Delete specific file | User |


## Security aspects
### .NET
References:
- [OWASP DotNet Security Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/DotNet_Security_Cheat_Sheet.html)

OWASP states that .NET Entity Framework is very effective against SQL injections. Secure password policy is also set to ASP.NET Core Identity options in `Program.cs`.

### User authentication
References:
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
- [OWASP Password Storage Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html)

OWASP states that passwords shorter than 8 characters are considered weak, so it is the minimum password length requirement when creating/updating an user. Passwords also require both case characters, digit and non-alphanumeric character.

ASP.NET Core `UserManager` uses PBKDF2 password hashing algorithm by default. OWASP suggests that [bcrypt](https://en.wikipedia.org/wiki/Bcrypt) would be a better alternative, so I ended up changing `UserManager`'s password hashing algorithm to bcrypt by implementing `BCryptPasswordHasher`. OWASP also states that the bcrypt work factor should be at least 10, so the password hasher uses 12.

This project used JWTs (JSON Web Tokens) for user authentication. Successful login grants the user a refresh token (expires in 3 h) that can then be used to get an access token (expires in 5 min) that can be used to access the API. Refresh tokens can't be used to access API resources. JWT issuer and audience fields are also populated and validated during authentication.

An user has to provide an email when creating the user account. The user has to confirm their email in order to login in.

### Role-based authorization
References:
- [OWASP Authorization Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authorization_Cheat_Sheet.html)

An user can have multiple roles that are required to access certain API endpoints. Even though roles are returned to a client with the access token, they are completely for display purposes (e.g. show admin options if the user is admin). Role authorization is always done on the server-side. Roles are always fetched from the database on every request to ensure that role changes apply instantly. An access token only contains user ID that `UserRoleMiddleware` uses to fetch roles from the database and add then to the request context. API endpoint authorization is done only after roles have been added to the request context.

Users always have the least amount of roles required to complete their tasks (OWASP: Enforce Least Privileges). This means that if an user has to only complete ordinary user tasks then `UserRole.User` role will be the only role assigned to the user.

API endpoints that require authorization perform role authorization checks by default for every method. For example, `UserController` uses `[Authorize(Roles = UserRole.User)]` annotation to perform role check for every possible method. This means that you have to specifically state the methods that do not require authorization with `[AllowAnonymous]` annotation (OWASP: Deny by Default).

Only users with admin role/privileges can change roles of another user.

### User information management
Users can only update their own information, which means that updating/deleting other's information is prevented. This is done by `UserController` checking that the requested user ID matches the ID in the provided access token.

### File uploads
References:
- [OWASP File Upload Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/File_Upload_Cheat_Sheet.html)

Users can use the API to upload files with `UserFileService`. Allowed file extensions are defined in `appsettings.json` and files without an allowed extension are not accepted (OWASP: List allowed extensions).

File name validation can also be completely ignores since all file names are replaced with random strings/IDs (OWASP: Filename Sanitization), which protects against path traversals. Users can only delete files that they have added themselves (user owns the resource).

Upload file size is also limited to 5 MB.

## Testing
The server API is tested with the following test cases to ensure security:

1. User information can be requested without authorization.
1. Users can log in with correct credentials.
1. Login with incorrect credentials fails.
1. Users must have confirmed email in order to log in.
1. Users can only update/delete their own information.
1. Only admins can update user roles.
1. Refresh tokens expire in 3 hours and access token in 5 minutes.
1. Refresh token can't be used to access the API.
1. Access token can't be used to gain another access token.
1. JWT issuer and audience are also validated.
1. All endpoints that require authorization can't be accessed without proper roles.
1. User role changes are imminent (roles are checked from the database on every request).
1. File information can be requested without authorization.
1. Users can only add/remove their own files.
1. SQL injections do not work.
1. Every request is validated (e.g. username and email are required to create new user).

Test cases were executed and validated using Postman and debugger.

## Future improvements
- Email could also be used to recover a forgotten password.
- JWT secrets could use rotation (during maintenance for example).

## Local development
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

You can also configure settings in `appsettings.json` (e.g. change email server).

Start the server:
```
$ dotnet run
```
