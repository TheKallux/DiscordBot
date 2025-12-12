using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Lavalink4NET.Extensions;
using Lavalink4NET;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Discord client
        services.AddSingleton<DiscordSocketClient>(sp => new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
        }));

        // Lavalink
        services.AddLavalink();

        // Our bot service
        services.AddHostedService<DiscordBotService>();
    });

await builder.Build().RunAsync();

public class DiscordBotService : BackgroundService
{
    private readonly DiscordSocketClient _client;
    private readonly IAudioService _audioService;

    public DiscordBotService(DiscordSocketClient client, IAudioService audioService)
    {
        _client = client;
        _audioService = audioService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client.Log += Log;
        _client.Ready += ReadyAsync;
        _client.MessageReceived += HandleMessageAsync;

        var token = File.ReadAllText("Discord_Token.txt");
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(-1, stoppingToken);
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    private Task ReadyAsync()
    {
        Console.WriteLine($"{_client.CurrentUser.Username} is ready!");
        return Task.CompletedTask;
    }

    private async Task HandleMessageAsync(SocketMessage message)
    {
        if (message.Author.IsBot)
            return;

        if (!message.Content.StartsWith("!hello"))
            await message.Channel.SendMessageAsync("Hello there!");

        else if (message.Content == "!gay")
            await message.Channel.SendMessageAsync($"{message.Author.Mention} is gay!");

        Console.WriteLine($"Command received: {message.Content}");
            

    }
}