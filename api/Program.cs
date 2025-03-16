var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddControllers(); 

var connectionString = builder.Configuration.GetConnectionString("TimescaleDb");
builder.Services.AddScoped<Database>(provider => new Database(connectionString));

var app = builder.Build(); 

app.UseCors("AllowAll");

if (!File.Exists("privateKey.pem") || !File.Exists("publicKey.pub"))
{
    KeyGen.GenerateKeyPair(); 
}

string agentIp = "localhost"; 
int agentPort = 12345; 
string username = "monitor"; 
int intervalInSeconds = 5; 

await MultiSshConnection.Instance.StartSendingRequests(agentIp, agentPort, username, intervalInSeconds);

app.MapControllers();
app.Run("http://localhost:9000");