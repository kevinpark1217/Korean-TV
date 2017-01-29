using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Korean_TV
{
    class Website
    {
        private string type;
        private string link;
        private int maxPage;
        private int naming;
        private List<Item> list;

        public Website(string type, string link, int maxPage, int naming)
        {
            this.type = type;
            this.link = link;
            this.maxPage = maxPage;
            this.naming = naming;
            list = new List<Item>();
        }

        public bool retrieve()
        {
            for (int page = 1; page <= maxPage; page++)
            {
                String data = website(link + page, null);
                if (data != null)
                    parse(data);
            }

            if (list.Count == 0)
                return false;
            return true;
        }

        private String website(String urlAddress, String phpsessid)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            if(phpsessid != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(new Cookie("PHPSESSID", phpsessid, "/", "torrentkim5.net"));
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);

                String data = readStream.ReadToEnd();
                response.Close();
                readStream.Close();
                return data;
            }
            return null;
        }

        private void parse(String data)
        {
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(data);

            HtmlNodeCollection contents = html.GetElementbyId("blist").SelectNodes(".//tr[@class='bg1']");
            foreach (HtmlNode element in contents)
            {
                HtmlNodeCollection title = element.SelectNodes(".//td[@class='subject']/a");
                if (title.Count == 1)
                    continue;
                Uri uri = new Uri(new Uri(link), WebUtility.HtmlDecode(title.ElementAt(1).GetAttributeValue("href", null)));
                details(uri.ToString());
            }
        }

        private string phpsessid()
        {
            var random = new Random();
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToLower();
            return new string(Enumerable.Repeat(chars, 26).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void details(string link)
        {
            String phpid = phpsessid();
            String data = website(link, phpid);

            if (data != null)
            {
                HtmlDocument html = new HtmlDocument();
                html.LoadHtml(data);

                HtmlNode torrentName = html.GetElementbyId("writeContents").SelectSingleNode(".//ul/li");
                HtmlNode torrentLink = html.GetElementbyId("file_table").SelectNodes(".//tr")[2].SelectSingleNode(".//a");
                String name = WebUtility.HtmlDecode(torrentName.InnerHtml);
                name = name.Substring(5, name.IndexOf("<li>") - 5).Trim();
                Uri uri = new Uri(new Uri(link), WebUtility.HtmlDecode(torrentLink.GetAttributeValue("href", null)));

                HtmlNode contents = html.GetElementbyId("writeContents").SelectSingleNode(".//ol");
                int depth = 0;
                while ((contents = contents.SelectSingleNode("li")) != null)
                    depth++;
                if (depth > 1 || !name.ToUpper().Contains("720P-NEXT"))
                    return;

                Item show = new Item(name, Manage.getPath(type, FolderType.Contents));
                show.phpsessid = phpid;
                show.torrent = uri.ToString();
                show.link = link;
                list.Add(show);
            }
        }

        public void download()
        {
            for (int i = 0; i < list.Count; i++)
            {
                Item show = list.ElementAt(i);
                int j;
                for (j = 0; j < i; j++)
                    if (list.ElementAt(j).Equals(show))
                        break;
                if (i != j)
                    continue;
                
                if(! Manage.check(show, Manage.getPath(type, FolderType.Contents), naming))
                    torrent(show);
            }
        }

        private void torrent(Item show)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(show.torrent);
            req.CookieContainer = new CookieContainer();
            req.CookieContainer.Add(new Cookie("PHPSESSID", show.phpsessid, "/", "torrentkim5.net"));
            req.Referer = show.link;
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream sr = resp.GetResponseStream();
            
            string file = show.getName(naming) + ".torrent";
            string torrent = Path.Combine(Manage.getPath(type, FolderType.Root), file);
            FileStream fstream = File.Create(torrent);
            sr.CopyTo(fstream);
            sr.Close();
            fstream.Close();

            string destination = Path.Combine(Manage.getPath(type, FolderType.Default), file);
            if (File.Exists(destination))
                File.Delete(destination);
            File.Move(torrent, destination);
        }

    }
}
