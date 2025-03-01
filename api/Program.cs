using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


if (!File.Exists("privateKey.pem") || !File.Exists("publicKey.pub"))
{
    KeyGen.GenerateKeyPair(); // Generate keys on first run
}
SshConnection.ConnectToAgent("localhost", 12345, "monitor");

app.MapPost("/api/endpoint", async (HttpContext context) =>
{
    var data = await context.Request.ReadFromJsonAsync<object>();
    Console.WriteLine($"Received: {JsonSerializer.Serialize(data)}");
    return Results.Ok("Data received successfully!");
});


//on api
//gen ssh key
//pass it as parameter to monitor
//monitor copies it to ~/.ssh/authorized_keys
//


app.Run("http://localhost:9000");