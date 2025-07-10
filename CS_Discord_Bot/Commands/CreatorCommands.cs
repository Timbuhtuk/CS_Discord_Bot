using CS_Discord_Bot.Enums;
using Discord.Commands;
using System.Diagnostics;

namespace CS_Discord_Bot.Commands
{
    public class CreatorСommands : ModuleBase<SocketCommandContext>
    {
        [Command("reboot", RunMode = RunMode.Sync)]
        [Summary("reboot all program")]
        public async Task Reboot()
        {
            await Context.Message.DeleteAsync();

            string? owner_id_str = Program.app_config["owner_id"];
            if (owner_id_str != null)
            {
                ulong owner_id = ulong.Parse(owner_id_str);

                if (Context.User.Id == owner_id)
                    Program.RestartApplication();
            }
        }

        [Command("execute", RunMode = RunMode.Async)]
        [Summary("reboot all program")]
        public async Task Execute(params string[] values)
        {
            string? owner_id_str = Program.app_config["owner_id"];
            if (owner_id_str != null)
            {
                ulong owner_id = ulong.Parse(owner_id_str);


                string query = Context.Message.Content.Replace(Program.app_config["command_tag"]! + "execute ", "");
                if (Context.User.Id == owner_id)
                {
                    try
                    {
                        Process process = Process.Start(query);
                    }
                    catch (Exception ex)
                    {
                        await Logger.AddLog(ex.Message, LogLevel.ERROR);
                    }
                }
            }
        }
    }
}
