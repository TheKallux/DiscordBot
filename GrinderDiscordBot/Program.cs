using Discord;
using Discord.WebSocket;

public class program
{

    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    private static DiscordSocketClient _client;

    public static async Task Main()
    {
        _client = new DiscordSocketClient();

        _client.Log += Log;

        var token = File.ReadAllText("Discord_Token.txt");

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
        await Task.Delay(-1);
    }
}