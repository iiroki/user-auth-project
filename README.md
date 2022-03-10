# User Auth Project
**COMP.SEC.300 Secure Programming: Project**

- **Server:** C# & ASP.NET Core
- **Client:** TypeScript & React (?)

## Project
Main goal of this project is to implement the listed features using secure programming principles.

Features:
- User authentication
- User role based authorization
- User information management
- User role management
- User file uploads

Secure implementations are reviewed in [Security aspects](#security-aspects) chapter.

## Server API
TODO

## Security aspects
### User authentication
TODO: JWT, refresh tokens, access tokens, email confirmation (+ 2FA?)...

### User role based authorization
TODO: Users can have multiple roles, role authorization should be done server-side...

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
