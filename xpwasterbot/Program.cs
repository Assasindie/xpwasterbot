using Discord;
using Discord.WebSocket;
using DotnetOsrsApiWrapper;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace xpwasterbot
{
    class UserXp
    {
        public string DiscordID;
        public DateTime LastUpdate;
        public PlayerInfo Player;
    }

    class Program
    {
        private static readonly RegistryKey RegKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        private readonly DiscordSocketClient _client;
        private Timer timer;

        private LinkedList<UserXp> UserXPList = new LinkedList<UserXp>();
        static Dictionary<string, ulong> UserList = new Dictionary<string, ulong>();

        public object MessageRecieved { get; }

        static void Main(string[] args)
        {
            RegKey.SetValue("XPWasterbot", "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\"");
            AddUsers();
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public static void AddUsers()
        {
            UserList.Add("player", 1);
        }

        public Program()
        {
            _client = new DiscordSocketClient();
            _client.Ready += ReadyAsync;
            _client.MessageReceived += MessageReceivedAsync;
            foreach (string name in UserList.Keys)
            {
                PlayerInfo Player = PlayerInfo.GetPlayerStats(name);
                int TotalXP = Player.Overall.Experience;
                UserList.TryGetValue(name, out ulong discordID);
                DateTime LastUpdate = DateTime.Now;

                if (File.Exists(Environment.CurrentDirectory + "\\" + name + ".json"))
                {
                    string json = File.ReadLines(Environment.CurrentDirectory + "\\" + name + ".json").Last();
                    LastUpdate = DateTime.Now;
                    UpdateJson(new UserXp() { DiscordID = discordID.ToString(), Player = Player, LastUpdate = DateTime.Now });
                }
                else
                {
                    UpdateJson(new UserXp() { DiscordID = discordID.ToString(), Player = Player, LastUpdate = LastUpdate });
                }
                UserXPList.AddLast(new UserXp() { DiscordID = discordID.ToString(), Player = Player, LastUpdate = LastUpdate });
                Console.WriteLine(DateTime.Now + " : " + "Added user " + discordID.ToString() + " with Osrs Name " + name + " with total XP " + TotalXP);
            }
            timer = new Timer(60000);
            timer.Elapsed += Timer_elapse;
            timer.AutoReset = true;
            timer.Start();
        }

        private void Timer_elapse(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine(DateTime.Now + " : " + "Checking for XP changes");
            foreach (UserXp user in UserXPList)
            {
                PlayerInfo Player = PlayerInfo.GetPlayerStats(user.Player.Name);
                int TotalXP = Player.Overall.Experience;
                if (user.Player.Overall.Experience != TotalXP)
                {
                    user.LastUpdate = DateTime.Now;
                    //assumes they have logged out because the highscores have updated
                    SendMessage(user);
                    Console.WriteLine(DateTime.Now + " : " + user.Player.Name + "s xp has changed by " + (TotalXP - user.Player.Overall.Experience));
                    user.Player = Player;
                    UpdateJson(user);
                }
            }
        }

        private void UpdateJson(UserXp user)
        {
            string json = JsonConvert.SerializeObject(user);
            using (StreamWriter sw = File.AppendText(Environment.CurrentDirectory + "\\" + user.Player.Name + ".json"))
            {
                sw.Write("\n" + json);
            }
            Console.WriteLine("Created file for " + user.Player.Name);
        }

        public async Task MainAsync()
        {
            await _client.LoginAsync(TokenType.Bot, "token");
            await _client.StartAsync();
            await _client.SetStatusAsync(UserStatus.Invisible);
            // Block the program until it is closed.
            await Task.Delay(-1);
        }

        private Task ReadyAsync()
        {
            Console.WriteLine(DateTime.Now + " : " + $"{_client.CurrentUser} is connected!");

            return Task.CompletedTask;
        }

        public Task SendMessage(UserXp user)
        {
            
            _client.GetGuild().GetTextChannel().SendMessageAsync("<@!" + user.DiscordID + "> is xp wasting 😴");
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            if (!(message is IUserMessage usermsg)) return;
            if (message.Content.Contains("!lastonline") && message.MentionedUsers.Count == 1 && message.Channel.Id == 1)
            {
                foreach (SocketUser user in message.MentionedUsers)
                {
                    foreach (UserXp xpuser in UserXPList)
                    {
                        if (xpuser.DiscordID == user.Id.ToString())
                        {
                            await message.Channel.SendMessageAsync(user.Username + "'s xp was last updated at " + xpuser.LastUpdate);

                        }
                    }
                }
            }
            if (message.Content.Contains("!lastgraph"))
            {
                await message.Channel.SendFileAsync(@"filelocation");
                await message.Channel.SendFileAsync(@"filelocation");
            }

            if (message.Content.Contains("!stats ") && message.MentionedUsers.Count == 1)
            {
                foreach (SocketUser user in message.MentionedUsers)
                {
                    UserXp PingedUser = UserXPList.Where(player => player.DiscordID == user.Id.ToString()).ToArray()[0];
                    await message.Channel.SendMessageAsync(PingedUser.Player.GetAllValuesToString());
                }
            }

            if (message.Content.Contains("!test"))
            {
                await message.Channel.SendMessageAsync(JsonConvert.SerializeObject(UserXPList.ElementAt(0)));
            }
        }
    }
}
