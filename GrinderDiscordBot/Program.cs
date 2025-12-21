using Microsoft.Extensions.Options;
using Discord;
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.Extensions;
using Lavalink4NET.Players;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using GrinderDiscordBot.Commands;


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

        services.AddSingleton<MusicCommands>();

        // Our bot service
        services.AddHostedService<DiscordBotService>();
    });

await builder.Build().RunAsync();

public class DiscordBotService : BackgroundService
{
    private readonly DiscordSocketClient _client;
    private readonly IAudioService _audioService;
    private readonly MusicCommands _musicCommands;

    public DiscordBotService(DiscordSocketClient client, IAudioService audioService, MusicCommands musicCommands)
    {
        _client = client;
        _audioService = audioService;
        _musicCommands = musicCommands;
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

        if (!message.Content.StartsWith("!"))
            return;

        if (message.Content.StartsWith("!hello"))
            await message.Channel.SendMessageAsync("Hello there!");

        else if (message.Content == "!gay")
            await message.Channel.SendMessageAsync($"{message.Author.Mention} is gay!");

        else if (message.Content.StartsWith("!join"))
        {
            await _musicCommands.JoinVoiceChannelAsync(message);
        }

        else if (message.Content.StartsWith("!play"))
        {
            await _musicCommands.PlayMusicAsync(message);
        }

        Console.WriteLine($"Command received: {message.Content}");
    }

    private async Task JoinVoiceChannelAsync(SocketMessage message)
    {
        var user = message.Author as SocketGuildUser;
        if (user?.VoiceChannel == null)
        {
            await message.Channel.SendMessageAsync("You must join a voice channel!");
            return;
        }

        var player = await _audioService.Players.JoinAsync(user.VoiceChannel.Guild.Id, user.VoiceChannel.Id);
        await message.Channel.SendMessageAsync($"Joined {user.VoiceChannel.Name}!");
    }

    
}