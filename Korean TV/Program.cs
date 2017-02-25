﻿using System;
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
            string varietyLink = @"https://twzoa.info/?m=bbs&bid=video&cat=예능%2F오락&p=";
            string newsLink = @"https://twzoa.info/?m=bbs&bid=video&cat=교양%2F시사&p=";
            string dramaLink = @"https://twzoa.info/?m=bbs&bid=drama&cat=한국드라마&p=";

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

            Website varietySite = new Website(variety, varietyLink, 3, 1);
            Website newsSite = new Website(news, newsLink, 2, 1);
            Website dramaSite = new Website(drama, dramaLink, 1, 0);

            if (varietySite.retrieve())
                varietySite.download();
            varietySite.logout();
            if (newsSite.retrieve())
                newsSite.download();
            newsSite.logout();
            if (dramaSite.retrieve())
                dramaSite.download();
            dramaSite.logout();

            //Console.Read();
        }
    }
}
