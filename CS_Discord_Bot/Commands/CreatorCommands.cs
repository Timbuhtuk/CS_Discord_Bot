using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS_Discord_Bot.Commands
{
    public class Creatorommands : ModuleBase<SocketCommandContext>
    {
        [Command("reboot", RunMode = RunMode.Sync)]
        [Summary("reboot all program")]
        public async Task Reboot()
        {
            await Context.Message.DeleteAsync();
            if(Context.User.Id == 509028138227859468)
                Program.RestartApplication();
        }
        
        [Command("execute", RunMode = RunMode.Async)]
        [Summary("reboot all program")]
        public async Task Execute(params string[] values)
        {
            string query = Context.Message.Content.Replace(Program.app_config["command_tag"]!+"execute " ,"");
            if(Context.User.Id == 509028138227859468)
            {
                try { 
                    Process process = Process.Start(query);
                    //await Logs.AddLog($"Execute called query:{query} ,process {process.ProcessName}");
                }
                catch(Exception ex)
                {
                    await Logs.AddLog(ex.Message,LogLevel.ERROR);
                }
            
            }
        }
    }
}
