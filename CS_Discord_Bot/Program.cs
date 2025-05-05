using CS_Discord_Bot.Commands;
using CS_Discord_Bot.Factories;
using CS_Discord_Bot.Handlers;
using CS_Discord_Bot.Models;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using EventHandler = CS_Discord_Bot.Handlers.EventHandler;

namespace CS_Discord_Bot
{
    public class Program
    {
#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
        public static DiscordSocketClient _client;
        public static CommandService _commands;
        public static CommandHandler _command_handler;
        public static ComponentHandler _component_handler;
        public static EventHandler _event_handler;
        public static ServiceProvider _service_provider;
        public static IConfigurationRoot app_config;
        private readonly DiscordMusicDBContext _db_context_factory;
        public static int _MAIN_THREAD;
#pragma warning restore CS8618


        public Program()
        {

            _MAIN_THREAD = Thread.CurrentThread.ManagedThreadId;
            app_config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appdata\\configuration.json", optional: false, reloadOnChange: true).Build();
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;


            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds |
                                 GatewayIntents.GuildMessages |
                                 GatewayIntents.GuildVoiceStates |
                                 GatewayIntents.MessageContent,
                MessageCacheSize = 1000
            };



            _service_provider = new ServiceCollection()
           .AddSingleton<DiscordSocketClient>(provider =>
           {
               var config = new DiscordSocketConfig
               {
                   GatewayIntents = GatewayIntents.Guilds |
                                    GatewayIntents.GuildMessages |
                                    GatewayIntents.GuildVoiceStates |
                                    GatewayIntents.MessageContent,
                   MessageCacheSize = 1000
               };

               var client = new DiscordSocketClient(config);
               client.Log += Logs.AddLog;
               return client;
           })
           .AddSingleton<CommandService>(provider =>
           {
               var commands = new CommandService();
               commands.Log += Logs.AddLog;
               return commands;
           })
           .AddSingleton<MusicCommands>()
           .AddSingleton<MusicClientsContainer>()

           .AddScoped<MusicClientFactory>()
           .AddDbContextFactory<DiscordMusicDBContext>(options => options.UseSqlServer(app_config["connection_string"]))

           .BuildServiceProvider();


            _client = _service_provider.GetRequiredService<DiscordSocketClient>();
            _commands = _service_provider.GetRequiredService<CommandService>();
            _command_handler = new CommandHandler(_commands, _client, _service_provider);
            _component_handler = new ComponentHandler(_client);
            _event_handler = new EventHandler(_client);
            _db_context_factory = new DiscordMusicDBContext();
        }
        public static async Task Main(string[] args) => await new Program().RunBotAsync();

        public async Task RunBotAsync()
        {
            using LogScope log_scope = new LogScope("Staring up", ConsoleColor.Green);

            await _command_handler.RegisterCommandsAsync();
            await _component_handler.RegisterComponentsAsync();
            await _event_handler.RegisterEventsAsync();

            await Logs.AddLog($"use_database: {app_config["use_database"]!}");
            await Logs.AddLog($"connection string: {app_config["connection_string"]!}");

            await _client.LoginAsync(TokenType.Bot, app_config["tokens:0"]);
            await _client.StartAsync();
            await UpdateDBGuilds();
            _ = DiscordMusicDBContext.ResolveMissingfilesInDB();

            _client.Ready += OnReady;

            log_scope.Dispose();
            await Task.Delay(-1);
        }
        public static void RestartApplication()
        {
            Process.Start(Process.GetCurrentProcess().MainModule!.FileName);
            Environment.Exit(0);
        }

        protected async Task UpdateDBGuilds()
        {
            using var _context = new DiscordMusicDBContext();
            foreach (var guild in _client.Guilds)
            {
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
            }
        }


        protected static async void OnProcessExit(object? sender, EventArgs e)
        {
            try
            {
                var musicClientsContainer = _service_provider.GetRequiredService<MusicClientsContainer>();
                await musicClientsContainer.DisposeAsync();
                await _client.StopAsync();
            }
            catch (Exception ex)
            {
                await Logs.AddLog($"Error during process exit: {ex.Message}", LogLevel.ERROR);
            }
            finally
            {
                _client.Dispose();
            }
        }

        public async Task OnReady()
        {
            await _service_provider.GetRequiredService<MusicClientsContainer>().Fill();
            await Logs.AddLog($"logged as {_client.CurrentUser.Username}", LogLevel.WARNING);

        }

    }

}
