using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Register services before calling Build
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp",
        policy => policy.WithOrigins("http://localhost:8080") // Vue dev server
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

builder.Services.AddControllers(); // Registering controllers

var app = builder.Build(); // Build the application

// Use middleware after Build
app.UseCors("AllowVueApp");
app.MapControllers();

// Key generation logic
if (!File.Exists("privateKey.pem") || !File.Exists("publicKey.pub"))
{
    KeyGen.GenerateKeyPair(); // Generate keys on first run
}

// Connect to SSH agent

string agentIp = "localhost"; 
int agentPort = 12345; 
string username = "monitor"; 
int intervalInSeconds = 5; 

var sshConnection = new SshConnection(agentIp, agentPort, username);
sshConnection.Connect();
sshConnection.StartSendingRequests(intervalInSeconds);

app.MapPost("/api/endpoint", async (HttpContext context) =>
{
    var data = await context.Request.ReadFromJsonAsync<object>();
    Console.WriteLine($"Received: {JsonSerializer.Serialize(data)}");
    return Results.Ok("Data received successfully!");
});

app.Run("http://localhost:9000");