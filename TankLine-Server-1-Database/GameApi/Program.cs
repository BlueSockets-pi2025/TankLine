using Microsoft.EntityFrameworkCore;
using GameApi.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Load the Kestrel configuration from the appsettings.json file
builder.WebHost.ConfigureKestrel(options =>
{
    var kestrelConfig = builder.Configuration.GetSection("Kestrel");
    options.Configure(kestrelConfig); // Apply the Kestrel configuration from the appsettings.json file

    // Load the SSL certificate for HTTPS
    var certificatePath = builder.Configuration["Kestrel:Endpoints:Https:Certificate:Path"];
    var privateKeyPath = builder.Configuration["Kestrel:Endpoints:Https:Certificate:KeyPath"];
    var privateKeyPassword = builder.Configuration["Kestrel:Endpoints:Https:Certificate:Password"];

    if (string.IsNullOrEmpty(certificatePath) || string.IsNullOrEmpty(privateKeyPath) || string.IsNullOrEmpty(privateKeyPassword))
    {
        throw new ArgumentException("The path to the certificate, private key or private key password is missing from the configuration.");
    }

    var certLoader = new CertLoader();
    var certificate = certLoader.LoadCertificate(certificatePath, privateKeyPath, privateKeyPassword);

    options.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.ServerCertificate = certificate; // Apply the SSL certificate
    });
});

// Add the services required for the application
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

// Configuring the connection to the PostgreSQL database
builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add controllers :
builder.Services.AddControllers();

// JWT configuration : 
var jwtKey = builder.Configuration["JwtSettings:SecretKey"];

if (string.IsNullOrEmpty(jwtKey))
{
    throw new ArgumentNullException("JwtSettings:SecretKey", "JWT key is missing from the configuration.");
}

var jwtIssuer = builder.Configuration["JwtSettings:Issuer"];
var jwtAudience = builder.Configuration["JwtSettings:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true, // Validate the audience
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            NameClaimType = ClaimTypes.NameIdentifier,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)) // Guarantee that jwtKey is not null
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware to extract the token from the cookie and add it to the Authorization header
app.Use(async (context, next) =>
{
    var token = context.Request.Cookies["AuthToken"];
    if (!string.IsNullOrEmpty(token))
    {
        // Add the token to the Authorization header as a Bearer token
        context.Request.Headers["Authorization"] = $"Bearer {token}";
    }
    await next();
});

// Force HTTPS redirection if not already done by Kestrel
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    // Activate Swagger in development mode for the API
    //app.UseSwagger();
    //app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

// Mapping controllers
app.MapControllers();

// Launch the application
app.Run();