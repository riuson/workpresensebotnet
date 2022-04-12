using Microsoft.EntityFrameworkCore;
using ServerApp.ChatBot;
using ServerApp.Database;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(Environment.GetCommandLineArgs());

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.AddJsonFile(
    "appsettings.json",
    optional: false,
    reloadOnChange: true);
builder.Configuration.AddJsonFile(
    $"appsettings.{builder.Environment.EnvironmentName}.json",
    optional: true,
    reloadOnChange: true);

builder.Services.AddHostedService<TeleBotService>();
builder.Services.AddSingleton<IMessageHandler, MessageHandler>();
builder.Services.AddSingleton<IDatabase, Database>();
builder.Services.AddSingleton<IDataFormatter, DataFormatter>();
builder.Services.AddSingleton<IScheduledMessageRemover, ScheduledMessageRemover>();
builder.Services.AddHostedService<BackgroundChatProcessor>();
builder.Services.AddSingleton<IPinnedMessagesManager, PinnedMessagesManager>();

var configuration = builder.Configuration;
builder.Services.AddDbContext<ApplicationDbContext>(
    contextLifetime: ServiceLifetime.Singleton,
    optionsAction: options =>
        options.UseSqlite(configuration.GetConnectionString("DataFile2")));

builder.Services.AddSingleton<ITelegramBotClient, TelegramBotClient>(_ =>
    new TelegramBotClient(configuration.GetValue<string>("TelegramBotToken")));

var port = configuration.GetValue<int>("WebHook:Port");
builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(port));

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