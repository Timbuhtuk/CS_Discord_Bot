using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS_Discord_Bot.Commands
{
    public class CommonCommands : ModuleBase<SocketCommandContext>
    {
        [Command("help", RunMode = RunMode.Async)]
        [Summary("")]
        [Alias("h", "р")]
        public async Task HelpAsync(/*[Remainder] string query*/)
        {
            var embed = new EmbedBuilder().WithDescription(Program.app_config.GetSection("help_client")["help_message"]).Build();
            await Context.Channel.SendMessageAsync(embed: embed);
        }
    }
}
