using Microsoft.EntityFrameworkCore;
using ServerApp.ChatBot;
using ServerApp.Database;

var builder = WebApplication.CreateBuilder(Environment.GetCommandLineArgs());

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.AddJsonFile(
    "appsettings.json",
    optional: true,
    reloadOnChange: true);
builder.Configuration.AddJsonFile(
    $"appsettings.{builder.Environment.EnvironmentName}.json",
    optional: true,
    reloadOnChange: true);

builder.Services.AddHostedService<TeleBotService>();
builder.Services.AddTransient<IMessageHandler, MessageHandler>();
builder.Services.AddTransient<IDatabase, Database>();

var configuration = builder.Configuration;
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(configuration.GetConnectionString("DataFile2")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
