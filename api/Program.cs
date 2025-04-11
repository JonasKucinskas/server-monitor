using System.Net.NetworkInformation;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp",
        policy => policy.WithOrigins("http://localhost:8080")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

builder.Services.AddControllers(); 

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "monitor",
            ValidAudience = "monitor",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey12345_superSecretKey12345")) 
                
        };
    });

builder.Services.AddAuthorization();


if (!File.Exists("privateKey.pem") || !File.Exists("publicKey.pub"))
{
    KeyGen.GenerateKeyPair(); 
}

var connectionString = builder.Configuration.GetConnectionString("TimescaleDb");
builder.Services.AddScoped<Database>(provider => new Database(connectionString));
builder.Services.AddHostedService<PingerService>();
builder.Services.AddHostedService<SystemInitService>();

var app = builder.Build(); 

app.UseCors("AllowVueApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run("http://localhost:9000");