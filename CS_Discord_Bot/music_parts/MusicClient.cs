using Discord.Audio;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Concurrent;
using System.Text;
using CS_Discord_Bot.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;



namespace CS_Discord_Bot
{
    
    public class MusicClient : IAsyncDisposable
    {
        
        #region fields

        private Guild guild;
        private readonly IDbContextFactory<DiscordMusicDBContext> _db_context_factory;
        public Models.Playlist playback_history { get; protected set; } = new Models.Playlist() { Name = "history", Id = -1 };
        public List<Models.Playlist>? saved_music { get; protected set; }
        public KeyValuePair<IVoiceChannel, Song>? current_song { get; protected set; }
        public IVoiceChannel? current_voice_channel { get; protected set; }

        public bool is_playing { get; protected set; }
        public bool is_paused { get; protected set; }
        public bool on_repeat { get; protected set; }
        private CancellationTokenSource SkipTokenSource { get; set; } = new CancellationTokenSource();

        public ConcurrentQueue<KeyValuePair<IVoiceChannel, Song>> music_queue { get; protected set; }

        public IAudioClient? audio_client { get; protected set; }

        protected Process? ffmpeg;

        public MusicView? music_view { get; protected set; }
        public IMessage? view_message { get; protected set; }

        public System.Threading.Timer MainLoop { get; protected set; }

        protected static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        #endregion

        public MusicClient(Guild guild,IDbContextFactory<DiscordMusicDBContext> factory)
        { 
            music_queue = new ConcurrentQueue<KeyValuePair<IVoiceChannel, Song>>();
            saved_music = new List<Models.Playlist>() { playback_history };
            music_view = new MusicView(this);

            is_playing = false;
            is_paused = false;
            on_repeat = false;

            this._db_context_factory = factory;
            this.guild = factory.CreateDbContext().Guilds.FirstOrDefault(g => g.DiscordId == guild.DiscordId)!;
            Logs.AddLog($"MusicClient created for guild {guild.Name}").Wait();

            MainLoop = new System.Threading.Timer(AfterConstruct, null, 0, 899000);
        }
        protected async void AfterConstruct(Object o)
        {
            await UpdateLikedMusicAsync();
            await RerenderMusicViewAsync();
        }
        
        public async Task ClearAsync(ICommandContext context)
        {
            await Logs.AddLog("ClearAsync called");
            using LogScope log_scope = new LogScope();

            music_queue.Clear();
            await LeaveAsync(context);
            is_paused = false;
            is_playing = false;
            current_song = null;
            if (ffmpeg != null)
            {
                ffmpeg.Kill();
            }
        }
        public async Task SkipAsync(ICommandContext? context = null)
        {
            await Logs.AddLog("SkipAsync called ");
            if (on_repeat)
            {
                KeyValuePair<IVoiceChannel, Song> song;
                music_queue.TryDequeue(out song);
            }
            if (is_playing)
            {
                SkipTokenSource.Cancel();
                SkipTokenSource = new CancellationTokenSource();
            }
            is_paused = false;
            is_playing = false;
            current_song = null;
        }
        public async Task LeaveAsync(ICommandContext? context = null)
        {
            await Logs.AddLog("LeaveAsync called");
            using LogScope log_scope = new LogScope();

            if (audio_client != null)
            {
                await audio_client.StopAsync();
                audio_client = null;
            }
        }
        public async Task JoinAsync(IVoiceChannel channel)
        {
            await Logs.AddLog($"JoinAsync to voice channel {channel.Name} called ");

            if (audio_client != null && audio_client.ConnectionState == ConnectionState.Connected && current_voice_channel != channel)
            {
                await audio_client.StopAsync();
                audio_client = await channel.ConnectAsync();
                current_voice_channel = channel;
            }
            else if (audio_client != null && audio_client.ConnectionState == ConnectionState.Connecting && current_voice_channel != channel)
            {
                Thread.Sleep(1000);
                await audio_client.StopAsync();
                audio_client = await channel.ConnectAsync();
                current_voice_channel = channel;
            }
            else if (audio_client != null && audio_client.ConnectionState == ConnectionState.Disconnecting)
            {
                Thread.Sleep(1000);
                audio_client = await channel.ConnectAsync();
                current_voice_channel = channel;
            }
            else
            {
                audio_client = await channel.ConnectAsync();
                current_voice_channel = channel;
            }

        }

        public async Task TogglePauseAsync(ICommandContext? context = null)
        {

            if (is_playing && !is_paused)
            {
                is_paused = true;
                is_playing = false;

                await Logs.AddLog("Paused");
            }
            else if (is_paused)
            {
                is_paused = false;
                is_playing = true;

                await Logs.AddLog("Resumed");
            }
        }
        public async Task PlayMusicAsync(CancellationToken cancellationToken)
        {
            using LogScope log_scope = new LogScope($"PlayMusicAsync called", ConsoleColor.DarkMagenta);
            if (!is_playing && !is_paused && music_queue.Count > 0)
            {
                KeyValuePair<IVoiceChannel, Song> song = music_queue.First();
                var voiceChannel = song.Key;
                var song_path = song.Value.FilePath;

                try
                {
                    await JoinAsync(voiceChannel);
                }
                catch (Exception e)
                {
                    await Logs.AddLog(e.Message, LogLevel.ERROR);
                    return;
                }

                using var pcmStream = File.OpenRead(song_path!);
                var output = audio_client?.CreatePCMStream(AudioApplication.Music, bufferMillis: 100);

                is_playing = true;
                is_paused = false;

                if (!on_repeat)
                {
                    music_queue.TryDequeue(out song);
                    current_song = song;
                    playback_history.Songs.Add(song.Value);
                }
                else
                {
                    current_song = music_queue.First();
                }

                await UpdateMusicViewAsync();
                await UpdatePlayHistoryAsync();

                try
                {
                    byte[] buffer = new byte[81920];
                    int bytesRead;

                    while ((bytesRead = await pcmStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        while (is_paused)
                        {
                            await Task.Delay(100);
                            
                        }
                        await output.WriteAsync(buffer, 0, bytesRead);
                        if (cancellationToken.IsCancellationRequested)
                            break;
                    }
                }
                catch (OperationCanceledException ex) {
                    Console.WriteLine($"CanceledException during playback, : {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during playback: {ex.Message}");
                }
                finally
                {
                    await output.FlushAsync();
                    current_song = null;
                    is_playing = false;
                    _ = PlayMusicAsync(SkipTokenSource.Token);
                }
                return;
            }
            else if (!is_playing && !is_paused && music_queue.Count == 0)
            {
                await UpdateMusicViewAsync();
                await LeaveAsync();
            }
        }


        /// <summary>
        /// PLay song provided in query in current voice channel
        /// </summary>
        /// <param name="voiceChannel">object representing voice channel</param>
        /// <param name="query">name of song or Url or file name </param>
        /// <param name="textChannel">object representing text channel, where command was called</param>
        /// <remarks>
        /// Find => Download (if needed) => Add to queue song by query
        /// </remarks>
        /// <returns>Task</returns>
        public async Task PlayAsync(IVoiceChannel? voiceChannel, int? song_id = null, int? playlist_id = null)
        {
            using LogScope log_scope = new LogScope($"PlayAsync called for ID - song:{song_id} | playlist:{playlist_id}", ConsoleColor.Magenta);

            if (voiceChannel == null)
            {
                await Logs.AddLog("User not in voice chat", LogLevel.WARNING);
                return;
            }

            using var _context = _db_context_factory.CreateDbContext();
            if (song_id != null) {
                var song = _context.Find<Song>(song_id);
                if (song == null)
                    return;

                var temp = new KeyValuePair<IVoiceChannel, Song>(voiceChannel, song);

                music_queue.Enqueue(temp);
            }
            if (playlist_id != null) {
                var playlist = _context.Playlists.Include(p => p.Songs).FirstOrDefault(p => p.Id == playlist_id);
                if (playlist == null)
                    return;
                foreach(Song s in playlist.Songs)
                {
                    var temp = new KeyValuePair<IVoiceChannel, Song>(voiceChannel, s);
                    music_queue.Enqueue(temp);
                }
            }
            await Logs.AddLog("Line len - " + music_queue.Count.ToString());

            await PlayMusicAsync(SkipTokenSource.Token);
            return;
        }
        public async Task PlayAsync(IVoiceChannel? voiceChannel, string query = "", IMessageChannel? textChannel = null)
        {
            using LogScope log_scope = new LogScope($"PlayAsync called for - {query}", ConsoleColor.Magenta);

            if (voiceChannel == null)
            {
                await Logs.AddLog("User not in voice chat", LogLevel.WARNING);
                if (textChannel != null)
                {
                    await textChannel.SendMessageAsync("user not in voice");
                }
                Logs.depth -= 1;
                return;
            }

            List<Song> songs;
            using var _context = _db_context_factory.CreateDbContext();
            var songs_db_instances = new List<Song>();


             
            var song = _context.Songs.Where(s => s.Name == query || s.Link == query).FirstOrDefault();
            var playlist = _context.Playlists.Include(p=>p.Songs).Where(p => p.Name == query).FirstOrDefault();
                if (song != null)
                {
                    songs = new List<Song>() { song };
                    await Logs.AddLog("used saved track instead downloading");
                }
                else if (playlist != null) {
                    songs = new List<Song>();
                    foreach(var s in playlist.Songs)
                    {
                        songs.Add(s);
                    }
                }
                else
                {
                    var songs_info = await VideoFinder.Find(query, true);
                    if (songs_info == null)
                    {
                        if (textChannel != null)
                        {
                            await textChannel.SendMessageAsync(embed: new EmbedBuilder()
                                .WithDescription("q cant find that trash, мой маленький гой")
                                .WithColor(Color.Orange)
                                .Build()
                            );
                        }
                    Logs.depth -= 1;
                    return;
                    };
                    
                    songs = await AudioDownloader.Download(songs_info, Program.app_config["music_client:music_folder"]);
                
                }
                    foreach (var s in songs)
                    {  
                        var inst = _context.Songs.FirstOrDefault(S => s.Name == S.Name && s.AuthorName == S.AuthorName);      
                        if (inst == null)
                        { 
                            songs_db_instances.Add(_context.Songs.Add(s).Entity); 
                        }  
                        else {
                            songs_db_instances.Add(inst); 
                            
                        }
                    
                    }
                     
                    _context.SaveChanges();
                
                
            


            foreach (var s in songs_db_instances)
            {
                var temp = new KeyValuePair<IVoiceChannel, Song>(voiceChannel, s);
                music_queue.Enqueue(temp);
            }

            await Logs.AddLog("Line len - " + music_queue.Count.ToString());

            await PlayMusicAsync(SkipTokenSource.Token);
            return;
        }
        public async Task PlayAsync(ICommandContext ctx, string query)
        {
            await PlayAsync((ctx.User as IGuildUser).VoiceChannel, query, ctx.Channel as ITextChannel);
        }
        public async Task PlayAsync(ICommandContext ctx)
        {
            await TogglePauseAsync(ctx);
        }
        //protected async Task<Stream> CreateStream(string filePath)
        //{
        //    using LogScope log_scope = new LogScope($"CreateStream called", ConsoleColor.DarkGreen);

        //    var ffmpegPath = Path.Combine(Environment.CurrentDirectory, "appdata", "ffmpeg.exe");

        //    var processStartInfo = new System.Diagnostics.ProcessStartInfo
        //    {
        //        FileName = ffmpegPath,
        //        Arguments = $"-i \"{filePath}\" -f s16le -ar 48000 -ac 2 pipe:1",
        //        // -i {filePath}   -> Input audio file
        //        // -f s16le        -> 16-bit PCM (little-endian)
        //        // -ar 48000       -> Sample rate 48 kHz
        //        // -ac 2           -> Stereo (2 channels)
        //        // pipe:1          -> Output to stdout (for streaming)
        //        UseShellExecute = false,
        //        RedirectStandardOutput = true,
        //        RedirectStandardError = true,
        //        CreateNoWindow = true
        //    };

        //    ffmpeg = new System.Diagnostics.Process
        //    {
        //        StartInfo = processStartInfo,
        //        EnableRaisingEvents = true
        //    };

        //    ffmpeg.Exited += async (sender, args) =>
        //    {
        //        await Logs.AddLog("FFMPEG - ended");
        //        is_playing = false;
        //        _ = OnPlayMusicRequired.Invoke();
        //        ffmpeg?.Dispose();
        //    };

        //    ffmpeg.ErrorDataReceived += (sender, e) =>
        //    {
        //        //if (!string.IsNullOrEmpty(e.Data))
        //        //    Logs.AddLog($"FFMPEG Error: {e.Data}").Wait();
        //    };

        //    if (!ffmpeg.Start())
        //    {
        //        await Logs.AddLog("FFMPEG STARTUP ERROR",LogLevel.ERROR);
        //        throw new Exception("FFMPEG STARTUP ERROR");
        //    }

        //    ffmpeg.BeginErrorReadLine();
        //    await Logs.AddLog("FFMPEG - started");

        //    return ffmpeg.StandardOutput.BaseStream;
        //}
        public async Task RerenderMusicViewAsync(IMessageChannel? channel = null, IMessage? new_msg = null)
        {
            await _semaphoreSlim.WaitAsync();
            await Logs.AddLog("View render called");
            using LogScope log_scope = new LogScope();

            try
            {
                if (view_message != null)
                {
                    SocketTextChannel current_channel = (SocketTextChannel)view_message.Channel;

                    if (new_msg == null || (new_msg.Author.Id == Program._client.CurrentUser.Id && new_msg.Content == "."))
                    {
                        return;
                    }


                    if (view_message.Id != new_msg.Id)
                    {
                        await view_message.DeleteAsync();
                        view_message = await current_channel.SendMessageAsync(text: ".", options: new RequestOptions { RetryMode = RetryMode.RetryRatelimit });
                    }
                }
                else if (guild.Anchor != null)
                {
                    var current_channel = (await Program._client.GetChannelAsync(guild.Anchor.Value) as IMessageChannel)!;
                    view_message = await current_channel.SendMessageAsync(text: ".", allowedMentions: AllowedMentions.None);
                }
                else if (channel != null)
                {
                    view_message = await channel.SendMessageAsync(text: ".");
                }
            }
            catch (Exception e)
            {
                await Logs.AddLog(e.Message, LogLevel.ERROR);
            }
            finally
            {
                _semaphoreSlim.Release();
                await UpdateMusicViewAsync();
            }
        }
        public async Task<bool> UpdateMusicViewAsync()
        {

            await _semaphoreSlim.WaitAsync();
            await Logs.AddLog("View update called");
           

            Discord.Embed? embed = null;
            if (music_queue.Count > 0 || current_song != null)
            {
                embed = new EmbedBuilder()
                    .WithDescription(GetQueue())
                    .WithColor(Color.Orange)
                    .WithAuthor("---LINE--------------------------------------------------",
                    is_playing ? "https://media0.giphy.com/media/v1.Y2lkPTc5MGI3NjExMHpuNm8ycXdiaTRkem81ZHN6M2w5MDdibnVrNmZ3MHhxNjIwa3VyNCZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9cw/vJHNq9tziq9C3HMrIB/giphy.gif" : "")
                    .Build();
            }

            var component = await music_view!.CreateComponent();
            try
            {
                if (view_message == null)
                    return false;
                await (view_message as IUserMessage)!.ModifyAsync(msg => { msg.Components = component; msg.Embed = embed; msg.Content = ""; });
                
            }
            catch (Exception e)
            {
                using LogScope log_scope = new LogScope();
                await Logs.AddLog("View update failure", LogLevel.WARNING);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
            return true;
        }
        public async Task SetViewMessage(SocketMessage? msg) {
            view_message = msg;
        }
        public async Task SetAnchorAsync(ICommandContext context)
        {
            using var _context = _db_context_factory.CreateDbContext();
            _context.Attach(guild);
            guild.Anchor = context.Channel.Id;
            await Logs.AddLog($"Anchor for {guild.Name} is now {context.Channel.Name}");

            await _context.SaveChangesAsync();
        }
        protected async Task UpdatePlayHistoryAsync()
        {
            if (current_song == null)
                return;

            var history_limit = 10; //0 - 24
            var song = current_song.Value.Value;
            if (!playback_history.Songs.Select(s => s.Id).Contains(song.Id))
            {
                playback_history.Songs.Add(song);
            };
            while(playback_history.Songs.Count > history_limit)
            {
                playback_history.Songs.Remove(playback_history.Songs.Last());
            }
            await Logs.AddLog("Playback updated");
        }
        
        public async Task<bool> ToggleMusicLikeAsync(int? playlistId,int? songId = null)
        {
            songId = current_song?.Value.Id;

            if (songId == null || playlistId == null)
                return false;

            DiscordMusicDBContext _context = await _db_context_factory.CreateDbContextAsync();
            Models.Playlist? playlist = _context.Playlists.Include(p => p.Songs).FirstOrDefault(p => p.Id == playlistId);
            Song? song = _context.Songs.FirstOrDefault(s => s.Id == songId);

            if (playlist == null || song == null)
                return false;

                if (playlist.Songs.Contains(song))
                {
                    playlist.Songs.Remove(song);
                    await Logs.AddLog($"{song.Name} removed from favorite on guild {guild.DiscordId}");
                }
                else if (playlist.Songs.Count==23)//limit 25, -2 reserved options in every selector
                {
                    await Logs.AddLog($"{song.Name} did not add to favorite on guild {guild.DiscordId} to much favorite");
                    return false;
                }                
                else
                {
                  playlist.Songs.Add(song);
                  await Logs.AddLog($"{song.Name} added to favorite on guild {guild.DiscordId}");
                }
            
            _context.SaveChanges();
            
            return true;
        }
        public async Task<bool> AddPlaylistAsync(string? playlist_name, ulong? author_id)
        {
            await Logs.AddLog($"AddPlaylistAsync called, playlist name: {playlist_name}");
            using LogScope log_scope = new LogScope();

            if (playlist_name == null || author_id == null)
                return false;

            using var _context = await _db_context_factory.CreateDbContextAsync();


            bool playlistExists = guild.Playlists.Select(p => p.Name).Any(name => name == playlist_name);

            if (playlistExists)
                return false;

            Models.Playlist playlist_entity;

            var found_songs = await VideoFinder.FindPlaylistByLink(playlist_name, true);
            if (found_songs != null)
            {
                var song_list = found_songs?.Take(23).ToList() ?? new List<Song>();

                var downloaded_song_list = await AudioDownloader.Download(song_list, Program.app_config["music_client:music_folder"]);

                var song_db_instances = new List<Song>();

                foreach (var s in downloaded_song_list)
                {
                    var inst = _context.Songs.FirstOrDefault(S => s.Name == S.Name && s.AuthorName == S.AuthorName);
                    if (inst == null)
                    {
                        song_db_instances.Add(_context.Songs.Add(s).Entity);
                    }
                    else
                    {
                        song_db_instances.Add(inst);
                    }

                }

                playlist_entity = new Models.Playlist
                {
                    Name = await VideoFinder.GetPlaylistName(playlist_name),
                    Songs = song_db_instances,
                    AuthorId = author_id.Value
                };
            }
            else
            {
                playlist_entity = new Models.Playlist
                {
                    Name = playlist_name,
                    AuthorId = author_id.Value
                };
            }
            await _context.Playlists.AddAsync(playlist_entity);

            guild = _context.Guilds.FirstOrDefault(g => g.Id == guild.Id)!;

            guild.Playlists.Add(playlist_entity);

            await _context.SaveChangesAsync();

            await UpdateLikedMusicAsync();
            await UpdateMusicViewAsync();
            return true;
        }

        public async Task<bool> RemovePlaylistAsync(int playlist_id)
        {
            await Logs.AddLog($"RemovePlaylistAsync called, playlist id: {playlist_id}");
            using LogScope log_scope = new LogScope();

            using var _context = _db_context_factory.CreateDbContext();

            Models.Playlist? playlist = _context.Find<Models.Playlist>(playlist_id);

            if (playlist == null)
                return false;
            _context.Remove(playlist);
            _context.SaveChanges();
            await UpdateLikedMusicAsync();
            return true;
        }
        public async Task UpdateLikedMusicAsync() {

            using var _context = await _db_context_factory.CreateDbContextAsync();

            var playlists = await _context.Guilds
         .Where(g => g.Id == guild.Id)
         .SelectMany(g => g.Playlists)
         .Include(p => p.Songs) 
         .ToListAsync();

            saved_music = playlists;
            saved_music.Add(playback_history);
            await Logs.AddLog("Saved music updated");
        }
        public async Task ToggleRepeatAsync()
        {
            if (on_repeat)
            {
                KeyValuePair<IVoiceChannel, Song> song;
                music_queue.TryDequeue(out song);
                await Logs.AddLog("Repeat: off");
            }
            else
            {
                if (current_song != null)
                    music_queue.Enqueue(current_song.Value);
                await Logs.AddLog("Repeat: on");
            }

            this.on_repeat = !this.on_repeat;

        }

        public string GetQueue()
        {
            if (!music_queue.IsEmpty || current_song != null || (music_queue.Count == 1 && on_repeat))
            {
                StringBuilder result = new StringBuilder();
                int offset = on_repeat ? 1 : 0;
                var music_list = music_queue.ToList();

                for (int q = music_list.Count - 1; q >= offset; q--)
                {
                    var temp = music_list[q].Value.Name;
                    result.Append($"{(q + 2 - offset).ToString()} {temp}\n");
                }


                if ((is_playing || is_paused) && current_song != null)
                {
                    result.Append($"**1. {current_song.Value.Value.Name}**");
                }

                return result.ToString();
            }
            return "";
        }

        public async ValueTask DisposeAsync()
        {
            if(ffmpeg!=null)
                ffmpeg.Dispose();
            if(audio_client!=null)
                audio_client.Dispose();
            if ( view_message != null)
                await  view_message.DeleteAsync();
            return;
        }
    }
}
