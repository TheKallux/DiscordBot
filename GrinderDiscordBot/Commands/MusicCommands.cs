using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Options;

namespace GrinderDiscordBot.Commands;

public class MusicCommands
{

    private readonly IAudioService _audioService;

    public MusicCommands(IAudioService audioService)
    {
        _audioService = audioService;
    }


    public async Task JoinVoiceChannelAsync(SocketMessage message)
    {
        var user = message.Author as SocketGuildUser;

        if (user?.VoiceChannel == null)
        {
            await message.Channel.SendMessageAsync("You must be in a voice channel!");
            return;
        }

        var playerOptions = new LavalinkPlayerOptions();
        var retrieveOptions = new PlayerRetrieveOptions(PlayerChannelBehavior.Join);

        var result = await _audioService.Players.RetrieveAsync<LavalinkPlayer, LavalinkPlayerOptions> (
            user.Guild.Id,
            user.VoiceChannel.Id,
            playerFactory: PlayerFactory.Default,
            options: Options.Create(playerOptions),
            retrieveOptions: retrieveOptions);

        if (result.Player != null)
        {
            await message.Channel.SendMessageAsync($"Joined {user.VoiceChannel.Name}!");
        }
    }

    public async Task PlayMusicAsync(SocketMessage message)
    {
        var user = message.Author as SocketGuildUser;
        if (user?.VoiceChannel == null)
        {
            await message.Channel.SendMessageAsync("You must be in a voice channel!");
            return;
        }

        var query = message.Content.Substring(6);
        if (string.IsNullOrWhiteSpace(query))
        {
            await message.Channel.SendMessageAsync("Enter a song name!");
            return;
        }

        var playerOptions = new LavalinkPlayerOptions();
        var retrievePlayerOptions = new PlayerRetrieveOptions(PlayerChannelBehavior.Join);

        var result = await _audioService.Players.RetrieveAsync<LavalinkPlayer, LavalinkPlayerOptions>(
            user.Guild.Id,
            user.VoiceChannel.Id,
            playerFactory: PlayerFactory.Default,
            options: Options.Create(playerOptions),
            retrieveOptions: retrievePlayerOptions);

        var player = result.Player;
        if (player == null)
        {
            await message.Channel.SendMessageAsync("Unable to connect to voice!");
            return;
        }

        var track = await _audioService.Tracks.LoadTrackAsync(query, TrackSearchMode.YouTube);
        if (track == null)
        {
            await message.Channel.SendMessageAsync("Unable to find song!");
            return;
        }

        await player.PlayAsync(track);
        await message.Channel.SendMessageAsync($"Now playing: **{track.Title}** by {track.Author}");

    }

    /*public async Task PauseMusicAsync(SocketMessage message)
    {
        var user = message.Author as SocketGuildUser;

    }*/
}


