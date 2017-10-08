using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Korean_TV
{

    class Program
    {
        public static String token;
        static void Main(string[] args)
        {
            token = TheTVDB.login(getKey(), "kevinpark1217", "309A7C5AB609096B");
            
            string variety = "Variety";
            string news = "News";
            string drama = "Drama";

            Manage.scrap(Manage.getPath(variety, FolderType.Active));
            Manage.scrap(Manage.getPath(news, FolderType.Active));
            Manage.scrap(Manage.getPath(drama, FolderType.Active));

            Manage.remove(Manage.getPath(variety, FolderType.Contents));
            Manage.remove(Manage.getPath(news, FolderType.Contents));
            Manage.remove(Manage.getPath(drama, FolderType.Contents));

            Manage.move(Manage.getPath(variety, FolderType.Complete), Manage.getPath(variety, FolderType.Contents), 1);
            Manage.move(Manage.getPath(news, FolderType.Complete), Manage.getPath(news, FolderType.Contents), 1);
            Manage.move(Manage.getPath(drama, FolderType.Complete), Manage.getPath(drama, FolderType.Contents), 0);

            string varietyLink = @"/?m=bbs&bid=video&cat=예능%2F오락&p=";
            string newsLink = @"/?m=bbs&bid=video&cat=교양%2F시사&p=";
            string dramaLink = @"/?m=bbs&bid=drama&cat=한국드라마&p=";

            string username = "itanimulli";
            string password = "d20yi2kqexl1";

            String[,] creds = Website.login(username, password);
            Website.checkIn(creds);

            Website varietySite = new Website(variety, varietyLink, creds, 1);
            Website newsSite = new Website(news, newsLink, creds, 1);
            Website dramaSite = new Website(drama, dramaLink, creds, 0);
            
            if (varietySite.retrieve(1, 2))
                varietySite.download();
            if (newsSite.retrieve(1, 1))
                newsSite.download();
            if (dramaSite.retrieve(1, 1))
                dramaSite.download();

            Website.logout(creds);

            //Console.Read();
        }
        
        private static String getKey()
        {
            List<String> keys = new List<string> { "3B93AA4377EF6427", "CBDB4A364D00EC28", "5600F962B88AF44D", "37C4E8B68F2D3ACB", "8B6F308A1E238266" };

            Random random = new Random();
            int index = random.Next(keys.Count);
            return keys[index];
        }
    }
}
