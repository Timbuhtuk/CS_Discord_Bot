using CS_Discord_Bot;
using Discord;
using Discord.WebSocket;
using CS_Discord_Bot.Models;

/// <summary>
/// Class represents discord component creator for bot music player
/// </summary>
public class MusicView
{
    protected readonly MusicClient _music_client;
    protected CS_Discord_Bot.Models.Playlist current_playlist;

    public MusicView(MusicClient music_client)
    {
        _music_client = music_client;
        current_playlist = music_client?.saved_music?.Count > 0 ? music_client.saved_music[0] : new CS_Discord_Bot.Models.Playlist() { Name = "Your playlists" };
    }


    public async Task<MessageComponent> CreateComponent()
    {
        var builder = new ComponentBuilder();


        var songs_list_items = new List<SelectMenuOptionBuilder>();
        if(current_playlist.Songs.Count > 0)
        {
            songs_list_items.Add(new SelectMenuOptionBuilder("run all", "run all"));
            foreach (var song in current_playlist.Songs)
            {
                songs_list_items.Add(new SelectMenuOptionBuilder(song.Name, song.Id.ToString()));
            }
        }
        if(current_playlist.Name != "history")
            songs_list_items.Add(new SelectMenuOptionBuilder("delete list", "delete list"));


        var playlists_list_items = new List<SelectMenuOptionBuilder>();
        foreach (var playlist in _music_client.saved_music!)
        {
            if(!string.IsNullOrEmpty(playlist.Name))
                playlists_list_items.Add(new SelectMenuOptionBuilder(playlist.Name, playlist.Id.ToString()));
        }



        builder.WithButton(GetLikeButtonLabel(), "LIKE", GetLikeButtonStyle(), disabled: _music_client.current_song == null || !_music_client.current_song.HasValue || current_playlist.Id == -1);
        builder.WithButton(GetPauseUnpauseLabel(), "PAUSE_UNPAUSE", GetPauseUnpauseStyle());
        builder.WithButton("▶▶|", "FORWARD", GetForwardButtonStyle());
        builder.WithButton(GetRepeatButtonLabel(), "REPEAT", GetRepeatButtonStyle());
        builder.WithButton(GetAddPlaylistlabel(),"ADDPLAYLIST",GetAddPlaylistStyle());




        if (playlists_list_items.Count > 0)
            builder.WithSelectMenu("PLAYLIST_MENU", playlists_list_items, row: 1, placeholder: current_playlist.Name);

        if(songs_list_items.Count>0)
            builder.WithSelectMenu("SONGS_MENU", songs_list_items, row: 2, placeholder: "playlist content");
        

        return builder.Build();

    }

    

    protected string GetAddPlaylistlabel()
    {
        return "+new playlist";
    }
    protected ButtonStyle GetAddPlaylistStyle()
    {
        return ButtonStyle.Success;
    }
    protected string GetLikeButtonLabel()
    {
        if( _music_client.current_song == null || !_music_client.current_song.HasValue)
        {
            return "X";
        }

        Song song = _music_client.current_song.Value.Value;
        if (!current_playlist.Songs.Select(s=> s.Id).Contains(song.Id))
        {
            return "💚";
        }
        else
        {
            return "🤍";
        }
    }

    protected ButtonStyle GetLikeButtonStyle()
    {
        if (_music_client.current_song == null || !_music_client.current_song.HasValue)
        {
            return ButtonStyle.Secondary;
        }

        Song song = _music_client.current_song.Value.Value;
        if (!current_playlist.Songs.Select(s => s.Id).Contains(song.Id))
        {
            return ButtonStyle.Secondary;
        }
        else
        {
            return ButtonStyle.Success;
        }
    }

    protected string GetPauseUnpauseLabel()
    {
        return _music_client.is_playing ? "||" : "▶";
    }

    protected ButtonStyle GetPauseUnpauseStyle()
    {
        return _music_client.is_paused ? ButtonStyle.Danger : ButtonStyle.Primary;
    }

    protected ButtonStyle GetForwardButtonStyle()
    {
        return _music_client.music_queue.Count > 0 ? ButtonStyle.Primary : ButtonStyle.Secondary;
    }

    protected string GetRepeatButtonLabel()
    {
        return "⟳";
    }

    protected ButtonStyle GetRepeatButtonStyle()
    {
        return _music_client.on_repeat ? ButtonStyle.Success : ButtonStyle.Secondary;
    }


    public async Task HandleComponent(SocketMessageComponent component)
    {
        switch (component.Data.CustomId)
        {
            case "LIKE":
                await component.DeferAsync();
                bool toggle_result = await _music_client.ToggleMusicLikeAsync(current_playlist.Id);
                if(!toggle_result)
                {
                    await component.FollowupAsync(embed:new EmbedBuilder().WithDescription("limit of saved music is 25 tracks").WithColor(Color.Orange).Build(),ephemeral:true);
                }
                else {
                    await _music_client.UpdateLikedMusicAsync();
                    current_playlist = _music_client.saved_music.Where(p => p.Id == current_playlist.Id).First();
                }
                
   
                break;
            case "ADDPLAYLIST":
                var mb = new ModalBuilder()
                .WithTitle("Add playlist")
                .WithCustomId("ADDPLAYLISTMODAL")
                .AddTextInput(label:"Enter name for playlist or Url to playlist:", customId:"playlist_name", placeholder: "playlist name or url")
                .Build();
                await component.RespondWithModalAsync(mb);
                break;
            case "PAUSE_UNPAUSE":
                await component.DeferAsync();
                await _music_client.TogglePauseAsync();

                break;
            case "FORWARD":
                await component.DeferAsync();
                await _music_client.SkipAsync();

                break;
            case "REPEAT":
                await component.DeferAsync();
                await _music_client.ToggleRepeatAsync();

                break;
            case "SONGS_MENU":
                await component.DeferAsync();
                switch (component.Data.Values.First())
                {
                    case "run all":
                        await _music_client.PlayAsync((component.User as IGuildUser)?.VoiceChannel,playlist_id: current_playlist.Id);
                        break;
                    case "delete list":
                        await _music_client.RemovePlaylistAsync(current_playlist.Id);
                        current_playlist = _music_client.saved_music.First();
                        break;
                    default:
                        await _music_client.PlayAsync((component.User as IGuildUser)?.VoiceChannel,song_id: int.Parse(component.Data.Values.First()));
                        break;
                }
                await _music_client.UpdateMusicViewAsync();
                break;
            case "PLAYLIST_MENU":
                await component.DeferAsync();
                current_playlist = _music_client.saved_music.Where(p=>p.Id == int.Parse(component.Data.Values.First())).First();

                break;
        }
        await _music_client.UpdateMusicViewAsync();
    }
}
