using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace xpwasterbot
{
    class UserXp
    {
        public string DiscordID;
        public string OsrsName;
        public string OsrsXP;

        public UserXp(string DiscordID, string OsrsName, string OsrsXP)
        {
            this.DiscordID = DiscordID;
            this.OsrsName = OsrsName;
            this.OsrsXP = OsrsXP;
        }
    }

    class Program
    {
        private readonly DiscordSocketClient _client;
        private Timer timer;

        private LinkedList<UserXp> UserXPList = new LinkedList<UserXp>();
        static Dictionary<string, ulong> UserList = new Dictionary<string, ulong>();
    
        static void Main(string[] args)
        {
            AddUsers();
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public static void AddUsers()
        {
            UserList.Add("Assasindie", 132448673710866432);
        }

        public Program()
        {
            _client = new DiscordSocketClient();    
            _client.Ready += ReadyAsync;
            foreach(string name in UserList.Keys)
            {
                string TotalXP = GetHighscore.GetTotalXP(name);
                UserList.TryGetValue(name, out ulong discordID);
                UserXPList.AddLast(new UserXp(discordID.ToString(), name, TotalXP));
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
                string TotalXP = GetHighscore.GetTotalXP(user.OsrsName);
                if (user.OsrsXP != TotalXP)
                {
                    //assumes they have logged out because the highscores have updated
                    SendMessage(user);
                    Console.WriteLine(DateTime.Now + " : " + user.OsrsName + "s xp has changed by"  + (int.Parse(TotalXP) - int.Parse(user.OsrsXP)));
                    user.OsrsXP = TotalXP;
                }
            }
        }

        public async Task MainAsync()
        {
            await _client.LoginAsync(TokenType.Bot, "Token");
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

            _client.GetGuild(111111).GetTextChannel(11111).SendMessageAsync("<@!" + user.DiscordID + "> is xp wasting 😴");
            return Task.CompletedTask;
        }

    }
}