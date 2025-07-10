using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace CS_Discord_Bot.Handlers
{
    public class EventHandler
    {
        protected readonly DiscordSocketClient _client;


        public EventHandler(DiscordSocketClient client)
        {
            _client = client;
        }

        public Task RegisterEventsAsync()
        {
            _client.MessageDeleted += MessageDeleted;
            _client.MessageReceived += MessageReceived;
            _client.ModalSubmitted += HandleModalAsync;

            Logger.AddLog("Event handler registered");
            return Task.CompletedTask;

        }
        protected async Task HandleModalAsync(SocketModal modal)
        {
            switch (modal.Data.CustomId)
            {
                case "ADDPLAYLISTMODAL":
                    await modal.DeferAsync();
                    List<SocketMessageComponentData> components = modal.Data.Components.ToList();
                    string playlist_name = components.First(x => x.CustomId == "playlist_name").Value;
                    MusicClient mc;
                    Program._service_provider.GetRequiredService<MusicClientsContainer>().music_clients.TryGetValue(modal.GuildId ?? 0, out mc);
                    Task.Run(() => mc!.AddPlaylistAsync(playlist_name, modal.User.Id));
                    break;
            }

        }
        protected async Task MessageDeleted(Cacheable<IMessage, ulong> cacheable1, Cacheable<IMessageChannel, ulong> cacheable2)
        {
            MusicClient music_client;
            Program._service_provider.GetRequiredService<MusicClientsContainer>().music_clients.TryGetValue((await cacheable2.GetOrDownloadAsync() as IGuildChannel).GuildId, out music_client);
            IMessage message = await cacheable1.GetOrDownloadAsync() as IMessage;
            if (music_client != null)
            {
                if (message != null && message.Id == music_client.view_message?.Id)
                {
                    await music_client.SetViewMessage(null);
                    await music_client.RerenderMusicViewAsync(new_msg: message);

                };

            };
        }

        protected async Task MessageReceived(SocketMessage message)
        {
            if (message == null) return;

            MusicClient music_client;
            Program._service_provider.GetRequiredService<MusicClientsContainer>().music_clients.TryGetValue((message.Channel as IGuildChannel).GuildId, out music_client);
            if (music_client != null)
            {
                if (message.Content.StartsWith(Program.app_config["command_tag"]!))
                {
                    return;
                }
                if (message.Author.Id == _client.CurrentUser.Id && message.Content == ".")
                {
                    await music_client.SetViewMessage(message);
                }
                else
                {
                    await music_client.RerenderMusicViewAsync(new_msg: message);
                }

            };

        }
    }
}
