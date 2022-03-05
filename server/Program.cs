using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using UserAuthServer.Initialization;
using UserAuthServer.Interfaces;
using UserAuthServer.Models;
using UserAuthServer.Services;
using UserAuthServer.Utils;

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
    // Password options
    options.Password.RequiredLength = 4;
    options.Password.RequiredUniqueChars = 0;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;

    // Email
    options.User.RequireUniqueEmail = true;

    // Sign in
    options.SignIn.RequireConfirmedEmail = true;

    // User role claim options
    options.ClaimsIdentity.UserIdClaimType = UserClaim.UserId;
    options.ClaimsIdentity.UserNameClaimType = UserClaim.Username;
    options.ClaimsIdentity.RoleClaimType = UserClaim.Role;
});

// Add user service
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<UserAuthServerDbContext>()
    .AddDefaultTokenProviders();

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
        options.TokenValidationParameters = new TokenValidationParameters() {
            NameClaimType = UserClaim.Username,
            RoleClaimType = UserClaim.Role,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = false,
            IssuerSigningKey = AuthSignKeyFactory.CreateAuthSignKey(builder.Configuration["Jwt:Secret"])
        };
    });

// Add custom JWT service
builder.Services.AddScoped<ITokenService, JwtService>();

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
        Description = "JWT based authorization.",
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

app.UseHttpsRedirection();
app.UseAuthentication();
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
