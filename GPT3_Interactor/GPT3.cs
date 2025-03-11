using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace GPT3_Interactor
{
    public struct GPT3
    {
       
        
        public async static Task<string> PromptPython(string text, string user) {

          await Logs.AddLog($"PromptPython called with {text}, {user}");

            string pythonExePath = @"C:\Users\timpf\AppData\Local\Programs\Python\Python311\python.exe";
            string scriptPath = @"C:\Users\timpf\Desktop\Phyton\from_g4f.py";
            string parameter = "User: Here is the history of our conversation:\n---Start of History---\n" + await GetUserHistory(user) + "---End of History---\nUse history only if needed.\nNow my current question: " + text;

            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = pythonExePath;
            start.Arguments = $"\"{scriptPath}\" \"{parameter}\""; 
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.CreateNoWindow = true;
            start.RedirectStandardError = true;

            start.StandardOutputEncoding = Encoding.UTF8;
            start.StandardErrorEncoding = Encoding.UTF8;

            var process = new System.Diagnostics.Process { StartInfo = start };

            process.ErrorDataReceived += (sender, e) => {
                Logs.AddLog(e.Data,LogLevel.ERROR);
            };
            process.Exited += (sender, args) =>
            {
                process.Dispose();
            };

            process.Start();

            
           process.BeginErrorReadLine();

           using (StreamReader reader = process.StandardOutput)
           {
                    string result = reader.ReadToEnd();
                    Console.WriteLine("Output from Python script: " + result);

                try
                {
                    await AppendUserHistory(user, "User:" + text + "\nGPT:" + result);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.ResetColor();
                }

                return result;
           }
        }
        private async static Task<string> GetUserHistory(string user) {
             
            var path = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, user + ".json");
             
            if (!File.Exists(path))
            {
                File.Create(path).Close();
                await File.WriteAllTextAsync(path, "[]");
            }
             
            var all_replics = JsonConvert.DeserializeObject<List<string>>(await File.ReadAllTextAsync(path, System.Text.Encoding.UTF8));
            var output = ""; 
            if(all_replics != null)
                foreach(var replic in all_replics) output += replic + '\n';
            return output;
        }    
        private async static Task AppendUserHistory(string user, string text) {
            var path = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, user + ".json");
             
            if (!File.Exists(path))
            {
                File.Create(path).Close();
                await File.WriteAllTextAsync(path, "[]");
            }
             
            var all_replics = JsonConvert.DeserializeObject<List<string>>(await File.ReadAllTextAsync(path));
             
            if(all_replics == null) 
                all_replics = new List<string>();
             
            all_replics.Add(text);
            var len = 0;
            foreach (var str in all_replics)
            {
                len += str.Length;
            }
            
            while (len > 3900) {
                all_replics.Remove(all_replics[0]);
                len = 0;
                foreach (var str in all_replics) {
                    len += str.Length;           
                }
            }   

            File.WriteAllTextAsync(path, JsonConvert.SerializeObject(all_replics), System.Text.Encoding.UTF8);
        }
    }   
}   
