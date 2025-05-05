using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CS_Discord_Bot.Handlers
{
    public class CommandHandler
    {
        protected readonly CommandService _commands;
        protected readonly DiscordSocketClient _client;
        protected readonly ServiceProvider _service_provider;

        public CommandHandler(CommandService commands, DiscordSocketClient client, ServiceProvider provider)
        {
            _commands = commands;
            _client = client;
            _service_provider = provider;
        }

        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            var modules_info = await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _service_provider);

            await Logs.AddLog($"Modules registered: {string.Join(", ", modules_info.Select(x => x.Name))}");
            await Logs.AddLog($"With commands: {string.Join(", ", modules_info.Select(x => string.Join(", ", x.Commands.Select(y => y.Name))))}");
            await Logs.AddLog("Command handler registered");
        }

        protected async Task HandleCommandAsync(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);

            if (message == null || message.Author.IsBot) return;

            int argPos = 0;
            if (message.HasStringPrefix(Program.app_config["command_tag"], ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _service_provider);
                if (!result.IsSuccess)
                {
                    await Logs.AddLog(result.ErrorReason, LogLevel.ERROR);
                }
                await message.DeleteAsync();
            }
        }
    }
}