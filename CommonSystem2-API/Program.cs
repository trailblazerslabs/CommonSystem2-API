using CommonSystem2_API.Configuration;
using CommonSystem2_API.DatabaseContext;
using CommonSystem2_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var cors = builder.Configuration.GetSection("Cors").Get<Cors>();
if (cors != null && cors.AllowedOrigins != null && (cors?.AllowedOrigins?.Length ?? 0) > 0)
{
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(b =>
        {
            b.WithOrigins(cors.AllowedOrigins)
                       .AllowAnyHeader()
                       .AllowAnyMethod();
        });
    });
}


Environment.SetEnvironmentVariable("BASEDIR", AppContext.BaseDirectory);

string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration.GetSection("Logging"))
    .WriteTo.File(logDir + "/log.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);
builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "API Documentation",
            Version = "v1.0",
            Description = ""
        });        
        options.ResolveConflictingActions(x => x.First());
    });
}

builder.Services.AddMvc().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.MaxDepth = 32; // fires exception when exceeding limit
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true; // disable case sensitivity for deserialization
    options.JsonSerializerOptions.PropertyNamingPolicy = null; // forces Pascal case
});

//builder.Services.AddDbContext<AppDbContext>(options =>
//               options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
//               sqlOptions =>
//               {
//                   sqlOptions.EnableRetryOnFailure(5);
//               }));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 21)), // Adjust to your MySQL version
        mySqlOptions =>
        {
            mySqlOptions.EnableRetryOnFailure(5);
        }));

// Configure Azure AD authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
 .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

//Configure JWT authentication
var jwtToken = builder.Configuration["Jwt:Secret"];
if (jwtToken != null)
{
    builder.Services.AddAuthentication("CredentialBasedAuth")
    .AddJwtBearer("CredentialBasedAuth", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtToken))
        };
    });
}
builder.Services.AddAuthorization();
// Configure other services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();  
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        var loggerErr = services.GetRequiredService<ILogger<Program>>();
        loggerErr.LogError(ex, "An error occurred while migrating the database.");
    }
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(settings =>
    {
        settings.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1.0");
        settings.OAuthUsePkce();
    });
}

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();