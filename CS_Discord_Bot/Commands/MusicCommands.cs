using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;


namespace CS_Discord_Bot.Commands
{
    /// <summary>
    /// Class that represents Music commands for bot
    /// </summary>
    public class MusicCommands : ModuleBase<SocketCommandContext>
    {
        public MusicClientsContainer container;

        public MusicCommands(MusicClientsContainer mcc)
        {
            container = mcc;
        }
        [Command("play", RunMode = RunMode.Async)]
        [Summary("Play the selected song from YouTube")]
        [Alias("p", "з")]
        public async Task PlayAsync([Remainder] string query)
        {
            await container.PlayAsync(Context, query);
        }
        [Command("play", RunMode = RunMode.Async)]
        [Summary("Play the selected song from YouTube")]
        [Alias("p", "з")]
        public async Task PlayAsync()
        {
            await container.PlayAsync(Context);
        }
        [Command("pause")]
        [Summary("Pause the current song")]
        [Alias("pa", "зф")]
        public async Task PauseAsync()
        {
            await container.PauseAsync(Context);
        }
        [Command("resume")]
        [Summary("Resume the current song")]
        [Alias("r", "к")]
        public async Task ResumeAsync()
        {
            await container.ResumeAsync(Context);
        }
        [Command("skip")]
        [Summary("Skip the current song")]
        [Alias("s", "ы")]
        public async Task SkipAsync()
        {
            await container.SkipAsync(Context);
        }
        [Command("clear")]
        [Summary("Clear the queue")]
        [Alias("c", "с")]
        public async Task ClearAsync()
        {
            await container.ClearAsync(Context);
        }
        [Command("leave")]
        [Summary("Disconnect bot from channel")]
        [Alias("l", "д")]
        public async Task LeaveAsync()
        {
            await container.LeaveAsync(Context);
        }
        [Command("anchor")]
        [Summary("Set current text chat as primary for bot")]
        [Alias("фтсрщк")]
        public async Task AnchorAsync()
        {
            await container.AnchorAsync(Context);
        }
    }
}