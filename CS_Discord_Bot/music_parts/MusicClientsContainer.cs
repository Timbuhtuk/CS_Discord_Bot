using CS_Discord_Bot.Models;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using CS_Discord_Bot.Factories;
using Microsoft.EntityFrameworkCore;

namespace CS_Discord_Bot
{
    public class MusicClientsContainer : IAsyncDisposable
    {
#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
        public Dictionary<ulong, MusicClient> music_clients;
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.

        private IDbContextFactory<DiscordMusicDBContext> _db_context_factory;

        public MusicClientsContainer(IDbContextFactory<DiscordMusicDBContext> db_context_factory) {
            music_clients = new Dictionary<ulong, MusicClient>();
            this._db_context_factory = db_context_factory;
        }

        public async Task Fill() {
            music_clients = new Dictionary<ulong, MusicClient>();
            foreach (var guild in Program._client.Guilds){
                    using var _context = _db_context_factory.CreateDbContext();
                    var models_guild = _context.Guilds.FirstOrDefault(g => g.DiscordId == guild.Id);
                    music_clients.TryAdd(guild.Id, Program._service_provider.GetRequiredService<MusicClientFactory>().Create(models_guild));
            }
        }

        public async Task Update(SocketGuild guild) {
            if (!music_clients.ContainsKey(guild.Id)) {
                using var _context = _db_context_factory.CreateDbContext();
                var models_guild = _context.Guilds.FirstOrDefault(g => g.DiscordId == guild.Id);

                if (models_guild == null)
                {
                    models_guild = new Guild()
                    {
                        Name = guild.Name,
                        DiscordId = guild.Id,
                    };
                    _context.Guilds.Add(models_guild);
                    _context.SaveChanges();
                }
                music_clients.TryAdd(guild.Id, Program._service_provider.GetRequiredService<MusicClientFactory>().Create(models_guild));
            }
        }

        public async Task AnchorAsync(SocketCommandContext context)
        {
            await Update(context.Guild);
            MusicClient? client;
            music_clients.TryGetValue(context.Guild.Id, out client);
            await client!.SetAnchorAsync(context);
        }

        public async Task ClearAsync(SocketCommandContext context)
        {
            await Update(context.Guild);
            MusicClient? client;
            music_clients.TryGetValue(context.Guild.Id, out client);
            await client!.ClearAsync(context);
        }

        public async Task LeaveAsync(SocketCommandContext context)
        {
            await Update(context.Guild);
            MusicClient? client;
            music_clients.TryGetValue(context.Guild.Id, out client);
            await client!.LeaveAsync(context);
        }

        public async Task PauseAsync(SocketCommandContext context)
        {
            await Update(context.Guild);
            MusicClient? client;
            music_clients.TryGetValue(context.Guild.Id, out client);
            await client!.TogglePauseAsync(context);
        }

        public async Task PlayAsync(SocketCommandContext context, string query)
        {
            await Update(context.Guild);
            MusicClient? client;
            music_clients.TryGetValue(context.Guild.Id, out client);
            await client!.PlayAsync(context, query);
        }

        public async Task PlayAsync(SocketCommandContext context)
        {
            await Update(context.Guild);
            MusicClient? client;
            music_clients.TryGetValue(context.Guild.Id, out client);
            await client!.PlayAsync(context);
        }

        public async Task ResumeAsync(SocketCommandContext context)
        {
            await Update(context.Guild);
            MusicClient? client;
            music_clients.TryGetValue(context.Guild.Id, out client);
            await client!.TogglePauseAsync(context);
        }

        public async Task SkipAsync(SocketCommandContext context)
        {
            await Update(context.Guild);
            MusicClient? client;
            music_clients.TryGetValue(context.Guild.Id, out client);
            await client!.SkipAsync(context);
        }


        public async ValueTask DisposeAsync()
        {
            foreach (var client in music_clients)
            {
                await client.Value.DisposeAsync();
            }
            return;
        }
    }
}
