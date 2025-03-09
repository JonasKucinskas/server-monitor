var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddControllers(); 

var connectionString = builder.Configuration.GetConnectionString("TimescaleDb");
// Register Database service with connection string
builder.Services.AddScoped<Database>(provider => new Database(connectionString));

var app = builder.Build(); 

app.UseCors("AllowAll");

if (!File.Exists("privateKey.pem") || !File.Exists("publicKey.pub"))
{
    KeyGen.GenerateKeyPair(); 
}

// Connect to SSH agent
string agentIp = "localhost"; 
int agentPort = 12345; 
string username = "monitor"; 
int intervalInSeconds = 5; 

var sshConnection = new SshConnection(agentIp, agentPort, username);
sshConnection.Connect();
sshConnection.StartSendingRequests(intervalInSeconds);
app.MapControllers();
app.Run("http://localhost:9000");