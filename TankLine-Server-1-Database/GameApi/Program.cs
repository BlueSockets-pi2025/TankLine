using Microsoft.EntityFrameworkCore;
using GameApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Charger la configuration Kestrel depuis le fichier appsettings.json
builder.WebHost.ConfigureKestrel(options =>
{
    var kestrelConfig = builder.Configuration.GetSection("Kestrel");
    options.Configure(kestrelConfig); // Appliquer la configuration Kestrel depuis le fichier appsettings.json

    // Charger le certificat SSL pour HTTPS
    var certificatePath = builder.Configuration["Kestrel:Endpoints:Https:Certificate:Path"];
    var privateKeyPath = builder.Configuration["Kestrel:Endpoints:Https:Certificate:KeyPath"];
    var privateKeyPassword = builder.Configuration["Kestrel:Endpoints:Https:Certificate:Password"];

    if (string.IsNullOrEmpty(certificatePath) || string.IsNullOrEmpty(privateKeyPath) || string.IsNullOrEmpty(privateKeyPassword))
    {
        throw new ArgumentException("Le chemin vers le certificat, la clé privée ou le mot de passe de la clé privée est manquant dans la configuration.");
    }

    var certLoader = new CertLoader();
    var certificate = certLoader.LoadCertificate(certificatePath, privateKeyPath, privateKeyPassword);

    options.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.ServerCertificate = certificate; // Appliquer le certificat SSL
    });
});

// Ajouter les services nécessaires à l'application
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurer la connexion à la base de données PostgreSQL
builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Ajouter les contrôleurs
builder.Services.AddControllers();

var app = builder.Build();

// Forcer la redirection HTTPS si ce n'est pas déjà fait par Kestrel
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    // Activer Swagger en mode développement pour l'API
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

// Mapper les contrôleurs
app.MapControllers();

// Lancer l'application
app.Run();
