using CS_Discord_Bot.Models;
using Microsoft.EntityFrameworkCore;


namespace CS_Discord_Bot.Factories
{
    public class MusicClientFactory
    {
        private readonly IDbContextFactory<DiscordMusicDBContext> _contextFactory;

        public MusicClientFactory(IDbContextFactory<DiscordMusicDBContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public MusicClient Create(Guild guild)
        {
            return new MusicClient(guild, _contextFactory);
        }
    }
}
