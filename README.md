# User Auth Project
**COMP.SEC.300 Secure Programming: Project**

Features:
- TODO: User creation with email confirmation
- TODO: User information management
- TODO: Login with multi-factor authentication
- TODO: Active login management

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
```

Start the server:
```
$ dotnet run
```

### Client
TODO

## Server API
TODO
