using Discord;
using Discord.WebSocket;
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
        public string OsrsName;
        public string OsrsXP;
        public DateTime LastUpdate;

        public UserXp(string DiscordID, string OsrsName, string OsrsXP, DateTime LastUpdate)
        {
            this.DiscordID = DiscordID;
            this.OsrsName = OsrsName;
            this.OsrsXP = OsrsXP;
            this.LastUpdate = LastUpdate;
        }
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
            //UserList.Add("Assasindie", 132448673710866432);
            UserList.Add("JZ-Cx", 195863984107159552);
            //UserList.Add("ironmanbtwbt", 195863984107159552);
            //UserList.Add("Lron PlsHop", 118668607193481217);
            //UserList.Add("Iron PlsHop", 132448673710866432);
            //UserList.Add("ayXises", 118668607193481217);
        }

        public Program()
        {
            _client = new DiscordSocketClient();    
            _client.Ready += ReadyAsync;
            _client.MessageReceived += MessageReceivedAsync;
            foreach(string name in UserList.Keys)
            {
                string TotalXP = GetHighscore.GetTotalXP(name);
                UserList.TryGetValue(name, out ulong discordID);
                DateTime LastUpdate = DateTime.Now;
                if (File.Exists(Environment.CurrentDirectory + "\\" + name + ".json"))
                {
                    string json = File.ReadLines(Environment.CurrentDirectory + "\\" + name + ".json").Last();
                    UserXp user = JsonConvert.DeserializeObject<UserXp>(json);
                    LastUpdate = user.LastUpdate;
                    if(user.OsrsXP != TotalXP)
                    {
                        LastUpdate = DateTime.Now;
                        UpdateJson(new UserXp(discordID.ToString(), name, TotalXP, DateTime.Now));
                    }
                    Console.WriteLine("Loaded last update time for " + name);
                } else
                {
                    UpdateJson(new UserXp(discordID.ToString(), name, TotalXP, LastUpdate));
                }
                UserXPList.AddLast(new UserXp(discordID.ToString(), name, TotalXP, LastUpdate));
                Console.WriteLine(DateTime.Now + " : " + "Added user " + discordID.ToString() + " with Osrs Name " + name + " with total XP " + TotalXP);
            }
            timer = new Timer(3600000);
            timer.Elapsed += Timer_elapse;
            timer.AutoReset = true;
            timer.Start();
        }

        private void Timer_elapse(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine(DateTime.Now + " : " + "Checking for XP changes");
            foreach (UserXp user in UserXPList)
            {
                string TotalXP = GetHighscore.GetTotalXP(user.OsrsName);
                if (user.OsrsXP != TotalXP)
                {
                    user.LastUpdate = DateTime.Now;
                    //assumes they have logged out because the highscores have updated
                    SendMessage(user);
                    Console.WriteLine(DateTime.Now + " : " + user.OsrsName + "s xp has changed by "  + (int.Parse(TotalXP) - int.Parse(user.OsrsXP)));
                    user.OsrsXP = TotalXP;
                    UpdateJson(user);
                }
            }
        }

        private void UpdateJson(UserXp user)
        {
            string json = JsonConvert.SerializeObject(user);
            using (StreamWriter sw = File.AppendText(Environment.CurrentDirectory + "\\" + user.OsrsName + ".json"))
            {
                sw.Write("\n" + json);
            }
            Console.WriteLine("Created file for " + user.OsrsName);
        }

        public async Task MainAsync()
        {
            await _client.LoginAsync(TokenType.Bot, "no leaky no token for u");
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

        public Task SendMessage(UserXp user) {
            //_client.GetGuild(518033583957606411).GetTextChannel(518033583957606413).SendMessageAsync("<@!" + user.DiscordID + "> is xp wasting 😴");
            _client.GetGuild(193290019950166017).GetTextChannel(594526085945753618).SendMessageAsync("<@!" + user.DiscordID + "> is xp wasting 😴");
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            if (!(message is IUserMessage usermsg)) return;
            if(message.Content.Contains("!lastonline") && message.MentionedUsers.Count == 1 && message.Channel.Id == 594526085945753618)
            {
                foreach(SocketUser user in message.MentionedUsers)
                {
                    foreach(UserXp xpuser in UserXPList)
                    {
                        if(xpuser.DiscordID == user.Id.ToString())
                        {
                            await message.Channel.SendMessageAsync(user.Username + "'s xp was last updated at " + xpuser.LastUpdate);

                        }
                    }
                }
            }
            if(message.Content.Contains("!lastgraph"))
            {
                await message.Channel.SendFileAsync(@"filelocation");
                await message.Channel.SendFileAsync(@"filelocation");
            }
        }
        }
}