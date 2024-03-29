using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using UserAuthServer.Constants;
using UserAuthServer.Initialization;
using UserAuthServer.Interfaces;
using UserAuthServer.Middleware;
using UserAuthServer.Models;
using UserAuthServer.Services;
using UserAuthServer.Utils;

// Logger
ILogger logger = LoggerFactory.Create(builder => builder
    .SetMinimumLevel(LogLevel.Debug)
    .AddConsole())
    .CreateLogger<Program>();

// Helper function for database location
string GetDbPath(string dbName) =>
    System.IO.Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), dbName);

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

// Setup SQLite database
builder.Services.AddDbContext<UserAuthServerDbContext>(options => {
    options.UseSqlite($"Data Source={GetDbPath(builder.Configuration["Database"])}");
});

// Configure user options
builder.Services.Configure<IdentityOptions>(options => {
    // [SECURE] Set strong password requirements
    options.Password.RequiredLength = 8;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;

    // [SECURE] Email
    options.User.RequireUniqueEmail = true;
    // [SECURE] Confirmed email is required in login route!

    // User role claim options
    options.ClaimsIdentity.UserIdClaimType = TokenClaim.UserId;
    options.ClaimsIdentity.UserNameClaimType = TokenClaim.Username;
    options.ClaimsIdentity.RoleClaimType = TokenClaim.Role;
});

// Add user service
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<UserAuthServerDbContext>()
    .AddDefaultTokenProviders();

// Set token validation options
var tokenOptions = new TokenValidationParameters {
    IssuerSigningKey = AuthSignKeyFactory.CreateAuthSignKey(builder.Configuration["Jwt:Secret"]),
    ValidIssuer = builder.Configuration["Jwt:Issuer"],
    ValidAudience = builder.Configuration["Jwt:Audience"],
    ValidateIssuerSigningKey = true,
    // [SECURE] Validate issuer and audience
    ValidateIssuer = true,
    ValidateAudience = true,
    NameClaimType = TokenClaim.Username
    // [SECURE] User role claim is not defined here
};

builder.Services.Configure<TokenValidationParameters>(options => {
    options.IssuerSigningKey = tokenOptions.IssuerSigningKey;
    options.ValidIssuer = tokenOptions.ValidIssuer;
    options.ValidAudience = tokenOptions.ValidAudience;
    options.ValidateIssuerSigningKey = tokenOptions.ValidateIssuerSigningKey;
    options.ValidateAudience = tokenOptions.ValidateAudience;
    options.ValidateIssuer = tokenOptions.ValidateIssuer;
    options.NameClaimType = tokenOptions.NameClaimType;
});

// Add JWT authentication
builder.Services
    .AddAuthentication(options => {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options => {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = tokenOptions;
        options.Events = new JwtBearerEvents {
            // [SECURE] Set access token check
            OnTokenValidated = context => {
                if (context.Principal != null
                        && !TokenUtil.HasTokenTypeClaim(context.Principal.Claims, TokenType.Access)) {
                    logger.LogDebug("Authorization without access token");
                    context.Fail(new Exception("Authorization must be done with access token"));
                }

                return Task.CompletedTask;
            }
        };
    });

// Add custom services
builder.Services.AddSingleton<ITokenService, JwtService>(); // JWT for authentication/authorization
builder.Services.AddSingleton<IEmailConfirmService, EmailConfirmationSender>(); // Email service
builder.Services.AddScoped<IUserFileService, UserFileService>();

// [SECURE] Use bcrypt password hashing algorithm
builder.Services.AddSingleton<IPasswordHasher<User>, BCryptPasswordHasher<User>>();

// [SECURE] User role middleware for authorization -> Check user roles server-side
builder.Services.AddScoped<UserRoleMiddleware, UserRoleMiddleware>();

// Configure controllers to use JSON
builder.Services.Configure<MvcOptions>(options => {
    options.Filters.Add(new ConsumesAttribute("application/json"));
    options.Filters.Add(new ProducesAttribute("application/json"));
});

// Add Cross-origin resource sharing
builder.Services.AddCors();

// Setup API docs
builder.Services.AddSwaggerGen(options => {
    var version = "v1";
    options.SwaggerDoc(version, new Microsoft.OpenApi.Models.OpenApiInfo {
        Title = "User Auth Server",
        Version = version,
        Description = "User Auth Server API documentation made with OpenAPI & ReDoc."
    });

    var authName = "Bearer token";
    options.AddSecurityDefinition(authName, new OpenApiSecurityScheme {
        Description = "JWT based authorization with access token.",
        Name = "Authorization", // Header
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = authName
    });

    // Set authorization options
    options.OperationFilter<SecurityRequirementsOperationFilter>(true, authName);
    options.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
    options.SupportNonNullableReferenceTypes();

    // Set the comments path for the Swagger JSON.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// API documentation
app.UseSwagger();
app.UseReDoc(options => {
    options.DocumentTitle = "User Auth Server API";
    options.SpecUrl = "/swagger/v1/swagger.json";
    options.ConfigObject.HideDownloadButton = true;
    options.ConfigObject.ExpandResponses = "200,201";
});

// Build request pipeline
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<UserRoleMiddleware>(); // User role fetching happens right after authentication
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope()) {
    // Ensure that needed secret values are set
    if (app.Configuration["Jwt:Secret"] == null
            || app.Configuration["PowerUser:Username"] == null
            || app.Configuration["PowerUser:Email"] == null
            || app.Configuration["PowerUser:Password"] == null
            || app.Configuration["PowerUser:Name"] == null) {
        throw new InvalidOperationException("Necessary secrets not defined, check README.md for secrets!");
    }

    // Initialize user roles and users
    await UserInitializer.seedUserRoles(scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>());
    await UserInitializer.seedUsers(scope.ServiceProvider.GetRequiredService<UserManager<User>>(), app.Configuration);
}

app.Run();
