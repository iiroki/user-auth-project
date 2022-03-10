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
TODO: JWT...

### User role based authorization
TODO

### User information management
TOOD

### User role management
TODO

### User file uploads
TODO

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
