using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace CS_Discord_Bot.Handlers
{
    public class ComponentHandler
    {
        protected readonly DiscordSocketClient _client;


        public ComponentHandler(DiscordSocketClient client)
        {
            _client = client;
        }

        public Task RegisterComponentsAsync()
        {
            _client.InteractionCreated += HandleComponentAsync;
            Logger.AddLog("Component handler registered");
            return Task.CompletedTask;
        }

        protected async Task HandleComponentAsync(SocketInteraction interaction)
        {
            if (interaction is SocketMessageComponent component)
            {
                MusicClient? music_client;
                Program._service_provider.GetRequiredService<MusicClientsContainer>().music_clients.TryGetValue(interaction.GuildId.Value, out music_client);
                if (music_client != null)
                {
                    music_client.music_view.HandleComponent(component);
                }
            }
        }
    }
}
