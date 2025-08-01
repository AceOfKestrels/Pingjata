using Discord.WebSocket;
using Pingjata.Bot;
using Pingjata.Bot.EventHandlers.MessageHandlers;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
services.AddControllers();

services.AddSingleton(new DiscordSocketClient());
services.AddHostedService<DiscordBot>();
services.AddHostedService<PingHandler>();
services.AddHostedService<DmHandler>();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();