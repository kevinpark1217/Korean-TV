using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Korean_TV
{

    class Program
    {
        static void Main(string[] args)
        {
            string varietyLink = @"https://torrentkim5.net/bbs/s.php?k=720p-NEXT&b=torrent_variety&page=";
            string newsLink = @"https://torrentkim5.net/bbs/s.php?k=720p-NEXT&b=torrent_docu&page=";
            string dramaLink = @"https://torrentkim5.net/bbs/s.php?k=720p-NEXT&b=torrent_tv&page=";

            string variety = "Variety";
            string news = "News";
            string drama = "Drama";

            Manage.remove(Manage.getPath(variety, FolderType.Contents));
            Manage.remove(Manage.getPath(news, FolderType.Contents));
            Manage.remove(Manage.getPath(drama, FolderType.Contents));
            Manage.move(Manage.getPath(variety, FolderType.Complete), Manage.getPath(variety, FolderType.Contents), 1);
            Manage.move(Manage.getPath(news, FolderType.Complete), Manage.getPath(news, FolderType.Contents), 1);
            Manage.move(Manage.getPath(drama, FolderType.Complete), Manage.getPath(drama, FolderType.Contents), 0);

            Website varietySite = new Website(variety, varietyLink, 2, 1);
            Website newsSite = new Website(news, newsLink, 2, 1);
            Website dramaSite = new Website(drama, dramaLink, 2, 0);

            if (varietySite.retrieve())
                varietySite.download();
            if (newsSite.retrieve())
                newsSite.download();
            if (dramaSite.retrieve())
                dramaSite.download();
        }
    }
}
