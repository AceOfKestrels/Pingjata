using Discord;
using Discord.WebSocket;

namespace Pingjata.Service;

public class SlashCommandManager
{
    private readonly Dictionary<string, ApplicationCommandProperties> Commands = [];
    private bool _ready;
    private readonly ILogger<SlashCommandManager> Logger;
    private readonly DiscordSocketClient Client;

    public SlashCommandManager(ILogger<SlashCommandManager> logger, DiscordSocketClient client)
    {
        Logger = logger;
        Client = client;

        Client.Ready += OnReady;
    }

    public void RegisterSlashCommand(SlashCommandBuilder command)
    {
        if (_ready)
        {
            Logger.LogWarning("Trying to register command while bot is already in 'ready' state");

            return;
        }

        if (Commands.ContainsKey(command.Name))
        {
            Logger.LogWarning("Command with name {Name} already registered", command.Name);

            return;
        }

        Commands.Add(command.Name, command.Build());
    }

    private async Task OnReady()
    {
        _ready = true;

        try
        {
            await Client.BulkOverwriteGlobalApplicationCommandsAsync([..Commands.Values]);
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to create application commands: {Error}", e);
        }
    }

}