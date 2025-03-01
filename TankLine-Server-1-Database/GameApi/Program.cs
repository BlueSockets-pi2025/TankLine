using Microsoft.EntityFrameworkCore;
using GameApi.Data;

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
builder.Services.AddSwaggerGen();

// Configuring the connection to the PostgreSQL database
builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

// Force HTTPS redirection if not already done by Kestrel
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    // Activate Swagger in development mode for the API
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

// Mapping controllers
app.MapControllers();

// Launch the application
app.Run();