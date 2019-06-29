using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace xpwasterbot
{
    class GetHighscore
    {

        public static string GetTotalXP(string username)
        {
            
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://secure.runescape.com/m=hiscore_oldschool/index_lite.ws?player=" + username);
            StreamReader resReader = new StreamReader(req.GetResponse().GetResponseStream());
            string res = resReader.ReadToEnd();
            string[] ver = res.Split(',');
            string[] xp = ver[2].Split('\n');
            return xp[0];

        }
     }
}
