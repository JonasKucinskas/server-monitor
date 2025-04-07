using System.Net.NetworkInformation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddControllers(); 

if (!File.Exists("privateKey.pem") || !File.Exists("publicKey.pub"))
{
    KeyGen.GenerateKeyPair(); 
}

var connectionString = builder.Configuration.GetConnectionString("TimescaleDb");
builder.Services.AddScoped<Database>(provider => new Database(connectionString));
builder.Services.AddHostedService<PingerService>();
builder.Services.AddHostedService<SystemInitService>();

var app = builder.Build(); 

app.UseCors("AllowAll");
app.MapControllers();
app.Run("http://localhost:9000");